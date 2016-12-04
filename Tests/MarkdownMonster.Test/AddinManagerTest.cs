using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.AddIns;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class AddinManagerTest
    {

        [TestMethod]
        public void MyTestMethod()
        {
            var manager = new AddinManager();
            manager.GetAddinList();
        }
    }
}
