using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.X509;
using XSocketsClient.Common.Interfaces;

namespace XSocketsClient
{
    internal static class SocketWrapperFactory
    {
        private const string PlatformAssembly = "XSocketsClient.Platform";
        private static readonly Lazy<Type> _socketWrapperType = new Lazy<Type>(() =>
        {
            var an = typeof(SocketWrapperFactory).GetTypeInfo().Assembly.GetName();
            an.Name = PlatformAssembly;

            var assm = Assembly.Load(an);

            var type = assm.GetType("XSocketsClient.Wrapper.SocketWrapper");

            return type;
        });

        public static async Task<ISocketWrapper> ConnectToSocketAsync(string host, int port, X509Certificate certificate = null)
        {

            object[] args = certificate == null ? null : new[] {certificate};

            var obj = Activator.CreateInstance(_socketWrapperType.Value, args) as ISocketWrapper;
            if (obj == null)
                throw new Exception("Platform assembly not found. Ensure that XSocketsClient.Platform is present");

            await obj.ConnectAsync(host, port).ConfigureAwait(false);

            return obj;
        }
    }
}
