using System.Collections.Concurrent;
using System.Threading.Tasks;
using XSocketsClient.Common.Event.Arguments;
using XSocketsClient.Common.Event.Interface;
using XSocketsClient.Common.Interfaces;
using XSocketsClient.Helpers;

namespace XSocketsClient
{
    /// <summary>
    /// Acts as a wrapper and abstraction for XSocketClient.
    /// If you are only publishing this is the class to use
    /// </summary>
    public class ClientPool
    {
        private BlockingCollection<ITextArgs> _textQueue;

        private IXSocketClient _websocket;

        private IXSocketJsonSerializer _jsonSerializer;

        private static readonly ConcurrentDictionary<string, ClientPool> Websockets;

        private string _conn;
        private static readonly object Locker = new object();

        static ClientPool()
        {
            Websockets = new ConcurrentDictionary<string, ClientPool>();
        }

        public static ClientPool GetInstance(string conn, string origin)
        {
            lock (Locker)
            {
                if (!Websockets.ContainsKey(conn))
                {
                    var x = new ClientPool
                    {
                        _conn = conn,
                        _textQueue = new BlockingCollection<ITextArgs>(),
                        _jsonSerializer = new XSocketJsonSerializer()
                    };
                    x._websocket = new XSocketClient(x._conn, origin);
                    ((XSocketClient)x._websocket).OnSocketOpen += (sender, args) => Task.Factory.StartNew(() =>
                    {
                        //Will send messages to the XSockets server as soon as there is messages in the queue.
                        foreach (var v in x._textQueue.GetConsumingEnumerable())
                        {
                            Websockets[x._conn]._websocket.Send(v);
                        }
                    });
                    x._websocket.OnClose += (sender, args) =>
                    {
                        ClientPool v;
                        Websockets.TryRemove(x._conn, out v);
                    };
                    Websockets.TryAdd(conn, x);
                    x._websocket.Open();
                }
            }
            return Websockets[conn];
        }

        /// <summary>
        /// Send prepared ITextArgs
        /// </summary>
        /// <param name="textArgs"></param>
        public void Send(ITextArgs textArgs)
        {
            Websockets[_conn]._textQueue.Add(textArgs);
        }

        /// <summary>
        /// Send any object, with an eventname... will be transformed into a ITextArgs
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="eventname"></param>
        public void Send(object obj, string eventname)
        {
            Send(new TextArgs { @event = eventname.ToLower(), data = _jsonSerializer.SerializeToString(obj) });
        }
    }
}
