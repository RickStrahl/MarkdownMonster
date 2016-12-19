using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class BugReportTests  {

        [TestMethod]
        public void BugReportTest()
        {
            try
            {
                throw new ApplicationException("Error generated... Extended Chars ¢ /►₧ƒƒ");
            }
            catch (Exception ex)
            {
                mmApp.Log("BugReport Test Failure",ex);
                //mmApp.SendBugReport(ex);
            }
            

            // wait to allow thread to finish
            Thread.Sleep(2000);
        }
        

    }
}
