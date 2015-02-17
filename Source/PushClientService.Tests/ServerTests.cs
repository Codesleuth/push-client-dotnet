using System;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PushClientService.models;
using PushClientService.services;
using PushClientService.wrappers;

namespace PushClientService.Tests
{
    [TestFixture]
    public class ServerTestsWhenStarting
    {
        private Mock<ISocketWrapper> _socket;

        [SetUp]
        public void GivenAServerWhenStarting()
        {
            _socket = new Mock<ISocketWrapper>();

            var server = new Server(() => _socket.Object, null, null);
            server.Start();
        }

        [Test]
        public void ThenASocketConnectedEventHandlerIsAttached()
        {
            _socket.Verify(socket => socket.On("connect", It.IsAny<Action>()));
        }

        [Test]
        public void ThenAPushEventHandlerIsAttached()
        {
            _socket.Verify(socket => socket.On("PushEvent", It.IsAny<Action<object>>()));
        }
    }

    [TestFixture]
    public class ServerTestsWhenStopping
    {
        private Mock<ISocketWrapper> _socket;
        private Mock<IPushService> _pushService;

        [SetUp]
        public void GivenAServerWhenStopping()
        {
            _socket = new Mock<ISocketWrapper>();
            _pushService = new Mock<IPushService>();

            var server = new Server(() => _socket.Object, _pushService.Object, null);
            server.Start();
            server.Stop();
        }

        [Test]
        public void ThenThePushServiceIsCancelled()
        {
            _pushService.Verify(service => service.Cancel());
        }

        [Test]
        public void ThenTheSocketIsClosed()
        {
            _socket.Verify(socket => socket.Close());
        }

        [Test]
        public void ThenTheSocketIsDisconnected()
        {
            _socket.Verify(socket => socket.Disconnect());
        }
    }

    [TestFixture]
    public class ServerTestsOnConnected
    {
        private Mock<ISocketWrapper> _socket;
        private string _secret;

        [SetUp]
        public void GivenAServerWhenStarting()
        {
            _secret = "somesecret";

            _socket = new Mock<ISocketWrapper>();
            _socket
                .Setup(socket => socket.On("connect", It.IsAny<Action>()))
                .Callback<string, Action>((eventString, fn) => fn());

            var server = new Server(() => _socket.Object, null, _secret);
            server.Start();
        }

        [Test]
        public void ThenTheSecretIsSentToTheServer()
        {
            _socket.Verify(socket => socket.Emit("secret", It.IsAny<Action<object>>(), _secret));
        }
    }

    [TestFixture]
    public class ServerTestsOnPushEvent
    {
        private Mock<ISocketWrapper> _socket;
        private string _secret;
        private Mock<IPushService> _pushService;
        private JObject _bodyObject;
        private string _deliveryHeader;
        private string _signatureHeader;
        private string _userAgentHeader;

        [SetUp]
        public void GivenAServerWhenStartingThenReceivingPushEvent()
        {
            _secret = "somesecret";
            _deliveryHeader = "1234";
            _signatureHeader = "badly written";
            _userAgentHeader = "batman";

            var headersObject = new JObject
            {
                {"X-Github-Delivery", _deliveryHeader},
                {"X-Hub-Signature", _signatureHeader},
                {"User-Agent", _userAgentHeader}
            };
            _bodyObject = new JObject();

            var data = new JObject
            {
                {"headers", headersObject},
                {"body", _bodyObject}
            };

            _socket = new Mock<ISocketWrapper>();
            _socket
                .Setup(socket => socket.On("connect", It.IsAny<Action>()))
                .Callback<string, Action>((eventString, fn) => fn());
            _socket
                .Setup(socket => socket.On("PushEvent", It.IsAny<Action<object>>()))
                .Callback<string, Action<object>>((eventString, fn) => fn(data));

            _pushService = new Mock<IPushService>();

            var server = new Server(() => _socket.Object, _pushService.Object, _secret);
            server.Start();
        }

        [Test]
        public void ThenThePushEventIsPushed()
        {
            _pushService.Verify(service => service.Push(It.Is<PushHeaders>(headers => headers.Delivery == _deliveryHeader
                                                                                      && headers.Signature == _signatureHeader
                                                                                      && headers.UserAgent == _userAgentHeader), _bodyObject));
        }
    }
}
