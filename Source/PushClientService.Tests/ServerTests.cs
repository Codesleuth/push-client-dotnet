using System;
using Moq;
using NUnit.Framework;
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
    }

    [TestFixture]
    public class ServerTestsWhenStopping
    {
        private Mock<ISocketWrapper> _socket;

        [SetUp]
        public void GivenAServerWhenStopping()
        {
            _socket = new Mock<ISocketWrapper>();

            var server = new Server(() => _socket.Object, null, null);
            server.Start();
            server.Stop();
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

        [Test]
        public void ThenAPushEventHandlerIsAttached()
        {
            _socket.Verify(socket => socket.On("PushEvent", It.IsAny<Action<object>>()));
        }
    }

    [TestFixture]
    public class ServerTestsOnPushEvent
    {
        private Mock<ISocketWrapper> _socket;
        private string _secret;
        private object _data;
        private Mock<IPushService> _pushService;

        [SetUp]
        public void GivenAServerWhenStartingThenReceivingPushEvent()
        {
            _secret = "somesecret";
            _data = new object();

            _socket = new Mock<ISocketWrapper>();
            _socket
                .Setup(socket => socket.On("connect", It.IsAny<Action>()))
                .Callback<string, Action>((eventString, fn) => fn());
            _socket
                .Setup(socket => socket.On("PushEvent", It.IsAny<Action<object>>()))
                .Callback<string, Action<object>>((eventString, fn) => fn(_data));

            _pushService = new Mock<IPushService>();

            var server = new Server(() => _socket.Object, _pushService.Object, _secret);
            server.Start();
        }

        [Test]
        public void ThenThePushEventIsPushed()
        {
            _pushService.Verify(service => service.Push(_data));
        }
    }
}
