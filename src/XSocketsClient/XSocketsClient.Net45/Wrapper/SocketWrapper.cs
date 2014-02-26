using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Threading;
using XSocketsClient.Common.Interfaces;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace XSocketsClient.Wrapper
{
    public class SocketWrapper : ISocketWrapper, IDisposable
    {
        private readonly CancellationTokenSource _tokenSource;
        private readonly TaskFactory _taskFactory;
        private readonly X509Certificate2 _certificate2;

        public string RemoteIpAddress
        {
            get
            {
                var endpoint = Socket.RemoteEndPoint as IPEndPoint;
                return endpoint != null ? endpoint.Address.ToString() : null;
            }
        }

        public SocketWrapper()
        {
            _tokenSource = new CancellationTokenSource();
            _taskFactory = new TaskFactory(_tokenSource.Token);
        }

        public async Task<Stream> ConnectAsync(string host, int port)
        {
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            
            var task = _taskFactory.FromAsync((cb, s) => Socket.BeginConnect(host, port, cb, s), Socket.EndConnect, null).ConfigureAwait(false);
            
            await task;

            Socket.NoDelay = false;
            if (Socket.Connected)
                Stream = new NetworkStream(Socket);

            

            if(_certificate2 != null)
                await AuthenticateAsClient(_certificate2);

            return Stream;
        }

        public SocketWrapper(X509Certificate certificate) : this()
        {
            _certificate2 = new X509Certificate2(certificate.GetEncoded());
        }

        public async Task AuthenticateAsClient(X509Certificate2 certificate)
        {
            var ssl = new SslStream(Stream, false, (sender, x509Certificate, chain, errors) =>
            {
                if (errors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
                {
                    return true;
                }

                // if (errors == SslPolicyErrors.None)
                //return true;
                return true;
            }, null);

            var tempStream = new SslStreamWrapper(ssl);
            Stream = tempStream;

            await ssl.AuthenticateAsClientAsync(RemoteIpAddress, new X509Certificate2Collection(certificate), SslProtocols.Tls, false).ConfigureAwait(false);
        }

        public Socket Socket { get; set; }

        public virtual void Bind(EndPoint endPoint)
        {
            Socket.Bind(endPoint);
        }

        public virtual bool Connected
        {
            get { return Socket.Connected; }
            set { }
        }

        public Stream Stream { get; private set; }

        public virtual async Task<int> ReceiveAsync(byte[] buffer, int offset = 0)
        {
            if (_tokenSource.IsCancellationRequested || !this.Connected)
                return -1;

            
            var read = await Stream.ReadAsync(buffer, offset, buffer.Length - offset).ConfigureAwait(false);

            return read;
        }


        public virtual void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            if (Stream != null) Stream.Dispose();
            if (Socket != null) Socket.Dispose();
        }

        public virtual void Close()
        {
            if (Stream != null)
            {
                Stream.Flush();
                Stream.Close();
            }

            if (Socket != null) Socket.Close();
        }

        public int EndSend(IAsyncResult asyncResult)
        {
            Stream.EndWrite(asyncResult);
            return 0;
        }

        public virtual async Task SendAsync(byte[] buffer)
        {
            if (_tokenSource.IsCancellationRequested || !this.Connected)
                return;

            try
            {
                await Stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            }
            catch (IOException)
            {
                _tokenSource.Cancel();
            }
        }
    }
}
