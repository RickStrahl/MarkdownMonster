using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnagItAddin;
using Westwind.Utilities;

namespace SnagItAddIn.Test
{
    [TestClass]
    public class SnagItComponentTests
    {
        [TestMethod]
        public void SnagItComponent()
        {
            var snag = new SnagItAutomation();
            snag.ShowPreviewWindow = true;
            //snag.CaptureMode = CaptureModes.FreeHand;
            snag.ColorDepth = 32;
            snag.DelayInSeconds = 4;
            snag.CapturePath = "c:\\temp";

            string filename = snag.CaptureImageToFile();
            Assert.IsNotNull(filename);

            ShellUtils.GoUrl(filename);
        }
    }
}
