using Newtonsoft.Json;

namespace ZoneUpdater.Models
{
    public class DnsRecordResult
    {
        [JsonProperty("content")]
        public string? Content { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("proxied")]
        public bool Proxied { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("ttl")]
        public int Ttl { get; set; }
    }
}
