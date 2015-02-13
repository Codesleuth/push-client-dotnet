using log4net;
using Quobject.SocketIoClientDotNet.Client;

namespace PushClientService
{
    public class Server
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Server));
        private Socket _socket;

        public void Start()
        {
            _log.Info("Service starting...");
            _socket = IO.Socket(Configuration.PushProxyUrl);
            _socket.On(Socket.EVENT_CONNECT, () =>
            {
                _log.InfoFormat("Connected to {0}.", Configuration.PushProxyUrl);

                _socket.Emit("secret",
                    data => _log.InfoFormat("Subscribed to secret: {0}", Configuration.Secret),
                    Configuration.Secret);

                _socket.On("PushEvent", data =>
                {
                    _log.Info("PushEvent received");
                    _log.Debug("Payload: " + data.ToString());
                    PushService.Push(data);
                });
            });
            _socket.On(Socket.EVENT_DISCONNECT, () => _log.InfoFormat("Disconnected from {0}.", Configuration.PushProxyUrl));
            _log.Info("Service started.");
        }

        public void Stop()
        {
            _log.Info("Service stopping...");
            _socket.Close();
            _socket.Disconnect();
            _log.Info("Service stopped.");
        }
    }
}