using Certes;

namespace ZoneUpdater.Services
{
    internal class AcmeContextProvider : IAcmeContextProvider
    {
        private readonly string _email;
        private readonly Uri _acmeServer;
        private readonly IConfiguration _configuration;

        public AcmeContextProvider(string email, Uri acmeServer, IConfiguration configuration)
        {
            _email = email;
            _acmeServer = acmeServer;
            _configuration = configuration;
        }

        public async Task<IAcmeContext> GetAcmeContextAsync()
        {
            var accountKey = _configuration["LetsEncrypt:AccountKey"];
            if (string.IsNullOrEmpty(accountKey))
            {
                var acme = new AcmeContext(_acmeServer);
                await acme.NewAccount(_email, true);
                return acme;
            }

            var key = KeyFactory.FromPem(accountKey);
            return new AcmeContext(_acmeServer, key);
        }
    }
}
