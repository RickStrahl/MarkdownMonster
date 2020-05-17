using System.Threading;
using MarkdownMonster.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster._Classes.WebSockets
{
    [TestClass]
    public class WebServerTests
    {

        [TestMethod]
        public void WebServerTest()
        {

            WebServer server = null;
            try
            {
                server = new WebServer();
                server.StartServer();

                Thread.Sleep(1000000);
            }
            finally
            {
                server?.StopServer();
            }
        }


    }
}
