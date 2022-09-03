using MySql.Data.MySqlClient;
using System.Data;

using Backend.Models.User;

namespace Backend.Models.Services
{
    public class DatabaseConnector
    {
        private readonly MySqlConnection conn;

        public DatabaseConnector()
        {
            var config = new ConfigurationBuilder().
                AddJsonFile("appsettings.json").Build();
           
            string server = config.GetSection("DbConnection")["Server"];
            string database = config.GetSection("DbConnection")["Database"];
            string username = config.GetSection("DbConnection")["Username"];
            string password = config.GetSection("DbConnection")["Password"];
            string port = config.GetSection("DbConnection")["Port"];

            try
            {
                conn = new MySqlConnection(
                    $"server={server};" +
                    $"database={database};" +
                    $"username={username};" +
                    $"password={password};" +
                    $"port={port};");

                conn.Open();
                if (conn.State == ConnectionState.Open)
                {
                    Console.WriteLine("Connected Successfully.");
                }
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Close();
                    Console.WriteLine("Database not connected.");
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        ~DatabaseConnector()
        {
            conn.Close();
        }

        public async Task<int> FindUserIdByUserNameAsync(string userName)
        {
            string query = "SELECT user_id FROM user WHERE username = @username"; 
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@username", userName)
            };
            return (int) (await CreateCommand(query, parameters).ExecuteScalarAsync() ?? 0);
        }

        public async Task<string> FindUserEmailByIdAsync(int id)
        {
            string query = "SELECT email FROM user WHERE user_id = @id";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@id", id),
            };
            return (string) (await CreateCommand(query, parameters).ExecuteScalarAsync() ?? "");
        }

        public async Task<bool> CreateAccountAsync(Account account)
        {
            string query = "INSERT INTO user(username, email, password, salt) " +
            "VALUES(@username, @email, @password, @salt)";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@username", account.UserName),
                new MySqlParameter("@email", account.Email),
                new MySqlParameter("@password", account.Password),
                new MySqlParameter("@salt", account.Salt)
            };
            return await CreateCommand(query, parameters).ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> DeleteAccountAsync(int id)
        {
            string query = "DELETE FROM user WHERE user_id=@id"; 
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@id", id),
            };
            return await CreateCommand(query, parameters).ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            string query = "SELECT * FROM user WHERE username = @username";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@username", username) 
            };
            using (var reader = await CreateCommand(query, parameters).ExecuteReaderAsync())
            {
                return reader.HasRows;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            string query = "SELECT * FROM user WHERE email = @email";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@email", email)
            };
            using (var reader = await CreateCommand(query, parameters).ExecuteReaderAsync())
            {
                return reader.HasRows;
            }
        }

        public async Task<bool> RegisterUserAndMFACodeAsync(int id, byte[] code)
        {
            string query = "INSERT INTO MFA (user_id, code) VALUES (@id, @code)";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@id", id),
                new MySqlParameter("@code", code)
            };
            return await CreateCommand(query, parameters).ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> StoreUserMFACodeAsync(int id, byte[] code)
        {
            string query = "UPDATE MFA SET code = @code " +
                           "WHERE user_id = @id";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@id", id),
                new MySqlParameter("@code", code)
            };
            return await CreateCommand(query, parameters).ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> StoreUserMFARecoveryCodeAsync(int id, byte[] recoveryCode)
        {
            string query = "UPDATE MFA SET recovery_code = @recoveryCode " +
                           "WHERE user_id = @id";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@id", id),
                new MySqlParameter("@recoveryCode", recoveryCode)
            };
            return await CreateCommand(query, parameters).ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> ChangeUserMFASettingsAsync(int id, bool value)
        {
            string query = "UPDATE user_settings SET value = @value " +
                           "WHERE user_id = @user_id && settings_id = @settings_id";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@user_id", id),
                new MySqlParameter("@settings_id", await FindMFASettingsIdAsync()),
                new MySqlParameter("@value", value)
            };
            return await CreateCommand(query, parameters).ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> MFACodesMatchAsync(int id, byte[] code)
        {
            string query = "SELECT * FROM MFA WHERE user_id = @id AND code = @code"; 
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@id", id),
                new MySqlParameter("@code", code)
            };
            using (var reader = await CreateCommand(query, parameters).ExecuteReaderAsync())
            {
                return reader.HasRows;
            }
        }

        public async Task<bool> MFARecoveryCodesMatchAsync(int id, byte[] recoveryCode)
        {
            string query = "SELECT * FROM MFA WHERE user_id = @id AND recovery_code = @recovery_code";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@id", id),
                new MySqlParameter("@recovery_code", recoveryCode)
            };
            using (var reader = await CreateCommand(query, parameters).ExecuteReaderAsync())
            {
                return reader.HasRows;
            }
        }

        public async Task<bool> CredentialsMatchAsync(string userName, byte[] password)
        {
            string query = "SELECT * FROM user WHERE username = @username " +
                "&& password = @password";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@username", userName),
                new MySqlParameter("@password", password)
            };
            using (var reader = await CreateCommand(query, parameters).ExecuteReaderAsync())
            {
                return reader.HasRows;
            }
        }

        public async Task<byte[]> GetUserSaltAsync(string userName)
        {
            string query = "SELECT salt FROM user WHERE username = @username";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@username", userName)
            };
            return (byte[]) (await CreateCommand(query, parameters).ExecuteScalarAsync() 
                             ?? Array.Empty<byte>());
        }

        public async Task<bool> UserHasEnabledMFAAsync(int userId)
        {
            string query = "SELECT * FROM user_settings " +
                "WHERE user_id = @user_id && " +
                "settings_id = @settings_id && " +
                "value = 1";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@user_id", userId),
                new MySqlParameter("@settings_id", await FindMFASettingsIdAsync())
            };
            using (var reader = await CreateCommand(query, parameters).ExecuteReaderAsync())
            {
                return reader.HasRows;
            }
        }

        public async Task<bool> UserHasRegisteredMFAAsync(int userId)
        {
            string query = "SELECT * FROM MFA WHERE user_id = @user_id";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@user_id", userId)
            };
            using (var reader = await CreateCommand(query, parameters).ExecuteReaderAsync())
            {
                return reader.HasRows;
            }
        }

        private async Task<int> FindMFASettingsIdAsync()
        {
            string query = "SELECT settings_id FROM settings " +
                "WHERE name = @name";
            List<MySqlParameter> parameters = new()
            {
                new MySqlParameter("@name", "two_factor_login")
            };
            return (int) (await CreateCommand(query, parameters).ExecuteScalarAsync() ?? 0);
        }


        private MySqlCommand CreateCommand(string query, List<MySqlParameter> parameters)
        {
            var command = new MySqlCommand(query, conn);
            foreach (MySqlParameter parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.ParameterName, parameter.Value);
            }
            return command;
        }
    }
}
