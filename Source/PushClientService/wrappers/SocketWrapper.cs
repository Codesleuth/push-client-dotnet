using System;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Socket = Quobject.SocketIoClientDotNet.Client.Socket;

namespace PushClientService.wrappers
{
    public class SocketWrapperWrapper : ISocketWrapper
    {
        private readonly Socket _socket;

        public SocketWrapperWrapper(Socket socket)
        {
            _socket = socket;
        }

        public Emitter On(string eventString, Action fn)
        {
            return _socket.On(eventString, fn);
        }

        public Emitter On(string eventString, Action<object> fn)
        {
            return _socket.On(eventString, fn);
        }

        public Emitter Emit(string eventString, Action<object> ack, params object[] args)
        {
            return _socket.Emit(eventString, ack, args);
        }

        public void Close()
        {
            _socket.Close();
        }

        public void Disconnect()
        {
            _socket.Disconnect();
        }
    }
}
