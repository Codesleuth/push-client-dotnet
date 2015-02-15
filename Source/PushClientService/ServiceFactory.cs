using PushClientService.services;
using PushClientService.wrappers;
using Quobject.SocketIoClientDotNet.Client;

namespace PushClientService
{
    internal static class ServiceFactory
    {
        public static Server CreateServer()
        {
            return new Server(CreatePushProxySocket, CreatePushService(), Configuration.Secret);
        }

        private static ISocketWrapper CreatePushProxySocket()
        {
            var socket = IO.Socket(Configuration.PushProxyUrl);
            return new SocketWrapperWrapper(socket);
        }

        private static PushService CreatePushService()
        {
            return new PushService(CreatePayloadPusher(), Configuration.ConcurrentPushes, Configuration.MaxPushQueue);
        }

        private static PayloadPusher CreatePayloadPusher()
        {
            return new PayloadPusher(Configuration.CiUrl);
        }
    }
}