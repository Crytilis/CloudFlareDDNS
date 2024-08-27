using System.Security.Cryptography.X509Certificates;

namespace ZoneUpdater.Services;

public interface ICertificateGenerator
{
    Task<X509Certificate2> GenerateCertificateAsync(string domain);
}