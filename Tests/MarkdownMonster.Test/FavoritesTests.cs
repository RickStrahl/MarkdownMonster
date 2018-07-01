using System;
using Markdig.Syntax;
using MarkdownMonster.Favorites;
using MarkdownMonster.Windows.DocumentOutlineSidebar;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class FavoritesTests
    {
        [TestMethod]
        public void LoadFavoritesTest()
        {

            var model = new FavoritesModel();

            bool result = model.LoadFavorites();

            Assert.IsTrue(result);
            Assert.IsNotNull(model.Favorites);

        }
    }
}
