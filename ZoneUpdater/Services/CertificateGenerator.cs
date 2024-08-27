using Certes.Acme.Resource;
using Certes;
using System.Security.Cryptography.X509Certificates;

namespace ZoneUpdater.Services
{
    internal class CertificateGenerator : ICertificateGenerator
    {
        private readonly IAcmeContextProvider _acmeContextProvider;
        private readonly IDnsChallengeVerifier _dnsChallengeVerifier;

        public CertificateGenerator(IAcmeContextProvider acmeContextProvider, IDnsChallengeVerifier dnsChallengeVerifier)
        {
            _acmeContextProvider = acmeContextProvider;
            _dnsChallengeVerifier = dnsChallengeVerifier;
        }

        public async Task<X509Certificate2> GenerateCertificateAsync(string domain)
        {
            var acmeContext = await _acmeContextProvider.GetAcmeContextAsync();
            var order = await acmeContext.NewOrder(new[] { domain });

            var authz = await order.Authorizations();
            var firstAuthz = authz.First();
            var dnsChallenge = await firstAuthz.Dns();

            // Assume UpdateDnsRecord is a method that updates your DNS TXT record
            // UpdateDnsRecord(domain, dnsChallenge.Token, dnsChallenge.KeyAuthz);

            // Verify DNS TXT record
            var isVerified = await _dnsChallengeVerifier.VerifyDnsTxtRecordAsync(domain, dnsChallenge.KeyAuthz);
            if (!isVerified)
            {
                throw new Exception("DNS TXT record verification failed");
            }

            var challenge = await dnsChallenge.Validate();
            if (challenge.Status != ChallengeStatus.Valid)
            {
                throw new Exception("Challenge validation failed");
            }

            // Generate CSR
            var csrInfo = new CsrInfo
            {
                CommonName = domain,
                CountryName = "US",
                Locality = "City",
                Organization = "Org",
                OrganizationUnit = "OU",
                State = "State"
            };

            var keyPair = KeyFactory.NewKey(KeyAlgorithm.ES256);

            // Finalize the order with the generated CSR.
            _ = await order.Generate(csrInfo, keyPair);

            // Get the location of the order
            var orderLocation = order.Location;

            // Refresh the order to check its status
            var refreshedOrder = acmeContext.Order(orderLocation);
            var orderResource = await refreshedOrder.Resource();

            while ((await refreshedOrder.Resource()).Status == OrderStatus.Pending)
            {
                await Task.Delay(2000);
                orderResource = await refreshedOrder.Resource();
            }

            if (orderResource.Status != OrderStatus.Valid)
            {
                throw new Exception($"Order is not valid. Status: {orderResource.Status}");
            }

            // Fetch certificate using refreshedOrder
            var certChain = await refreshedOrder.Download();
            var pfxBuilder = certChain.ToPfx(keyPair);
            var pfx = pfxBuilder.Build(domain, "");

            return new X509Certificate2(pfx, "", X509KeyStorageFlags.Exportable);
        }
    }
}
