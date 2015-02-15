namespace PushClientService.services
{
    public interface IPayloadPusher
    {
        void Push(object data);
    }
}