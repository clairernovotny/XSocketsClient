using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;

[assembly: PreApplicationStartMethod(typeof(Samples.Web.App_Start.XSocketsBootstrap), "Start")]

namespace Samples.Web.App_Start
{
    //Server instance
    using XSockets.Core.Common.Socket;

    //Create class for the server instance
    public static class XSocketsBootstrap
    {
        private static IXSocketServerContainer container;
        public static void Start()
        {
            container = XSockets.Plugin.Framework.Composable.GetExport<IXSocketServerContainer>();
            container.StartServers();
            foreach (var server in container.Servers)
            {
                Debug.WriteLine("Started Server: {0}:{1}", server.ConfigurationSetting.Uri.Host, server.ConfigurationSetting.Port);
                Debug.WriteLine(string.Format("Scheme: {0}", server.ConfigurationSetting.Uri.Scheme));
                Debug.WriteLine("SSL/TLS: {0}", server.ConfigurationSetting.IsSecure);
                Debug.WriteLine("Allowed Connections (0 = infinite): {0}", server.ConfigurationSetting.NumberOfAllowedConections);
                Debug.WriteLine("------------------------------------------------------");
            }
        }
    }
}
