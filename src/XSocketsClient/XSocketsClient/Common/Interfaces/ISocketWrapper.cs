using System;
using System.Threading.Tasks;
using System.IO;

namespace XSocketsClient.Common.Interfaces
{
    public interface ISocketWrapper
    {
        bool Connected { get; set; }
        string RemoteIpAddress { get; }
        Stream Stream { get; }

        
        Task SendAsync(byte[] buffer);
        Task<int> ReceiveAsync(byte[] buffer, int offset = 0);
        
        void Dispose();
        void Close();

 
        Task<Stream> ConnectAsync(string host, int port);
    }
}