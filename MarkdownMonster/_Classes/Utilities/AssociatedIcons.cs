using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MarkdownMonster.Windows;

namespace MarkdownMonster.Utilities
{
    /// <summary>
    /// Helper class that allows retrieving of associated icons for given file types.
    /// This class caches files by extensions and returns an image source or a default
    /// image source for unknown files.    
    /// </summary>
    public class AssociatedIcons
    {
        private Dictionary<string,ImageSource> Icons = new Dictionary<string,ImageSource>();

        public static ImageSource DefaultIcon = null;

        static AssociatedIcons()
        {
            DefaultIcon = new BitmapImage(new Uri("pack://application:,,,/Assets/defaulticon.png", UriKind.RelativeOrAbsolute));            
        }

        /// <summary>
        /// Gets an Icon as an image source from the file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ImageSource GetIconFromFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return DefaultIcon;

            var ext = Path.GetExtension(filename);
            if (string.IsNullOrEmpty(ext))
                return DefaultIcon;

            if (Icons.TryGetValue(ext.ToLower(), out ImageSource icon))
                return icon;
            
            try
            {
                var icn = Icon.ExtractAssociatedIcon(filename);                                
                icon = icn.ToImageSource();
                Icons.Add(ext, icon);
            }
            catch
            {
                icon = DefaultIcon;                
            }

            return icon;
        }

    }

    public static class IconUtilities
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ToImageSource(this Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }

        static Dictionary<string, string> ExtensionToImageMappings { get; } = new Dictionary<string, string>() {
            {  "cs", "csharp" },
            {  "txt", "txt" },
            { "prg", "foxpro" },
            { "jpg", "image" },
            { "png", "image" },
            { "gif", "image" },
            { "bmp", "image" }
        };
    }


}
