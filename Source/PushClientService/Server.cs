using System;
using log4net;
using PushClientService.services;
using PushClientService.wrappers;
using Quobject.SocketIoClientDotNet.Client;

namespace PushClientService
{
    public class Server
    {
        private static readonly string EVENT_CONNECT = Socket.EVENT_CONNECT;
        private static readonly string EVENT_DISCONNECT = Socket.EVENT_DISCONNECT;
        private const string EVENT_PUSH_EVENT = "PushEvent";

        private readonly string _secret;
        private readonly Func<ISocketWrapper> _socketFactory;
        private readonly IPushService _pushService;
        private static readonly ILog _log = LogManager.GetLogger(typeof(Server));
        private ISocketWrapper _socket;

        public Server(Func<ISocketWrapper> socketFactory, IPushService pushService, string secret)
        {
            _socketFactory = socketFactory;
            _pushService = pushService;
            _secret = secret;
        }

        public void Start()
        {
            _log.Info("Service starting...");
            _socket = _socketFactory();
            _socket.On(EVENT_CONNECT, OnConnected);
            _socket.On(EVENT_DISCONNECT, OnDisconnect);
            _log.Info("Service started.");
        }

        private void OnConnected()
        {
            _log.Info("Socket.IO Connected.");
            _socket.Emit("secret", OnSecretCallback, _secret);
            _socket.On(EVENT_PUSH_EVENT, OnPushEvent);
        }

        private void OnPushEvent(object data)
        {
            _log.Info("Socket.IO PushEvent received");
            _log.Debug("Payload: " + data);
            _pushService.Push(data);
        }

        private void OnSecretCallback(object data)
        {
            _log.InfoFormat("Subscribed to secret: {0}", _secret);
        }

        private static void OnDisconnect()
        {
            _log.Info("Socket.IO Disconnected.");
        }

        public void Stop()
        {
            _log.Info("Service stopping...");
            _pushService.Cancel();
            _socket.Close();
            _socket.Disconnect();
            _log.Info("Service stopped.");
        }
    }
}