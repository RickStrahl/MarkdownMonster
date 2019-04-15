using System;
using System.Collections.Generic;
using MarkdownMonster.Windows;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class mmAppTests
    {
        [TestMethod]
        public void EncryptDecryptStringWithMachineKey()
        {
            string password = "super@Seekrit";

            var encrypted = mmApp.EncryptString(password);
            Assert.IsNotNull(encrypted);
            Console.WriteLine(encrypted);

            var decrypted = mmApp.DecryptString(encrypted);
            Console.WriteLine(decrypted);

            Assert.IsTrue(password == decrypted, "initial and two-way values don't match");
        }

        [TestMethod]
        public void VersionForDisplayTest()
        {
            Assert.AreEqual(mmApp.GetVersionForDisplay("1.70.0.0"), "1.70");
            Assert.AreEqual(mmApp.GetVersionForDisplay("1.70.10.0"), "1.70.10");
            Assert.AreEqual(mmApp.GetVersionForDisplay("1.70.1.0"), "1.70.1");
            Assert.AreEqual(mmApp.GetVersionForDisplay("1.70.1.12"), "1.70.1.12");
        }

        [TestMethod]
        public void FindImageEditorTest()
        {
            var editor = mmFileUtils.FindImageEditor();
            Console.WriteLine(editor);
            Assert.IsNotNull(editor);
        }

        [TestMethod]
        public void ApplicationInsightsOutputBug()
        {

            var appInsights = new TelemetryClient
            {
                InstrumentationKey = Telemetry.Key
            };
            appInsights.Context.Session.Id = Guid.NewGuid().ToString();
            var unique = Guid.NewGuid().ToString();
            
            var appRunTelemetry =
                appInsights.StartOperation<RequestTelemetry>(
                    $"{unique}");
            appRunTelemetry.Telemetry.Start();


            var ex = new ApplicationException("Oops - did it again.");
            var version = mmApp.GetVersion();
            var dotnetVersion = WindowsUtils.GetDotnetVersion();

            appRunTelemetry.Telemetry.Success = false;
            appInsights.TrackException(ex,
                new Dictionary<string, string>
                {
                    {"msg", "Test"},
                    {"version", version },
                    {"dotnetVersion", dotnetVersion }
                });

            try
            {
                appInsights.StopOperation(appRunTelemetry);
            }
            catch
            {
                // LogLocal("Failed to Stop Telemetry Client: " + ex.GetBaseException().Message);
            }

            appInsights.Flush();
            appInsights = null;
            appRunTelemetry.Dispose();
            
        }


        [TestMethod]
        public void ApplicationInsightLogTest()
        {
            mmApp.InitializeLogging();
            mmApp.Log($"Test Message {DateTime.Now}" , new ApplicationException("Exception: Nothing to do nowhere to hide"),logLevel: LogLevels.Critical);
            mmApp.ShutdownLogging();
        }

        [TestMethod]
        public void ApplicationInsightExceptionTest()
        {
            mmApp.InitializeLogging();
            

            var ex = new ApplicationException($"Test Exception thrown as Error");
            mmApp.Log(ex);

            ex = new ApplicationException($"Another Test Exception thrown as Warning");
            mmApp.Log(ex,logLevel: LogLevels.Warning);

            //mmApp.ShutdownLogging();            
        }


        [TestMethod]
        public void ApplicationInsightInfoAndTest()
        {
            mmApp.InitializeLogging();            
            mmApp.LogInfo("Logging an Information message");
            mmApp.LogTrace("Logging a Trace Message");
            mmApp.ShutdownLogging();
        }
    }
}
