using RestSharp;
using RestSharp.Authenticators;

namespace ZoneUpdater.Authenticators
{
    public class CloudFlareAuthenticator : AuthenticatorBase
    {
        public CloudFlareAuthenticator(string? token) : base(token ?? throw new ArgumentNullException(nameof(token)))
        {
        }

        protected override ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            return ValueTask.FromResult<Parameter>(new HeaderParameter(KnownHeaders.Authorization, $"Bearer {accessToken}"));
        }
    }
}
