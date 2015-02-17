using System;
using log4net;
using Newtonsoft.Json.Linq;
using PushClientService.models;
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
            _socket.On(EVENT_PUSH_EVENT, OnPushEvent);
            _log.Info("Service started.");
        }

        private void OnConnected()
        {
            _log.Info("Socket.IO Connected.");
            _socket.Emit("secret", OnSecretCallback, _secret);
        }

        private void OnPushEvent(object data)
        {
            _log.Info("Socket.IO PushEvent received");
            _log.Debug("Payload: " + data);

            var jObject = (JObject) data;
            var jHeaders = jObject["headers"];
            var jBody = (JObject) jObject["body"];

            var headers = new PushHeaders
            {
                Delivery = (string) jHeaders["X-Github-Delivery"],
                Signature = (string) jHeaders["X-Hub-Signature"],
                UserAgent = (string) jHeaders["User-Agent"],
            };

            _pushService.Push(headers, jBody);
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