using System.IdentityModel.Tokens.Jwt;

namespace Backend.Models.JWT.Util
{
    public class JWTReader
    {
        private JwtSecurityToken _token;

        public JWTReader(HttpRequest request)
        {
            // Removing the Bearer prefix
            string jwt = request.Headers["Authorization"].ToString().Split(" ")[1];
            _token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        }

        public int GetID()
        {
            var value = _token.Claims.First(c => c.Type == "nameid").Value;

            return Convert.ToInt32(value);
        }
    }
}
