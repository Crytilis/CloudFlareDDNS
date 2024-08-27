using Newtonsoft.Json;

namespace ZoneUpdater.Models
{
    public class CloudFlareResponse<T>
    {
        [JsonProperty("errors")]
        public List<string>? Errors { get; set; }

        [JsonProperty("messages")]
        public List<string>? Messages { get; set; }

        [JsonProperty("result")]
        public T? Result { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
