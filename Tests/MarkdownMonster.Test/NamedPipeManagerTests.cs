using System;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class NamedPipeManagerTests   
    {
        [TestMethod]
        public void NamedPipeManager()
        {
            string result = string.Empty;

            var manager = new NamedPipeManager("MarkdownMonster_Tests");
            
            manager.ReceiveString += (s) =>
            {
                Console.WriteLine("Received results: " + s);
                result += s + "\r\n";
            };

            manager.StartServer();

            Thread.Sleep(200);  // allow thread to spin up

            var manager2 = new NamedPipeManager("MarkdownMonster_Tests");

            manager2.Write("Hello World");
            manager2.Write("Goodbye World");                        

            manager.StopServer();

            Thread.Sleep(200);

            Assert.IsFalse(string.IsNullOrEmpty(result),"Result shouldn't be null");
        }

        [TestMethod]
        public void OpenFilesInRunningApplication()
        {
            var manager = new NamedPipeManager("MarkdownMonster");


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("c:\\temp\\test.txt");
            sb.AppendLine("c:\\temp\\davidrosenhaft.prg");

            Console.WriteLine(sb);

            Assert.IsTrue(manager.Write(sb.ToString()));
        }
    }
}
