using System;
using System.Threading.Tasks;
using System.IO;

namespace XSocketsClient.Common.Interfaces
{
    public interface ISocketWrapper
    {
        string RemoteIpAddress { get; }
        Stream ReadStream { get; }
        Stream WriteStream { get; }
        
        Task SendAsync(byte[] buffer);
        Task<int> ReceiveAsync(byte[] buffer, int offset = 0);
        
        void Dispose();
        void Close();


        Task ConnectAsync(Uri host, Guid storageGuid = default(Guid));
    }
}