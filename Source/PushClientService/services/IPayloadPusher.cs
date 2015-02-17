using PushClientService.models;

namespace PushClientService.services
{
    public interface IPayloadPusher
    {
        void Push(PushPayload payload);
    }
}