using System.Security.Cryptography.X509Certificates;
using ZoneUpdater.Services;

namespace ZoneUpdater.Clients
{
    public class LetsEncryptClient
    {
        private readonly IAcmeContextProvider _acmeContextProvider;
        private readonly ICertificateGenerator _certificateGenerator;

        public LetsEncryptClient(IAcmeContextProvider acmeContextProvider, ICertificateGenerator certificateGenerator)
        {
            _acmeContextProvider = acmeContextProvider ?? throw new ArgumentNullException(nameof(acmeContextProvider));
            _certificateGenerator = certificateGenerator ?? throw new ArgumentNullException(nameof(certificateGenerator));
        }

        public async Task<X509Certificate2> GetCertificateAsync(string domain)
        {
            // Ensure the ACME context is initialized and the account exists or is created
            await _acmeContextProvider.GetAcmeContextAsync();

            // Generate and return the certificate
            return await _certificateGenerator.GenerateCertificateAsync(domain);
        }
    }
}
