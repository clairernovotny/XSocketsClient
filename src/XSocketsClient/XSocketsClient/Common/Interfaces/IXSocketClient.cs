using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XSocketsClient.Common.Event.Arguments;
using XSocketsClient.Common.Event.Interface;

namespace XSocketsClient.Common.Interfaces
{
    public interface IXSocketClient
    {
        event EventHandler<TextArgs> OnMessage;
        event EventHandler<BinaryArgs> OnBlob;
        event EventHandler OnOpen;
        event EventHandler OnClose;
        event EventHandler<TextArgs> OnError;
        event EventHandler<BinaryArgs> OnPong;
        event EventHandler<BinaryArgs> OnPing;

        IXSocketJsonSerializer Serializer { get; set; }
        IClientInfo ClientInfo { get; set; }
        
        bool IsConnected { get; }
        bool IsHandshakeDone { get; }
        bool FireOnMessageForUnboundEvents { get; set; }
        ISocketWrapper Socket { get; }
        IList<ISubscription> GetBindings();
        
        string Url { get; }

        Task Open();
        void Close();
        
        void Bind(string name);              
        void Bind(string name, Action<ITextArgs> callback);
        void Bind(string name, Action<ITextArgs> callback, Action<ITextArgs> confirmCallback);
        void One(string name, Action<ITextArgs> callback);
        void One(string name, Action<ITextArgs> callback, Action<ITextArgs> confirmCallback);
        void Many(string name, uint limit, Action<ITextArgs> callback);
        void Many(string name, uint limit, Action<ITextArgs> callback, Action<ITextArgs> confirmCallback);

        void UnBind(string name);
      
        void Send(string payload);
        void Send(string payload, Action callback);
        void Send(ITextArgs payload);
        void Send(ITextArgs payload, Action callback);
        void Send(IBinaryArgs payload);
        void Trigger(ITextArgs payload);
        void Trigger(ITextArgs payload, Action callback);
        void Send(object obj, string @event);
        void Send(object obj, string @event, Action callback);
        void Trigger(object obj, string @event);
        void Trigger(object obj, string @event, Action callback);
        void Trigger(IBinaryArgs payload);
    }
}
