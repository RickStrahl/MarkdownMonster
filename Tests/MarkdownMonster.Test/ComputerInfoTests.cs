using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class ComputerInfoTests
    {
        [TestMethod]
        public void UninstallBrowserEmulationTest()
        {
            ComputerInfo.EnsureBrowserEmulationEnabled("MarkdownMonster.exe", uninstall: true);
        }

        [TestMethod]
        public void EnsureBrowserEmulationTest()
        {
            ComputerInfo.EnsureBrowserEmulationEnabled("MarkdownMonster.exe");
        }
        
        // Test this only individually 
        // Otherwise the test output folder will be written as
        // the path
#if true

        [TestMethod]
        public void EnsureAssociationsForceTest()
        {
            ComputerInfo.EnsureAssociations(uninstall: true);
        }

        [TestMethod]
        public void EnsureAssociationsTest()
        {
            ComputerInfo.EnsureAssociations();
        }

        
        [TestMethod]
        public void UninstallSystemPathTest()
        {
            ComputerInfo.EnsureSystemPath(uninstall: true);
        }
        [TestMethod]
        public void EnsureSystemPathTest()
        {
            ComputerInfo.EnsureSystemPath();
        }
    }
#endif
    }
