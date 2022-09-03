namespace Backend.Models.Services.MFA
{
    public interface IMFAService
    {
        public void SendVerificationCode(string email, int code);
    }
}
