﻿using System;
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

        public SocketWrapper()
        {
            _tokenSource = new CancellationTokenSource();
            _taskFactory = new TaskFactory(_tokenSource.Token);
        }

        public async Task ConnectAsync(string host, int port)
        {
            Socket = new TcpClient();
            await Socket.ConnectAsync(host, port).ConfigureAwait(false);

            Socket.NoDelay = false;
            if (Socket.Connected)
            {
                ReadStream = Socket.GetStream();
                WriteStream = ReadStream;
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

            
            var read = await ReadStream.ReadAsync(buffer, offset, buffer.Length - offset).ConfigureAwait(false);

            return read;
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
            }
            catch (IOException)
            {
                _tokenSource.Cancel();
            }
        }
    }
}
