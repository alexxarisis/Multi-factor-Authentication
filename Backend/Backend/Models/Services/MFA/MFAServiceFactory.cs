namespace Backend.Models.Services.MFA
{
    public enum MFAServiceType { EMAIL/*, SMS, PHONE*/}

    public class MFAServiceFactory
    {
        private readonly Dictionary<MFAServiceType, IMFAService> services;

        public MFAServiceFactory()
        {
            services = new Dictionary<MFAServiceType, IMFAService>();
            services.Add(MFAServiceType.EMAIL, new EmailService());
        }

        public IMFAService CreateService(MFAServiceType type)
        {
            return services[type];
        }
    }
}
