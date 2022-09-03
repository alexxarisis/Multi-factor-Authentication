using Newtonsoft.Json;

namespace Backend.Models.User
{
    public class LoginRequest
    {
        public string UserName { get;}
        public string Password { get;}

        [JsonConstructor]
        public LoginRequest(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public bool IsInvalid()
        {
            return string.IsNullOrWhiteSpace(UserName) ||
                   string.IsNullOrWhiteSpace(Password);
        }
    }
}
