using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.X509;
using XSocketsClient.Common.Event.Arguments;
using XSocketsClient.Common.Event.Interface;
using XSocketsClient.Common.Interfaces;
using XSocketsClient.Globals;
using XSocketsClient.Helpers;
using XSocketsClient.Model;
using XSocketsClient.Protocol;
using XSocketsClient.Protocol.Handshake.Builder;


namespace XSocketsClient
{
    /// <summary>
    /// A client for communicating with XSockets over pub/sub
    /// </summary>
    public partial class XSocketClient : IXSocketClient
    {
        public IXSocketJsonSerializer Serializer { get; set; }

        public IClientInfo ClientInfo { get; set; }

        /// <summary>        
        /// Will fire in two scenarios
        /// - The binding that fires is primitive
        /// - The IsPrimitive property is true when passed into the constructor
        /// </summary>
        public event EventHandler<TextArgs> OnMessage;

        /// <summary>
        /// Fires when a binary message arrives
        /// </summary>
        public event EventHandler<BinaryArgs> OnBlob;

        /// <summary>
        /// Fires when the handshake is done and everything is setup on the server
        /// </summary>
        public event EventHandler OnOpen;

        /// <summary>
        /// Fires if the connection is closed
        /// </summary>
        public event EventHandler OnClose;

        /// <summary>
        /// Fires when an error message is received
        /// </summary>
        public event EventHandler<TextArgs> OnError;

        public event EventHandler<BinaryArgs> OnPing;
        public event EventHandler<BinaryArgs> OnPong;

        /// <summary>
        /// Fires when the socket is open
        /// </summary>
        internal event EventHandler OnSocketOpen;

        private bool IsPrimitive { get; set; }

        /// <summary>
        /// Return true if the socket is connected and the handshake is done
        /// </summary>
        public bool IsConnected
        {
            get { return this.Socket != null && this.Socket.Connected && this.IsHandshakeDone; }
        }

        /// <summary>
        /// True if the handshake with the server is done
        /// </summary>
        public bool IsHandshakeDone { get; private set; }

        public bool FireOnMessageForUnboundEvents { get; set; }

        public ISocketWrapper Socket { get; private set; }

        public string Url { get; private set; }

        
        private IXFrameHandler _frameHandler;
        
        private readonly string _origin;
        private readonly X509Certificate _certificate;

        public XSocketClient(string url, string origin, bool connect = false, bool isPrimitive = false)
        {
            this.ClientInfo = new ClientInfo();

            this.Serializer = new XSocketJsonSerializer();

            this.Bindings = new List<ISubscription>();

            this.IsPrimitive = isPrimitive;

            this.AddDefaultSubscriptions();

            this.Url = url;
            this._origin = origin;
            
#pragma warning disable 4014
            if (connect)
                Open();
#pragma warning restore 4014
        }

        public XSocketClient(string url, string origin, X509Certificate certificate, bool connect = false, bool isPrimitive = false):this(url,origin,connect,isPrimitive)
        {
            this._certificate = certificate;
        }

        private void Error(ITextArgs textArgs)
        {
            if (this.OnError != null)
                this.OnError.Invoke(this, textArgs as TextArgs);
        }

        public async Task Open()
        {
            if (this.IsConnected) return;

            var connectionstring = GetConnectionstring();

            var handshake = new Rfc6455Handshake(connectionstring, this._origin);
            
            await ConnectSocket().ConfigureAwait(false);

            await DoHandshake(handshake).ConfigureAwait(false);
        }

        private string GetConnectionstring()
        {
            var connectionstring = this.Url;

            if (this.ClientInfo.StorageGuid != Guid.Empty)
                connectionstring += "?" + Constants.Connection.Parameters.XSocketsClientStorageGuid + "=" +
                                    this.ClientInfo.StorageGuid;
            return connectionstring;
        }
        
        private async Task ConnectSocket()
        {
            var url = new Uri(this.Url);
            Socket = await SocketWrapperFactory.ConnectToSocketAsync(url.Host, url.Port, _certificate).ConfigureAwait(false);
        }

        private async Task DoHandshake(Rfc6455Handshake handshake)
        {
            var buffer = new byte[1024];
            try
            {
                await Socket.SendAsync(Encoding.UTF8.GetBytes(handshake.ToString())).ConfigureAwait(false);
                await Socket.ReceiveAsync(buffer).ConfigureAwait(false);
            }
            catch (Exception)
            {
                FireOnClose();
            }

            Receive();

            IsHandshakeDone = true;

            if (!this.IsPrimitive)
            {
                await BindUnboundBindings().ConfigureAwait(false);
            }

            if (this.OnSocketOpen != null)
                this.OnSocketOpen.Invoke(this, null);
        }

        public ITextArgs AsTextArgs(object o, string eventname)
        {
            return new TextArgs { @event = eventname.ToLower(), data = this.Serializer.SerializeToString(o) };
        }

        private void Opened(ITextArgs textArgs)
        {
            this.ClientInfo = this.Serializer.DeserializeFromString<ClientInfo>(textArgs.data);
            FireOnOpen();
        }

        private void ThrowIfPrimitive()
        {
            if (this.IsPrimitive)
            {
                throw new Exception("You can't use binding if having a primitive connection");
            }
        }

        protected virtual void FireOnBlob(IBinaryArgs args)
        {
            if (this.OnBlob != null) this.OnBlob.Invoke(this, args as BinaryArgs);
        }

        protected virtual void FireOnOpen()
        {
            if (this.OnOpen != null) this.OnOpen.Invoke(this, null);
        }


        /// <summary>
        /// Fires the OnClose event
        /// </summary>
        protected virtual void FireOnClose()
        {
#pragma warning disable 4014
            this.Close();
#pragma warning restore 4014
            if (this.OnClose != null) this.OnClose.Invoke(this, null);
        }

        /// <summary>
        /// A message was received
        /// </summary>
        /// <param name="args"></param>
        protected virtual void FireOnMessage(ITextArgs args)
        {
            try
            {
                //Is this a primitive instance (pub/sub not used)
                if (this.IsPrimitive)
                {
                    if (this.OnMessage != null) this.OnMessage.Invoke(this, args as TextArgs);
                    return;
                }

                //Get binding
                var binding = this.GetBindings().FirstOrDefault(b => b.Event.Equals(args.@event) && b.IsBound);
                //If binding is missing and we allow unbound events, fire onmessage
                if (binding == null)
                {
                    if (FireOnMessageForUnboundEvents)
                    {                        
                        if (this.OnMessage != null) this.OnMessage.Invoke(this, args as TextArgs);
                    }
                    return;
                }

                //The binding is primitive = fire onmessage
                if (binding.IsPrimitive)
                {
                    //fire primitive event onmessage
                    if (this.OnMessage != null) this.OnMessage.Invoke(this, args as TextArgs);
                }
                //Pub/Sub is used, fire the callback with the argument
                else
                {                   
                    this.FireBoundMethod(binding,args);
                }
            }
            catch (Exception) // Will dispatch to OnError on exception
            {
                if (this.OnError != null) this.OnError.Invoke(this, args as TextArgs);
            }
        }

        private async void FireBoundMethod(ISubscription binding, ITextArgs args)
        {
            binding.Callback(args);
            binding.Counter++;
            if (binding.SubscriptionType == SubscriptionType.One)
                await this.UnBind(binding.Event);
            else if (binding.SubscriptionType == SubscriptionType.Many && binding.Counter == binding.Limit)
            {
                await this.UnBind(binding.Event);
            }
        }

        /// <summary>
        ///     Close the current connection
        /// </summary>
        public async Task Close()
        {
            var frame = GetDataFrame(FrameType.Close, Encoding.UTF8.GetBytes(""));

            try
            {
                await Socket.SendAsync(frame.ToBytes());
                Socket.Dispose();
            }
            catch (Exception)
            {
            }

            this.IsHandshakeDone = false;
        }

        public Task Ping(byte[] data)
        {
            return this.SendControlFrame(FrameType.Ping, data);
        }
        public Task Pong(byte[] data)
        {
            return this.SendControlFrame(FrameType.Pong, data);
        }
    }
}