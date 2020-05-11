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
        private Dictionary<string, ImageSource> Icons = new Dictionary<string, ImageSource>();

        public static ImageSource DefaultIcon;


        static AssociatedIcons()
        {
            try
            {
                DefaultIcon =
                    new BitmapImage(
                        new Uri("pack://application:,,,/MarkdownMonster;component/Assets/default_file.png"));
            }
            catch
            {

            }
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

            // check 'special files' files first
            var justFile = Path.GetFileName(filename);
            string key = null;

            if (Icons.TryGetValue(justFile.ToLower(), out ImageSource icon))
                return icon;
            if (IconUtilities.ExtensionToImageMappings.TryGetValue(justFile.ToLower(), out string imageKey))
                key = justFile.ToLower();
            else
            {
                // Check extensions next
                var ext = Path.GetExtension(filename);

                if (string.IsNullOrEmpty(ext))
                    return DefaultIcon;

                key = ext.ToLower();
                if (Icons.TryGetValue(key, out icon))
                    return icon;

                // check for extensions
                if (!IconUtilities.ExtensionToImageMappings.TryGetValue(key, out imageKey))
                    imageKey = key.Substring(1);
            }

            try
            {
                var imagePath = Path.Combine(App.InitialStartDirectory, "Editor", "fileicons", imageKey + ".png");
                if (File.Exists(imagePath))
                    icon = new BitmapImage(new Uri(imagePath));
                else
                    icon = DefaultIcon;
            }
            catch
            {
                icon = DefaultIcon;
            }

            if (!Icons.ContainsKey(key))
                Icons.Add(key, icon);

            return icon;
        }

        public ImageSource GetIconFromType(string doctype)
        {
            if (string.IsNullOrEmpty(doctype))
                return DefaultIcon;

            var kv = IconUtilities.ExtensionToImageMappings.FirstOrDefault(kval=> kval.Value.Equals(doctype,StringComparison.InvariantCultureIgnoreCase));
            if (kv.Key == null)
                return DefaultIcon;


            ImageSource icon = null;
            try
            {
                var imagePath = Path.Combine(App.InitialStartDirectory, "Editor", "fileicons", doctype + ".png");
                if (File.Exists(imagePath))
                    icon = new BitmapImage(new Uri(imagePath));
                else
                    icon = DefaultIcon;
            }
            catch
            {
                icon = DefaultIcon;
            }

            if (!Icons.ContainsKey(kv.Key))
                Icons.Add(kv.Key, icon);

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

        public static Dictionary<string, string> ExtensionToImageMappings { get; } = new Dictionary<string, string>()
        {

            // Whole Files
            {"package.json", "npm"},
            {"package-lock.json", "package"},
            {"bower.json", "package"},
            {"license.txt", "license"},
            {"favicon.ico", "favicon"},
            {"folder.folder", "folder"}, // special case
            {"folder.openfolder", "folder-open"}, // special case
            {"untitled", "md"},

            //{"kavadocstopic.md", "kavadocs"},
            {"_kavadocs.json", "kavadocs" },
            { "_toc.json", "kavadocs" },

            // special doc type
            {"kavadocs","kavadocs" },
            {"preview", "preview" },

            // dev config files
            {"license", "license"},
            {".lic", "license"},
            {".gitignore", "git"},
            {".gitattributes", "git"},
            {".npmignore", "npm"},
            {".editorconfig", "editorconfig"},

            // Common

            {".md", "md"},
            {".markdown", "md"},
            {".mkdown", "md"},
            {".mdcrypt", "md"},
            {".mdproj", "mdproj" },

            // .NET
            {".cs", "csharp"},
            {".vb", "vb"},
            {".fs", "fs"},
            {".nuspec", "nuget"},
            {".nupkg", "nuget"},
            {".csproj", "csproj"},
            {".sln", "sln"},


            // Packages
            {".package.json", "package"},
            {".bower.json", "package"},
            {".paket", "package"},

            // HTML/CSS
            {".html", "html"},
            {".htm", "html"},
            {".css", "css"},
            {".less", "less"},
            {".scss", "css"},
            {".txt", "txt"},
            {".log", "txt"},
            {".ts", "ts"},
            {".js", "js"},
            {".json", "json"},
            {".jsx", "react"},
            {".vue", "vue"},
            {".tsconfig", "ts"},

            // office docs
            {".ppt", "ppt"},
            {".pptx", "ppt"},
            {".doc", "docx"},
            {".docx", "docx"},
            {".xls","xls" },
            {".xlsx","xls" },
            {".one", "onenote"},
            {".onenote", "onenote"},
            {".pdf", "pdf"},

            // Scripts
            {".cshtml", "razor"},
            {".vbhtml", "razor"},
            {".razor", "razor"},
            {".aspx", "aspx"},
            {".asax", "aspx"},
            {".asp", "aspx"},
            {".php", "php"},
            {".rails", "rails"},
            {".rhtml", "rails"},
            {".py", "py"},

            // Foxpro
            {".prg", "prg"},
            {".fxp", "prg"},
            {".vcx", "prg"},
            {".vct", "prg"},
            {".scx", "prg"},
            {".sct", "prg"},
            {".dbf", "prg"},
            {".fpt", "prg"},
            {".cdx", "prg"},
            {".dbc", "prg"},
            {".dbt", "prg"},

            //Languages
            {".java", "java"},
            {".sql", "sql"},
            {".diff", "diff"},
            {".merge", "diff"},
            {".h", "h"},
            {".cpp", "cpp"},
            {".c", "cpp"},

            // Text Formats
            {".xml", "xml"},
            {".xsd", "xml"},
            {".xsl", "xml"},
            {".xaml", "xml"},


            // Configuration
            {".config", "config"},
            {".manifest", "config"},
            {".dotsettings", "config" },
            {".conf", "config"},
            {".appx", "config"},
            {".yaml", "yaml"},
            {".yml", "yaml"},
            {".cer", "cert"},
            {".pfx", "cert"},
            {".key", "key"},

            // Images
            {".png", "image"},
            {".jpg", "image"},
            {".jpeg", "image"},
            {".gif", "image"},
            {".ico", "image"},
            {".bmp", "image"},
            {".eps", "image"},
            {".svg", "svg"},
            {".cdr", "image"},
            {".psd", "psd"},

            // Fonts
            {".woff", "font"},
            {".woff2", "font"},
            {".otf", "font"},
            {".eot", "font"},
            {".ttf", "font"},

            // Media
            {".mp3", "audio"},
            {".wmv", "audio"},
            {".wav", "audio"},
            {".aiff", "audio"},
            {".mpeg", "video"},

            // Shell
            {".ps1", "ps1"},
            {".dll", "bat"},
            {".exe", "bat"},
            {".bat", "shell"},
            {".cmd", "shell"},
            {".sh", "shell"},
            {".zip", "zip"},
            {".7z", "zip"},
            {".reg", "reg"}

        };
    }
}
