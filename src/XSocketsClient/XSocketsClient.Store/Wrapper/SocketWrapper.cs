using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Windows.Networking;
using Windows.Networking.Sockets;
using XSocketsClient.Common.Interfaces;


namespace XSocketsClient.Wrapper
{
    public class SocketWrapper : ISocketWrapper, IDisposable
    {
        private readonly CancellationTokenSource _tokenSource;
        private readonly TaskFactory _taskFactory;
      //  private readonly X509Certificate _certificate2;
      //  private TlsProtocolHandler _tlsProtocolHandler;


        public string RemoteIpAddress
        {
            get
            {
                var endpoint = Socket.Information.RemoteAddress;
                return endpoint != null ? endpoint.CanonicalName : null;
            }
        }

        public SocketWrapper()
        {
            _tokenSource = new CancellationTokenSource();
            _taskFactory = new TaskFactory(_tokenSource.Token);
        }

        public async Task ConnectAsync(Uri host, Guid storageGuid = default(Guid))
        {
            Socket = new StreamSocket();

            await Socket.ConnectAsync(new HostName(host.Host), host.Port.ToString(CultureInfo.InvariantCulture), SocketProtectionLevel.PlainSocket).AsTask().ConfigureAwait(false);

        
            Socket.Control.NoDelay = false;

            ReadStream = Socket.InputStream.AsStreamForRead();
            WriteStream = Socket.OutputStream.AsStreamForWrite();

            //if(_certificate2 != null)
            //    await AuthenticateAsClient(_certificate2);
        }

        //public SocketWrapper(byte[] certificate)
        //    : this()
        //{

        //    var buf = new DataWriter();
        //    buf.WriteBytes(certificate);
            
        //    var cert = new Certificate(buf.DetachBuffer());


        //}

        //private async Task AuthenticateAsClient(X509Certificate certificate)
        //{
        //    _tlsProtocolHandler = new TlsProtocolHandler(ReadStream, WriteStream);
        //    _tlsProtocolHandler.Connect(new PskTlsClient())

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

        //    await ssl.AuthenticateAsClientAsync(RemoteIpAddress, new X509Certificate2Collection(certificate), SslProtocols.Tls, false).ConfigureAwait(false);
        //}

        public StreamSocket Socket { get; set; }

  

        public Stream ReadStream { get; private set; }
        public Stream WriteStream { get; private set; }

        public virtual async Task<int> ReceiveAsync(byte[] buffer, int offset = 0)
        {
            if (_tokenSource.IsCancellationRequested)
                return -1;

            try
            {
                var read = await ReadStream.ReadAsync(buffer, offset, buffer.Length - offset).ConfigureAwait(false);

                return read;
            }
            catch (Exception)
            {
                return -1;
            }
           
        }


        public virtual void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            if (ReadStream != null) ReadStream.Dispose();
            if (WriteStream != null) WriteStream.Dispose();
            if (Socket != null) Socket.Dispose();
        }

        public virtual void Close()
        {
            if (ReadStream != null)
            {
                ReadStream.Flush();
                ReadStream.Dispose();
            }
            if (WriteStream != null)
            {
                WriteStream.Flush();
                WriteStream.Dispose();
            }

            if (Socket != null) Socket.Dispose();
        }


        public virtual async Task SendAsync(byte[] buffer)
        {
            if (_tokenSource.IsCancellationRequested)
                return;

            try
            {
                await ReadStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            }
            catch (IOException)
            {
                _tokenSource.Cancel();
            }
        }
    }
}
