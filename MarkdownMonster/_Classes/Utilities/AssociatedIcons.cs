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

            var ext = Path.GetExtension(filename);
            if (string.IsNullOrEmpty(ext))
                return DefaultIcon;
            
            if (Icons.TryGetValue(ext.ToLower(), out ImageSource icon))
                return icon;
            
            // check for extensions
            if (!IconUtilities.ExtensionToImageMappings.TryGetValue(ext, out string imageKey))
                imageKey = ext.Substring(1);
        
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
            Icons.Add(ext.ToLower(), icon);

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

            // special files
            { "package.json", "npm" },
            { "bower.json", "package" },

            { "license","license" },
            { ".lic" , "license" },
            { ".gitignore", "git" },
            { ".gitattributes", "git" },
            {  ".npmignore", "npm" },
            { ".editorconfig", "editorconfig" },
            { ".md", "md" },
            { ".markdown", "md" },
            { ".mdcrypt", "md" },
            { ".package.json", "package" },
            { ".bower.json", "package" },
            {  ".cs", "csharp" },
            {  ".vb", "vb" },
            {  ".fs", "fs" },
            {  ".nuspec", "nuget" },
            {  ".nupkg", "nuget" },
            { ".ts", "ts" },
            { ".js", "js" },
            { ".json", "json" },            
            {  ".tsconfig", "ts" },
            { ".html", "html" },
            { ".htm", "html" },
            { ".css", "css" },
            { ".less", "css" },
            { ".scss", "css" },
            {  ".txt", "txt" },
            {  ".log", "txt" },
            { ".cshtml", "razor" },
            { ".vbhtml", "razor" },
            { ".aspx", "aspx" },
            { ".asax", "aspx" },
            { ".asp", "aspx" },
            { ".php","php" },
            { ".py", "py" },
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
            { ".java", "java" },
            { ".sql", "sql" },            
            { ".diff", "diff" },
            { ".merge", "diff" },
            { ".pdf", "pdf" },
            { ".h", "h" },
            { ".cpp", "cpp" },
            { ".c", "cpp" },
            { ".xml", "xml" },
            { ".xsd", "xml" },
            { ".xsl", "xml" },
            { "", "xml" },
            { ".config", "config" },
            { ".manifest", "config" },
            { ".conf", "config" },
            { ".appx", "config" },
            { ".yaml", "yaml" },
            { ".yml", "yaml" },
            { ".cer", "cert" },
            { ".pfx", "cert" },
            { ".key", "key" },
            { ".png", "image" },
            { ".jpg", "image" },
            { ".jpeg", "image" },
            { ".gif", "image" },
            { ".ico", "image" },
            { ".bmp", "image" },
            { ".eps", "image" },
            { ".svg", "image" },
            { ".woff", "font" },
            { ".woff2", "font" },
            { ".otf", "font" },
            { ".eot", "font" },
            { ".ttf", "font" },
            { ".mp3", "audio" },
            { ".wmv", "audio" },
            { ".wav", "audio" },
            { ".aiff", "audio" },
            { ".mpeg", "video" },
            { ".ps1","ps1" },
            { ".dll", "bat" },
            { ".exe", "bat" },
            { ".bat", "bat" },
            { ".cmd", "bat" },
            { ".sh", "bat" }
            
        };
    }


    public class FileIconAssociation
    {
        public string Extension { get; set; }
        public string IconFile { get; set; }
        public bool IsFilename { get; set; }
    }


}
