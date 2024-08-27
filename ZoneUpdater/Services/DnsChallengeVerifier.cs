using DnsClient;

namespace ZoneUpdater.Services
{
    internal class DnsChallengeVerifier : IDnsChallengeVerifier
    {
        private readonly LookupClient _lookupClient = new();

        public async Task<bool> VerifyDnsTxtRecordAsync(string domain, string expectedTxtValue)
        {
            var queryResult = await _lookupClient.QueryAsync($"_acme-challenge.{domain}", QueryType.TXT);
            var txtRecords = queryResult.Answers.TxtRecords();
            return txtRecords.Any(txtRecord => txtRecord.Text.Any(txtValue => txtValue == expectedTxtValue));
        }
    }
}
