using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.Models.JWT.Util;

namespace Backend.Models.JWT.Tokens
{
    public abstract class JWT
    {
        protected SecurityTokenDescriptor tokenDescriptor;

        public JWT(RsaSecurityKey key)
        {
            tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256),
                Subject = new ClaimsIdentity()
            };
        }

        public JWT AddData(JWTData data)
        {
            tokenDescriptor.Subject.AddClaims(data.claims);
            return this;
        }

        public virtual string CreateToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
