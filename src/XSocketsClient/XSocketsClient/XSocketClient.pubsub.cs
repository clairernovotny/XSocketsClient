using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XSocketsClient.Common.Event.Interface;
using XSocketsClient.Common.Interfaces;
using XSocketsClient.Globals;
using XSocketsClient.Model;

namespace XSocketsClient
{
    /// <summary>
    /// A client for communicating with XSockets over pub/sub
    /// </summary>
    public partial class XSocketClient
    {
        private IList<ISubscription> Bindings { get; set; }

        private void AddDefaultSubscriptions()
        {
            var onError = new Subscription(Constants.Events.OnError, Error) { IsBound = true };
            this.AddBinding(onError);
            var onOpen = new Subscription(Constants.Events.Connections.Opened, Opened) { IsBound = true };
            this.AddBinding(onOpen);
        }

        public IList<ISubscription> GetBindings()
        {
            lock (this.Bindings)
            {            
                return this.Bindings.ToList();
            }
        }
        private void AddBinding(ISubscription subscription)
        {
            lock (this.Bindings)
            {
                this.Bindings.Add(subscription);
            }
        }

        private void RemoveBinding(ISubscription subscription)
        {
            lock (this.Bindings)
            {
                this.Bindings.Remove(subscription);
            }
        }

        private Task BindUnboundBindings()
        {
            var unboundBindings = this.GetBindings().Where(p => p.IsBound == false).ToList();

            if (!unboundBindings.Any()) 
                return Task.FromResult(true);

            var tasks = new List<Task>();
            foreach (var unboundBinding in unboundBindings)
            {
                var binding = unboundBinding;

                var task = Task.Run(async () =>
                {
                    await Send(this.AsTextArgs(new XSubscriptions {Event = binding.Event.ToLower(), Confirm = binding.Confirm}
                                               , Constants.Events.PubSub.Subscribe)).ConfigureAwait(false);

                    var b = this.GetBindings().Single(p => p.Event == binding.Event);
                    b.IsBound = true;
                });
                tasks.Add(task);
            }

            return Task.WhenAll(tasks);
        }

        public Task Bind(string name)
        {
            return this.Subscribe(name, SubscriptionType.All);          
        }

        public Task Bind(string name, Action<ITextArgs> callback)
        {
            return this.Subscribe(name, callback, SubscriptionType.All);         
        }

        public Task Bind(string name, Action<ITextArgs> callback, Action<ITextArgs> confirmCallback)
        {
            return this.Subscribe(name, callback, confirmCallback, SubscriptionType.All);
        }

        public Task One(string name, Action<ITextArgs> callback)
        {
            return this.Subscribe(name, callback, SubscriptionType.One);    
        }

        public Task One(string name, Action<ITextArgs> callback, Action<ITextArgs> confirmCallback)
        {
            return this.Subscribe(name, callback, confirmCallback, SubscriptionType.One);
        }

        public Task Many(string name, uint limit, Action<ITextArgs> callback)
        {
            return this.Subscribe(name, callback, SubscriptionType.Many, limit);
        }

        public Task Many(string name, uint limit, Action<ITextArgs> callback, Action<ITextArgs> confirmCallback)
        {
            return this.Subscribe(name, callback, confirmCallback, SubscriptionType.Many, limit);
        }

        private void AddConfirmCallback(Action<ITextArgs> confirmCallback, string @event)
        {
            var e = string.Format("__{0}", @event);
            if (this.GetBindings().Any(p => p.Event == e)) return;

            var confirm = new Subscription(e, confirmCallback);
            this.AddBinding(confirm);
            confirm.IsBound = this.IsConnected;
        }

        /// <summary>
        /// Remove the subscription from the list
        /// </summary>
        /// <param name="name"></param>
        public async Task UnBind(string name)
        {
            ISubscription subscription = this.GetBindings().FirstOrDefault(b => b.Event.Equals(name.ToLower()));
            if (subscription == null) return;

            if (this.IsConnected)
            {
                //Unbind on server
                await Send(this.AsTextArgs(new XSubscriptions {Event = name.ToLower()}, Constants.Events.PubSub.Unsubscribe)).ConfigureAwait(false);
            }

            this.RemoveBinding(subscription);
        }

        private async Task Subscribe(string name, SubscriptionType subscriptionType, uint limit = 0)
        {
            ThrowIfPrimitive();
            var subscription = new Subscription(name, subscriptionType, limit);
            this.AddBinding(subscription);

            if (this.IsConnected)
            {
                await Send(this.AsTextArgs(new XSubscriptions
                {
                    Event = name.ToLower(),
                    Confirm = true
                }, Constants.Events.PubSub.Subscribe)).ConfigureAwait(false);

                subscription.IsBound = true;
            }
        }
        private async Task Subscribe(string name, Action<ITextArgs> callback, SubscriptionType subscriptionType, uint limit = 0)
        {
            ThrowIfPrimitive();
            var subscription = new Subscription(name, callback, subscriptionType, limit);
            this.AddBinding(subscription);

            if (this.IsConnected)
            {                
                await Send(this.AsTextArgs(new XSubscriptions
                {
                    Event = name.ToLower(),
                    Confirm = true
                }, Constants.Events.PubSub.Subscribe)).ConfigureAwait(false);
                subscription.IsBound = true;
            }
        }
        private async Task Subscribe(string name, Action<ITextArgs> callback, Action<ITextArgs> confirmCallback, SubscriptionType subscriptionType, uint limit = 0)
        {
            ThrowIfPrimitive();
            var subscription = new Subscription(name.ToLower(), callback, subscriptionType, limit, true);
            this.AddBinding(subscription);
            AddConfirmCallback(confirmCallback, subscription.Event);
            if (this.IsConnected)
            {                
                await Send(this.AsTextArgs(new XSubscriptions { Event = name.ToLower() }, Constants.Events.PubSub.Subscribe)).ConfigureAwait(false);
                subscription.IsBound = true;
            }
        }        
        //Sending methods
        

        /// <summary>
        ///     Send a binary message)
        /// </summary>
        /// <param name="payload">IBinaryArgs</param>
        public async Task Send(IBinaryArgs payload)
        {
            if (!this.IsConnected)
                throw new Exception("You cant send messages when not conencted to the server");
            
            var frame = GetDataFrame(payload);
            try
            {
                await Socket.SendAsync(frame.ToBytes()).ConfigureAwait(false);
            }
            catch
            {
                FireOnClose();
            }
        }

        public async Task Send(string payload)
        {
            if (!this.IsConnected)
                throw new Exception("You cant send messages when not conencted to the server");

            var frame = GetDataFrame(payload);
            try
            {
                await Socket.SendAsync(frame.ToBytes()).ConfigureAwait(false);
            }
            catch
            {
                FireOnClose();
            }
        }
     

        public async Task Send(ITextArgs payload)
        {
            if (!this.IsConnected)
                throw new Exception("You cant send messages when not conencted to the server");

            var frame = GetDataFrame(payload);
            try
            {

                await Socket.SendAsync(frame.ToBytes()).ConfigureAwait(false);
            }
            catch (Exception)
            {
                FireOnClose();
            }
        }

        public Task Send(object obj, string @event)
        {
            return this.Send(this.AsTextArgs(obj, @event));
        }

        public Task Trigger(ITextArgs payload)
        {
            return this.Send(payload);
        }

        public Task Trigger(object obj, string @event)
        {
            return this.Send(this.AsTextArgs(obj,@event));
        }

        public Task Trigger(IBinaryArgs payload)
        {
            return this.Send(payload);
        }
    }
}