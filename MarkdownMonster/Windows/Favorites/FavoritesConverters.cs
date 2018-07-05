using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using MarkdownMonster.Favorites;
using MarkdownMonster.Utilities;

namespace MarkdownMonster.Windows
{
    public class FavoritesTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var favorite = value as FavoriteItem;
            if (favorite == null)
                return value;
            
            string path = favorite.File;
            string file = null;
            if (!string.IsNullOrEmpty(path))
                file = System.IO.Path.GetFileName(path);

            if (!string.IsNullOrEmpty(favorite.Title))
                return favorite.Title;

            return file;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class FavoritesIconConverter : IValueConverter
    {
        private AssociatedIcons icons = new AssociatedIcons();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var favorite = value as FavoriteItem;
            if (favorite == null)
                return value;

            string file = null;

            if (favorite.IsFolder)
                file = "folder.openfolder";
            else
                file = favorite.File;

            var ext = Path.GetExtension(file);
            if (string.IsNullOrEmpty(ext))
                file = "folder.folder";

            return icons.GetIconFromFile(file);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
