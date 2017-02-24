using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class MarkdownParserFactoryTest
    {
        [TestCleanup]
        public void TearDown()
        {
            AddIns.AddinManager.Current.AddIns.Clear();
            MarkdownParserFactory.CurrentParser = null;
        }

        [TestMethod]
        public void ShouldReturnListOfParserNamesContainingTheDefaultParserNameWhenNoAddinsLoaded()
        {
            List<string> names = MarkdownParserFactory.GetParserNames();
            Assert.AreEqual(1, names.Count);
            Assert.AreEqual(MarkdownParserFactory.DefaultMarkdownParserName, names.First());
        }

        [TestMethod]
        public void ShouldReturnListOfParserNamesLoaded()
        {
            _loadAddin(new CustomParserAddin());

            var names = MarkdownParserFactory.GetParserNames();
            Assert.AreEqual(2, names.Count);

            CollectionAssert.AreEqual(new List<string> { MarkdownParserFactory.DefaultMarkdownParserName, "CustomParser" }, names);
        }

        [TestMethod]
        public void ShouldReturnTheDefaultParserWhenNoArgsGiven()
        {

            var parser = MarkdownParserFactory.GetParser();
            Assert.IsInstanceOfType(parser, typeof(MarkdownParserMarkdig));
        }

        [TestMethod]
        public void ShouldReturnTheSpecifiedParser()
        {
            _loadAddin(new CustomParserAddin());

            var parser = MarkdownParserFactory.GetParser(addinId: "CustomParser");
            Assert.IsInstanceOfType(parser, typeof(CustomParser));
        }

        [TestMethod]
        public void ShouldReturnTheCachedParserWhenForceLoadArgIsFalse()
        {
            _loadAddin(new CustomParserAddin());

            MarkdownParserFactory.GetParser();
            var parser = MarkdownParserFactory.GetParser(forceLoad: false, addinId: "CustomParser");
            Assert.IsInstanceOfType(parser, typeof(MarkdownParserMarkdig));
        }

        [TestMethod]
        public void ShouldReturnTheSpecifiedParserWhenForceLoadArgIsTrue()
        {
            _loadAddin(new CustomParserAddin());

            MarkdownParserFactory.GetParser();
            var parser = MarkdownParserFactory.GetParser(forceLoad: true, addinId: "CustomParser");
            Assert.IsInstanceOfType(parser, typeof(CustomParser));
        }

        [TestMethod]
        public void ShouldReturnTheDefaultMarkdownParserWhenUnkownParserNameGiven()
        {
            var parser = MarkdownParserFactory.GetParser(addinId: "BogusParser");
            Assert.IsInstanceOfType(parser, typeof(MarkdownParserMarkdig));
        }

        [TestMethod]
        public void ShouldFetchAddinParserByAddinId()
        {
            _loadAddin(new CustomParserAddin());
            var parser = MarkdownParserFactory.GetParser(addinId: "CustomParserAddin");
            Assert.IsInstanceOfType(parser, typeof(CustomParser));
        }
        private void _loadAddin(AddIns.MarkdownMonsterAddin addin)
        {
            AddIns.AddinManager.Current.AddIns.Add(addin);
            foreach (var ai in AddIns.AddinManager.Current.AddIns)
            {
                ai.OnApplicationStart();
            }
        }
        
    }

    class CustomParserAddin : AddIns.MarkdownMonsterAddin
    {

        public override void OnApplicationStart()
        {
            Id = "CustomParserAddin";
            Name = "CustomParser";
        }

        public override IMarkdownParser GetMarkdownParser()
        {
            return new CustomParser();
        }
    }

    class CustomParser : IMarkdownParser
    {
        public string Parse(string markdown)
        {
            return string.Empty;
        }
    }
}
