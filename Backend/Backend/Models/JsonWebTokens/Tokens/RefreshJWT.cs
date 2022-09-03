using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Backend.Models.JWT.Tokens
{
    public class RefreshJWT : JWT
    {
        public RefreshJWT(RsaSecurityKey key) : base(key)
        {
            tokenDescriptor.Expires = DateTime.Now.AddHours(1);
            tokenDescriptor.Subject.AddClaims(new[]
            {
                 new Claim(ClaimTypes.Role, JWTType.REFRESH.Value)
            });
        }
    }
}
