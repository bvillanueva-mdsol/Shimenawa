using Newtonsoft.Json;

namespace Medidata.Shimenawa.Models.Sumo
{
    public class SumoLogMapMessage
    {
        [JsonProperty("_raw")]
        public string RawMessage { get; set; }
    }
}