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

        
        Task Send(byte[] buffer, Action callback, Action<Exception> error);
        Task<int> Receive(byte[] buffer, Action<int> callback, Action<Exception> error, int offset = 0);
        
        void Dispose();
        void Close();

 
        Task<Stream> ConnectAsync(string host, int port);
    }
}