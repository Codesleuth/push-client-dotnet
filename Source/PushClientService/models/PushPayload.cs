using Newtonsoft.Json.Linq;

namespace PushClientService.models
{
    public class PushPayload
    {
        public PushHeaders Headers { get; private set; }
        public JObject Body { get; private set; }

        public PushPayload(PushHeaders headers, JObject body)
        {
            Headers = headers;
            Body = body;
        }
    }
}