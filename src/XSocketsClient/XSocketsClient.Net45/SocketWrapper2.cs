using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XSocketsClient.Common.Interfaces;

namespace XSocketsClient.Wrapper
{
    public class SocketWrapper2 : ISocketWrapper
    {
        private HttpClient _client;

        public string RemoteIpAddress { get; private set; }
        public Stream ReadStream { get; private set; }
        public Stream WriteStream { get; private set; }
        public async Task SendAsync(byte[] buffer)
        {
            try
            {
                await WriteStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            }
            catch (IOException)
            {
               // _tokenSource.Cancel();
            }
        }

        public Task<int> ReceiveAsync(byte[] buffer, int offset = 0)
        {
            return ReadStream.ReadAsync(buffer, offset, buffer.Length - offset);
        }

        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            if (ReadStream != null)
                ReadStream.Dispose();

            _client.CancelPendingRequests();
        }

        public async Task ConnectAsync(Uri host, string origin, string protocol)
        {
            var builder = new UriBuilder(host);
            
            // translate scheme to http(s)
            if ("wss".Equals(host.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                builder.Scheme = "https";
            }
            else
            {
                builder.Scheme = "http";
            }

           

            //_client = new HttpClient();

            //var requestMessage = new HttpRequestMessage(HttpMethod.Get, builder.ToString());


            //////Set Headers for WS
            //requestMessage.Headers.Upgrade.Add(new ProductHeaderValue("websocket"));
            //requestMessage.Headers.Connection.Add("Upgrade");
            //requestMessage.Headers.TryAddWithoutValidation("Origin", origin);
            //requestMessage.Headers.TryAddWithoutValidation("Sec-WebSocket-Protocol", protocol);
            //requestMessage.Headers.TryAddWithoutValidation("Sec-WebSocket-Key", key);
            //requestMessage.Headers.TryAddWithoutValidation("Sec-WebSocket-Version", "13");
            
            //var resp = await _client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            
            // //check the headers to validate response
            //var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);

            ReadStream = stream;
            WriteStream = stream;
        }

        private string GenerateKey()
        {
            var bytes = new byte[16];
            var random = new Random();
            for (var index = 0; index < bytes.Length; index++)
            {
                bytes[index] = (byte)random.Next(0, 255);
            }
            return Convert.ToBase64String(bytes);
        }


    }
}
