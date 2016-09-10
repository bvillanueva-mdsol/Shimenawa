using Newtonsoft.Json;

namespace Medidata.Shimenawa.Models.Sumo
{
    public class SumoSearchJobStatusResponse
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("messageCount")]
        public int MessageCount { get; set; }
    }
}