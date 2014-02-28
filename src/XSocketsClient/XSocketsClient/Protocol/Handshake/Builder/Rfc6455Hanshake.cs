using System;

namespace XSocketsClient.Protocol.Handshake.Builder
{
    internal class Rfc6455Handshake
    {
        private const string Handshake =
            "GET {0} HTTP/1.1\r\n" +
            "Host: {2}\r\n" +
            "Origin: {1}\r\n" +
            "Upgrade: websocket\r\n" +
            "Connection: Upgrade\r\n" +
            "Sec-WebSocket-Protocol: XSocketsNET\r\n" +
            "Sec-WebSocket-Key: {3}\r\n" +
            "Sec-WebSocket-Version: 13\r\n\r\n";// +
            //"{4}";

        private readonly string _host = String.Empty;
        private readonly string _origin = String.Empty;
        private readonly string _path = String.Empty;
        private string Key { get; set; }

        public Rfc6455Handshake(string url, string origin)
        {
            var uri = new Uri(url);
            this._origin = origin;
            this._host = string.Format("{0}:{1}", uri.Host, uri.Port);
           
            this._path = uri.PathAndQuery;
              
            this.Key = GenerateKey();
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
       
        public override string ToString()
        {
            return string.Format(Handshake, _path, _origin, _host, Key /*, "\r\n" + @"^n:ds[4U" */);
        }
    }
}