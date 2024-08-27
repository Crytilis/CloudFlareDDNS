using Newtonsoft.Json;

namespace ZoneUpdater.Models
{
    public class DnsRecordRequest
    {
        [JsonProperty("content")]
        public string? Content { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("proxied")]
        public bool Proxied { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("comment")]
        public string? Comment { get; set; }

        [JsonProperty("tags")]
        public List<string?>? Tags { get; set; }

        [JsonProperty("ttl")]
        public int Ttl { get; set; }

    }
}
