namespace ZoneUpdater.Services;

public interface IDnsChallengeVerifier
{
    Task<bool> VerifyDnsTxtRecordAsync(string domain, string expectedTxtValue);
}