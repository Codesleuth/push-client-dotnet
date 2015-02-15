using System;
using Quobject.EngineIoClientDotNet.ComponentEmitter;

namespace PushClientService.wrappers
{
    public interface ISocketWrapper
    {
        Emitter On(string eventString, Action fn);
        Emitter On(string eventString, Action<object> fn);
        Emitter Emit(string eventString, Action<object> ack, params object[] args);
        void Close();
        void Disconnect();
    }
}