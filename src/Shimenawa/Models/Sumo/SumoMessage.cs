using Newtonsoft.Json;

namespace Medidata.Shimenawa.Models.Sumo
{
    public class SumoMessage
    {
        [JsonProperty("map")]
        public SumoLogMapMessage Map { get; set; }
    }
}