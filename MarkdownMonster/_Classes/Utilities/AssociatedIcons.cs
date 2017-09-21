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
            DefaultIcon = new BitmapImage(new Uri(Path.Combine(Environment.CurrentDirectory, "Editor", "fileicons", "default_file.png")));            
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
            if (IconUtilities.ExtensionToImageMappings.TryGetValue(justFile, out string imageKey))
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
                var imagePath  = Path.Combine(Environment.CurrentDirectory, "Editor", "fileicons", imageKey + ".png");
                if (File.Exists(imagePath))                
                    icon = new BitmapImage(new Uri(imagePath));                                    
               else
                    icon = DefaultIcon;            
            }
            catch
            {
                icon = DefaultIcon;                
            }
            Icons.Add(key, icon);

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

        public static Dictionary<string, string> ExtensionToImageMappings { get; } = new Dictionary<string, string>() {

                      // Whole Files
            { "package.json", "npm" },
            { "package-lock.json", "package" },
            { "bower.json", "package" },
            { "license.txt", "license" },

            // dev config files
            { "license","license" },
            { ".lic" , "license" },
            { ".gitignore", "git" },
            { ".gitattributes", "git" },
            { ".npmignore", "npm" },
            { ".editorconfig", "editorconfig" },

            // Common
            { ".md", "md" },
            { ".markdown", "md" },
            { ".mdcrypt", "md" },

            // .NET
            {  ".cs", "csharp" },
            {  ".vb", "vb" },
            {  ".fs", "fs" },
            {  ".nuspec", "nuget" },
            {  ".nupkg", "nuget" },
            {  ".csproj", "csproj" },
            {  ".sln", "sln" },
           

            // Packages
            { ".package.json", "package" },
            { ".bower.json", "package" },
            { ".paket", "package" },
            
            // HTML/CSS
            { ".html", "html" },
            { ".htm", "html" },
            { ".css", "css" },
            { ".less", "css" },
            { ".scss", "css" },
            {  ".txt", "txt" },
            {  ".log", "txt" },
            { ".ts", "ts" },
            { ".js", "js" },
            { ".json", "json" },
            {  ".tsconfig", "ts" },

            // office docs
            {  ".ppt", "ppt" },
            {  ".docx", "docx" },
            {  ".one", "onenote" },
            {  ".onenote", "onenote" },
            { ".pdf", "pdf" },

            // Scripts
            { ".cshtml", "razor" },
            { ".vbhtml", "razor" },
            { ".aspx", "aspx" },
            { ".asax", "aspx" },
            { ".asp", "aspx" },
            { ".php","php" },
            { ".py", "py" },

            // Foxpro
            { ".prg", "prg" },
            { ".fxp", "prg" },
            { ".vcx", "prg" },
            { ".vct", "prg" },
            { ".scx", "prg" },
            { ".sct", "prg" },
            { ".dbf", "prg" },
            { ".fpt", "prg" },
            { ".cdx", "prg" },
            { ".dbc", "prg" },
            { ".dbt", "prg" },

            //Languages
            { ".java", "java" },
            { ".sql", "sql" },
            { ".diff", "diff" },
            { ".merge", "diff" },
            { ".h", "h" },
            { ".cpp", "cpp" },
            { ".c", "cpp" },

            // Text Formats
            { ".xml", "xml" },
            { ".xsd", "xml" },
            { ".xsl", "xml" },
            { ".xaml", "xml" },


            // Configuration
            { ".config", "config" },
            { ".manifest", "config" },
            { ".conf", "config" },
            { ".appx", "config" },
            { ".yaml", "yaml" },
            { ".yml", "yaml" },
            { ".cer", "cert" },
            { ".pfx", "cert" },
            { ".key", "key" },

            // Images
            { ".png", "image" },
            { ".jpg", "image" },
            { ".jpeg", "image" },
            { ".gif", "image" },
            { ".ico", "image" },
            { ".bmp", "image" },
            { ".eps", "image" },
            { ".svg", "svg" },
            { ".psd", "image" },
            { ".cdr", "image" },
            { ".woff", "font" },
            { ".woff2", "font" },
            { ".otf", "font" },
            { ".eot", "font" },
            { ".ttf", "font" },

            // Media
            { ".mp3", "audio" },
            { ".wmv", "audio" },
            { ".wav", "audio" },
            { ".aiff", "audio" },
            { ".mpeg", "video" },

            // Shell
            { ".ps1","ps1" },
            { ".dll", "bat" },
            { ".exe", "bat" },
            { ".bat", "bat" },
            { ".cmd", "bat" },
            { ".sh", "bat" },
            { ".zip", ".zip" }

        };
    }

    


}
