namespace PushClientService.services
{
    public interface IPushService
    {
        bool Push(object data);
        void Cancel();
    }
}