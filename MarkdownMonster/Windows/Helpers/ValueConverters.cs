using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using LibGit2Sharp;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Value Converter used to the reverse boolean value of a property (ie. !value)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertedBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool) value;
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class StringComparisonToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parm = parameter as string;
            if (value == null || parm == null)
                return false;
            return parm == value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class StringComparisonInvertedToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parm = parameter as string;
            if (value == null || parm == null)
                return true;
            return parm != value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// BUILT INTO WPF
    /// Converter used to bind a boolean to Visibility
    /// </summary>
    public class BooleanToCollapsedVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Boolean && (bool) value)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is Visibility && (Visibility) value == Visibility.Visible)
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Allows binding multiple ValueConverters as a group
    /// 
    /// https://web.archive.org/web/20130622171857/http://www.garethevans.com/linking-multiple-value-converters-in-wpf-and-silverlight
    /// </summary>
    public class ValueConverterGroup : List<IValueConverter>, IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return this.Aggregate(value,
                (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(bool))]
    public class NotEmptyStringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            if (!string.IsNullOrEmpty(text))
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(bool))]
    public class EmptyStringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            if (string.IsNullOrEmpty(text))
                return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FileNameFromPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (string.IsNullOrEmpty(path))
                return value;

            return Path.GetFileName(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FolderNameFromPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (string.IsNullOrEmpty(path))
                return value;

            var folder = Path.GetDirectoryName(path);
            folder = Path.GetFileName(folder);            
            return folder;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FullFolderNameFromPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (string.IsNullOrEmpty(path))
                return value;

            return Path.GetDirectoryName(path);            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShortFileNameDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (string.IsNullOrEmpty(path))
                return value;

            int size = System.Convert.ToInt32(parameter);
            if (size < 1)
                size = 70;

            return FileUtils.GetCompactPath(path, size);

            //string folder = Path.GetFileName(Path.GetDirectoryName(path));
            //return $"{Path.GetFileName(path)}  –  {folder}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class FontWeightFromBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (boolValue)
                    return FontWeights.SemiBold;
            }

            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class SourceControlIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is FileStatus))
                return value;

            var status = (FileStatus) value;

            if (status == FileStatus.Unaltered)
                return SourceControlIcons.Normal;
            if (status == FileStatus.ModifiedInIndex || status == FileStatus.ModifiedInWorkdir) 
                return SourceControlIcons.Changed;
            
            if (status == FileStatus.Ignored)
                return SourceControlIcons.Ignored;
            if (status == FileStatus.NewInIndex || status == FileStatus.NewInWorkdir)
                return SourceControlIcons.Added;
            if (status == FileStatus.Conflicted)
                return SourceControlIcons.Conflict;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class UriToCachedImageConverter : IValueConverter
    {
        private static Dictionary<string, BitmapImage> CachedBitmapImages = new Dictionary<string, BitmapImage>();
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value as string;

            if (!string.IsNullOrEmpty(val))
            {
                val = ((string) value).ToLower();
                    
                BitmapImage bi;
                if (CachedBitmapImages.TryGetValue(val, out bi))
                    return bi;

                try
                {
                    bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    //bi.UriSource = new Uri(value.ToString());
                    using (var fstream = new FileStream(value.ToString(), FileMode.Open, FileAccess.Read))
                    {
                        bi.StreamSource = fstream;
                        bi.EndInit();
                    }
                    CachedBitmapImages.Add(val, bi);
                    return bi;
                }
                catch { }
            }

            CachedBitmapImages.Add(val, null);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Two way conversion is not supported.");
        }
    }

}
