using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class ComputerInfoTests
    {
        [TestMethod]
        public void EnsureAssociationsTest()
        {
            ComputerInfo.EnsureAssociations();
        }

        [TestMethod]
        public void EnsureAssociationsForceTest()
        {
            ComputerInfo.EnsureAssociations(true);
        }
    }
}
