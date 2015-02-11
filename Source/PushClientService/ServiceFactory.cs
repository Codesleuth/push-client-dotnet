namespace PushClientService
{
    internal static class ServiceFactory
    {
        public static Server CreateServer()
        {
            return new Server();
        }
    }
}