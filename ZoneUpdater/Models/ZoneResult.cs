using Newtonsoft.Json;

namespace ZoneUpdater.Models
{
    public class ZoneResult
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }
}
