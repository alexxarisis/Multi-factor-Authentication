using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Backend.Models.JWT.Tokens
{
    public class MFACodeRefreshJWT : JWT
    {
        public MFACodeRefreshJWT(RsaSecurityKey key) : base(key)
        {
            tokenDescriptor.Expires = DateTime.Now.AddMinutes(10);
            tokenDescriptor.Subject.AddClaims(new[]
            {
                 new Claim(ClaimTypes.Role, JWTType.MFA_OTP_REFRESH.Value)
            });
        }
    }
}
