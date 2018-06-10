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
            mmFileUtils.EnsureBrowserEmulationEnabled("MarkdownMonster.exe", uninstall: true);
        }

        [TestMethod]
        public void EnsureBrowserEmulationTest()
        {
            mmFileUtils.EnsureBrowserEmulationEnabled("MarkdownMonster.exe");
        }
        
        // Test this only individually 
        // Otherwise the test output folder will be written as
        // the path
#if true

        [TestMethod]
        public void EnsureAssociationsForceTest()
        {
            mmFileUtils.EnsureAssociations(uninstall: true);
        }

        [TestMethod]
        public void EnsureAssociationsTest()
        {
            mmFileUtils.EnsureAssociations();
        }

        
        [TestMethod]
        public void UninstallSystemPathTest()
        {
            mmFileUtils.EnsureSystemPath(uninstall: true);
        }
        [TestMethod]
        public void EnsureSystemPathTest()
        {
            mmFileUtils.EnsureSystemPath();
        }
    }
#endif
    }
