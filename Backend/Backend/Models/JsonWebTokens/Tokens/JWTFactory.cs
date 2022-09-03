using Microsoft.IdentityModel.Tokens;
using Backend.Models.JWT.Util;

namespace Backend.Models.JWT.Tokens
{
    public class JWTFactory
    {
        private readonly Dictionary<string, JWT> tokens = new();

        public JWTFactory(RsaSecurityKey key)
        {
            tokens.Add(JWTType.ACCESS.Value, new AccessJWT(key));
            tokens.Add(JWTType.REFRESH.Value, new RefreshJWT(key));
            tokens.Add(JWTType.MFA_ACTIVATE.Value, new MFAActivateJWT(key));
            tokens.Add(JWTType.MFA_ACCESS.Value, new MFAAccessJWT(key));
            tokens.Add(JWTType.MFA_OTP_REFRESH.Value, new MFACodeRefreshJWT(key));
        }

        public string CreateJWT(JWTType type, JWTData data)
        {
            return tokens[type.Value].AddData(data).CreateToken();
        }
    }

}
