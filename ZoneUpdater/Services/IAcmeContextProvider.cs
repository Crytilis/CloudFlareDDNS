using Certes;

namespace ZoneUpdater.Services
{
    public interface IAcmeContextProvider
    {
        Task<IAcmeContext> GetAcmeContextAsync();
    }
}
