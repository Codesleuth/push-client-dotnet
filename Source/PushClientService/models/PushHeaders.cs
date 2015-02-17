using Newtonsoft.Json;

namespace PushClientService.models
{
    public class PushHeaders
    {
        [JsonProperty("X-Github-Signature")]
        public string Signature { get; set; }

        [JsonProperty("X-Github-Delivery")]
        public string Delivery { get; set; }

        [JsonProperty("User-Agent")]
        public string UserAgent { get; set; }
    }
}