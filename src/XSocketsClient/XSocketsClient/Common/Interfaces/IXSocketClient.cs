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
        Task Close();
        
        Task Bind(string name);
        Task Bind(string name, Action<ITextArgs> callback);
        Task Bind(string name, Action<ITextArgs> callback, Action<ITextArgs> confirmCallback);
        Task One(string name, Action<ITextArgs> callback);
        Task One(string name, Action<ITextArgs> callback, Action<ITextArgs> confirmCallback);
        Task Many(string name, uint limit, Action<ITextArgs> callback);
        Task Many(string name, uint limit, Action<ITextArgs> callback, Action<ITextArgs> confirmCallback);

        Task UnBind(string name);
      
        Task Send(string payload);
        Task Send(ITextArgs payload);
        Task Send(IBinaryArgs payload);
        Task Trigger(ITextArgs payload);
        Task Send(object obj, string @event);
        Task Trigger(object obj, string @event);
        Task Trigger(IBinaryArgs payload);
        Task Ping(byte[] data);
        Task Pong(byte[] data);
    }
}
