using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class MarkdownParserFactoryTest
    {
        [TestMethod]
        public void ShouldReturnListOfParserNamesContainingTheDefaultParserNameWhenNoAddinsLoaded()
        {
            var names = MarkdownParserFactory.GetParserNames();
            Assert.AreEqual(1, names.Count);
            Assert.AreEqual(MarkdownParserFactory.DefaultMarkdownParserName, names.First());
        }
    }
}
