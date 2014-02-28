using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Threading;
//using Org.BouncyCastle.Pkcs;
using XSocketsClient.Common.Interfaces;

namespace XSocketsClient.Wrapper
{
    public class SocketWrapper : ISocketWrapper, IDisposable
    {
        private readonly CancellationTokenSource _tokenSource;
        private readonly TaskFactory _taskFactory;
   //     private readonly X509Certificate2 _certificate2;

        public string RemoteIpAddress
        {
            get
            {
                var endpoint = Socket.Client.RemoteEndPoint as IPEndPoint;
                return endpoint != null ? endpoint.Address.ToString() : null;
            }
        }

        private NetworkStream ns;

        public SocketWrapper()
        {
            _tokenSource = new CancellationTokenSource();
            _taskFactory = new TaskFactory(_tokenSource.Token);
        }

        public async Task ConnectAsync(Uri host, string origin, string protocol)
        {
            Socket = new TcpClient();

            var secure = "wss".Equals(host.Scheme, StringComparison.OrdinalIgnoreCase);
            
            
            await Socket.ConnectAsync(host.Host, host.Port).ConfigureAwait(false);

            Socket.NoDelay = true;
            if (Socket.Connected)
            {

                var stream = Socket.GetStream();
                ns = stream;

                if (secure)
                {
                    var ssl = new SslStream(stream, false, (sender, x509Certificate, chain, errors) =>
                    {
                        
                        //if (errors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
                        //{
                        //    return true;
                        //}

                        //if (errors == SslPolicyErrors.None)
                        //return true;
                        //    return true;
                        return errors == SslPolicyErrors.None;
                    }, null);
                    await ssl.AuthenticateAsClientAsync(host.Host).ConfigureAwait(false);

                    var tempStream = new SslStreamWrapper(ssl);
                    ReadStream = tempStream;
                    WriteStream = tempStream;

                }
                else
                {
                    ReadStream = stream;
                    WriteStream = stream;
                }
                
            }

            //if(_certificate2 != null)
            //    await AuthenticateAsClient(_certificate2);
        }

        //public SocketWrapper(byte[] certificate)
        //    : this()
        //{
        //    _certificate2 = new X509Certificate2(certificate);
        //}

        //private async Task AuthenticateAsClient(X509Certificate2 certificate)
        //{
        //    var ssl = new SslStream(ReadStream, false, (sender, x509Certificate, chain, errors) =>
        //    {
        //        if (errors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
        //        {
        //            return true;
        //        }

        //        // if (errors == SslPolicyErrors.None)
        //        //return true;
        //        return true;
        //    }, null);

        //    var tempStream = new SslStreamWrapper(ssl);
        //    ReadStream = tempStream;
        //    WriteStream = tempStream;

        //    await ssl.AuthenticateAsClientAsync(RemoteIpAddress, new X509Certificate2Collection(certificate), SslProtocols.Tls, false).ConfigureAwait(false);
        //}

        private TcpClient Socket { get; set; }

        protected virtual bool Connected
        {
            get { return Socket.Connected; }
        }

        public Stream ReadStream { get; private set; }
        public Stream WriteStream { get; private set; }

        public virtual async Task<int> ReceiveAsync(byte[] buffer, int offset = 0)
        {
            if (_tokenSource.IsCancellationRequested || !this.Connected)
                return -1;

            var bytesRead = await ReadStream.ReadAsync(buffer, offset, buffer.Length - offset).ConfigureAwait(false);
            //var bytesRead = ReadStream.Read(buffer, offset, buffer.Length);

            return bytesRead;
        }


        public virtual void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            if (ReadStream != null) ReadStream.Dispose();
            if (WriteStream != null) WriteStream.Dispose();
            if (Socket != null) Socket.Close();
        }

        public virtual void Close()
        {
            if (ReadStream != null)
            {
                ReadStream.Flush();
                ReadStream.Close();
            }
            if (WriteStream != null)
            {
                WriteStream.Flush();
                WriteStream.Close();
            }

            if (Socket != null) 
                Socket.Close();
        }

        public virtual async Task SendAsync(byte[] buffer)
        {
            if (_tokenSource.IsCancellationRequested || !this.Connected)
                return;

            try
            {
                await WriteStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                await WriteStream.FlushAsync();
            }
            catch (IOException)
            {
                _tokenSource.Cancel();
            }
        }
    }
}
