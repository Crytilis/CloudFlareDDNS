using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using ZoneUpdater.Authenticators;
using ZoneUpdater.Models;

namespace ZoneUpdater.Clients
{
    public class CloudFlareClient
    {
        private readonly RestClient _client;

        public CloudFlareClient(string? apiToken)
        {
            if (string.IsNullOrEmpty(apiToken))
            {
                throw new ArgumentNullException(nameof(apiToken), "API token cannot be null or empty");
            }
            _client = new RestClient("https://api.cloudflare.com/client/v4", options => options.Authenticator = new CloudFlareAuthenticator(apiToken), configureSerialization:
                config =>
                {
                    config.UseNewtonsoftJson(new JsonSerializerSettings
                    {
                        ObjectCreationHandling = ObjectCreationHandling.Auto,
                        NullValueHandling = NullValueHandling.Ignore,
                    });
                });
        }

        public async Task<string?> GetZoneIdAsync(string zoneName)
        {
            var response = await _client.GetJsonAsync<CloudFlareResponse<List<ZoneResult>>>($"zones?name={zoneName}");
            var zoneResult = response?.Result?.FirstOrDefault();
            return zoneResult?.Id;
        }

        public async Task<Dictionary<string, string>?> GetRecordIdsAsync(string zoneId, List<string> recordNames)
        {
            var response = await _client.GetJsonAsync<CloudFlareResponse<List<DnsRecordResult?>>>($"zones/{zoneId}/dns_records?type=A");
            if (response?.Result == null)
            {
                return null;
            }

            var recordNameToIdMap = new Dictionary<string, string>();
            foreach (var record in response.Result)
            {
                if (record is { Name: not null } && !string.IsNullOrEmpty(record.Id) && recordNames.Contains(record.Name))
                {
                    recordNameToIdMap[record.Name] = record.Id;
                }
            }

            return recordNameToIdMap.Count > 0 ? recordNameToIdMap : null;
        }

        public async Task<CloudFlareResponse<DnsRecordResult>?> CreateOrUpdateRecordAsync(string zoneId, string? recordId, string recordName, string ipAddress)
        {
            var requestData = new DnsRecordRequest
            {
                Content = ipAddress,
                Name = recordName,
                Proxied = false,
                Type = "A",
                Comment = "DDNS Record Update",
                Tags = new List<string?>(),
                Ttl = 3600
            };
            CloudFlareResponse<DnsRecordResult>? response;

            if (string.IsNullOrEmpty(recordId))
            {
                // Create a new record
                response = await _client.PostJsonAsync<DnsRecordRequest, CloudFlareResponse<DnsRecordResult>>($"zones/{zoneId}/dns_records", requestData);
            }
            else
            {
                // Update existing record
                response = await _client.PutJsonAsync<DnsRecordRequest, CloudFlareResponse<DnsRecordResult>>($"zones/{zoneId}/dns_records/{recordId}", requestData);
            }

            return response is { Success: true } ? response : null;
        }

        public async Task<bool> CreateTxtRecordAsync(string zoneId, string? recordId, string recordName, string content)
        {
            var requestData = new DnsRecordRequest
            {
                Content = content,
                Name = recordName,
                Proxied = false,
                Type = "TXT",
                Comment = "Domain Verification",
                Tags = new List<string?>(),
                Ttl = 3600
            };
            CloudFlareResponse<DnsRecordResult>? response;

            if (string.IsNullOrEmpty(recordId))
            {
                // Create a new record
                response = await _client.PostJsonAsync<DnsRecordRequest, CloudFlareResponse<DnsRecordResult>>($"zones/{zoneId}/dns_records", requestData);
            }
            else
            {
                // Update existing record
                response = await _client.PutJsonAsync<DnsRecordRequest, CloudFlareResponse<DnsRecordResult>>($"zones/{zoneId}/dns_records/{recordId}", requestData);
            }

            return response is { Success: true, Result.Id: not null };
        }
    }
}
