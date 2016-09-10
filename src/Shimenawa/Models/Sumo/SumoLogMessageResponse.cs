using System.Collections.Generic;
using Newtonsoft.Json;

namespace Medidata.Shimenawa.Models.Sumo
{
    public class SumoLogMessageResponse
    {
        [JsonProperty("messages")]
        public List<SumoMessage> Messages { get; set; }
    }
}