using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Backend.Models.JWT.Tokens
{
    public class AccessJWT : JWT
    {
        public AccessJWT(RsaSecurityKey key) : base(key)
        {
            tokenDescriptor.Expires = DateTime.Now.AddSeconds(60);
            tokenDescriptor.Subject.AddClaims(new[]
            {
                 new Claim(ClaimTypes.Role, JWTType.ACCESS.Value)
            });
        }
    }
}
