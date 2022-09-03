namespace Backend.Models.JWT.Tokens
{
    public class JWTType
    {
        private JWTType(string value) { Value = value; }
        public string Value { get; private set; }

        public static JWTType ACCESS { get { return new JWTType("Access"); } }
        public static JWTType REFRESH { get { return new JWTType("Refresh"); } }
        public static JWTType MFA_ACCESS { get { return new JWTType("MFA Access"); } }
        public static JWTType MFA_OTP_REFRESH { get { return new JWTType("MFA OTP Refresh"); } }
        public static JWTType MFA_ACTIVATE { get { return new JWTType("MFA Activate"); } }
    }
}
