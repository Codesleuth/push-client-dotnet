using Newtonsoft.Json.Linq;
using PushClientService.models;

namespace PushClientService.services
{
    public interface IPushService
    {
        bool Push(PushHeaders headers, JObject body);
        bool Push(JObject data);
        void Cancel();
    }
}