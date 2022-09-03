using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Models.Security
{
    // maybe static?
    public class SecurePasswordGenerator
    {
        private readonly int passwordSize = 16;
        private readonly int saltSize = 16;

        // argon2 settings -> could be class or appsettings
        private readonly int degreeOfParallelism = 8;
        private readonly int iterations = 4;
        private readonly int memorySize = 1024;

        public byte[] CreateSalt()
        {
            return RandomNumberGenerator.GetBytes(saltSize);
        }

        public byte[] HashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2d(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = degreeOfParallelism,
                Iterations = iterations,
                MemorySize = memorySize
            };
            return argon2.GetBytes(passwordSize);
        }

        public async Task<byte[]> HashPasswordAsync(string password, byte[] salt)
        {
            var argon2 = new Argon2d(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = degreeOfParallelism,
                Iterations = iterations,
                MemorySize = memorySize
            };
            return await argon2.GetBytesAsync(passwordSize);
        }

        public async Task<byte[]> HashPasswordAsync(string password)
        {
            var argon2 = new Argon2d(Encoding.UTF8.GetBytes(password))
            {
                DegreeOfParallelism = degreeOfParallelism,
                Iterations = iterations,
                MemorySize = memorySize
            };
            return await argon2.GetBytesAsync(passwordSize);
        }

        public int CreateOneTimePassword()
        {
            return RandomNumberGenerator.GetInt32(100000,1000000);
        }
    }
}
