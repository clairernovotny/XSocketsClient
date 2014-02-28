using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSocketsClient.Common.Interfaces;

namespace XSocketsClient
{
    class SocketWrapper2 : ISocketWrapper
    {


        public string RemoteIpAddress { get; private set; }
        public Stream ReadStream { get; private set; }
        public Stream WriteStream { get; private set; }
        public Task SendAsync(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public Task<int> ReceiveAsync(byte[] buffer, int offset = 0)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync(Uri host, Guid storageGuid = default(Guid))
        {
            throw new NotImplementedException();
        }


    }
}
