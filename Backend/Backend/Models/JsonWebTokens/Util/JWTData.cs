using System.Security.Claims;

namespace Backend.Models.JWT.Util
{
    public class JWTData
    {
        public List<Claim> claims { get;}

        public JWTData(int id)
        {
            claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, id.ToString()));
        }
    }
}
