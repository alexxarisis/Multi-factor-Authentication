using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.User
{
    public class RegisterRequest
    {
        public string UserName { get;}
        [EmailAddress]
        public string Email { get;}
        public string Password { get;}

        [JsonConstructor]
        public RegisterRequest(string userName, string email, string password)
        {
            UserName = userName;
            Email = email;
            Password = password;
        }

        public bool IsInvalid()
        {
            return string.IsNullOrWhiteSpace(UserName) ||
                   string.IsNullOrWhiteSpace(Email) ||
                   string.IsNullOrWhiteSpace(Password);
        }
    }
}
