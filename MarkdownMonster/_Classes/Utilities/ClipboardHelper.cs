using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MarkdownMonster.Windows;
using Westwind.Utilities;

namespace MarkdownMonster
{
    /// <summary>
    /// Helper to  encode and set HTML fragment to clipboard.<br/>
    /// See <br/>
    /// <seealso  cref="CreateDataObject"/>.
    ///  </summary>
    /// <remarks>
    /// The MIT License  (MIT) Copyright (c) 2014 Arthur Teplitzki.
    /// https://gist.github.com/ArthurHub/10729205
    ///  </remarks>
    public static class ClipboardHelper
    {
        #region Fields and Consts

        /// <summary>
        /// Number of retry attempts when clipboard access fails
        /// </summary>
        private static int RetyAttempts = 10;


        /// <summary>
        /// The string contains index references to  other spots in the string, so we need placeholders so we can compute the  offsets. <br/>
        /// The  <![CDATA[<<<<<<<]]>_ strings are just placeholders.  We'll back-patch them actual values afterwards. <br/>
        /// The string layout  (<![CDATA[<<<]]>) also ensures that it can't appear in the body  of the html because the <![CDATA[<]]> <br/>
        /// character must be escaped. <br/>
        /// </summary>
        private const string Header = @"Version:0.9
StartHTML:<<<<<<<<1
EndHTML:<<<<<<<<2
StartFragment:<<<<<<<<3
EndFragment:<<<<<<<<4
StartSelection:<<<<<<<<3
EndSelection:<<<<<<<<4";

        /// <summary>
        /// html comment to point the beginning of  html fragment
        /// </summary>
        public const string StartFragment = "<!--StartFragment-->";

        /// <summary>
        /// html comment to point the end of html  fragment
        /// </summary>
        public const string EndFragment = @"<!--EndFragment-->";

        /// <summary>
        /// Used to calculate characters byte count  in UTF-8
        /// </summary>
        private static readonly char[] _byteCount = new char[1];



        #endregion


        /// <summary>
        /// Create <see  cref="DataObject"/> with given html and plain-text ready to be  used for clipboard or drag and drop.<br/>
        /// Handle missing  <![CDATA[<html>]]> tags, specified startend segments and Unicode  characters.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Windows Clipboard works with UTF-8  Unicode encoding while .NET strings use with UTF-16 so for clipboard to  correctly
        /// decode Unicode string added to it from  .NET we needs to be re-encoded it using UTF-8 encoding.
        /// </para>
        /// <para>
        /// Builds the CF_HTML header correctly for  all possible HTMLs<br/>
        /// If given html contains start/end  fragments then it will use them in the header:
        ///  <code><![CDATA[<html><body><!--StartFragment-->hello  <b>world</b><!--EndFragment--></body></html>]]></code>
        /// If given html contains html/body tags  then it will inject start/end fragments to exclude html/body tags:
        ///  <code><![CDATA[<html><body>hello  <b>world</b></body></html>]]></code>
        /// If given html doesn't contain html/body  tags then it will inject the tags and start/end fragments properly:
        /// <code><![CDATA[hello  <b>world</b>]]></code>
        /// In all cases creating a proper CF_HTML  header:<br/>
        /// <code>
        /// <![CDATA[
        /// Version:1.0
        /// StartHTML:000000177
        /// EndHTML:000000329
        /// StartFragment:000000277
        /// EndFragment:000000295
        /// StartSelection:000000277
        /// EndSelection:000000277
        /// <!DOCTYPE HTML PUBLIC  "-//W3C//DTD HTML 4.0 Transitional//EN">
        ///  <html><body><!--StartFragment-->hello  <b>world</b><!--EndFragment--></body></html>
        /// ]]>
        /// </code>
        /// See format specification here: [http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp][9]
        /// </para>
        /// </remarks>
        /// <param name="html">a  html fragment</param>
        /// <param  name="plainText">the plain text</param>
        public static DataObject CreateDataObject(string html, string plainText)
        {
            html = html ?? string.Empty;
            var htmlFragment = GetHtmlDataString(html);

            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, htmlFragment);
            dataObject.SetData(DataFormats.Text, plainText);
            dataObject.SetData(DataFormats.UnicodeText, plainText);
            return dataObject;
        }

        /// <summary>
        /// Clears clipboard and sets the given  HTML and plain text fragment to the clipboard, providing additional  meta-information for HTML.<br/>
        /// See <see  cref="CreateDataObject"/> for HTML fragment details.<br/>
        /// </summary>
        /// <example>
        ///  ClipboardHelper.CopyHtmlToClipboard("Hello <b>World</b>",  "Hello World");
        /// </example>
        /// <param name="html">an html fragment</param>
        /// <param  name="plainText">the plain text</param>
        public static bool CopyHtmlToClipboard(string html, string plainText = null, bool showStatusError = false)
        {
            try
            {
                if (plainText == null)
                    plainText = html;

                var dataObject = CreateDataObject(html, plainText);
                Clipboard.SetDataObject(dataObject, true);
                return true;
            }
            catch (Exception ex)
            {
                mmApp.Log("Copy HTML to Clipboard failed", ex);
                if (showStatusError)
                    mmApp.Model.Window.ShowStatusError($"Couldn't save Html to clipboard: {ex.Message}");

                return false;
            }
        }


        /// <summary>
        /// Returns the raw HTML of HTML content on the clipboard. Unlike GetText()
        /// this method returns the rendered HTML rather than the innerText of the
        /// HTML content.
        /// </summary>
        /// <returns>HTML or null on failure</returns>
        public static string GetHtmlFromClipboard()
        {
            var html = Clipboard.GetData(DataFormats.Html) as string;
            return StringUtils.ExtractString(html, "<!--StartFragment-->", "<!--EndFragment-->");
        }

        /// <summary>
        /// Generate HTML fragment data string with  header that is required for the clipboard.
        /// </summary>
        /// <param name="html">the  html to generate for</param>
        /// <returns>the resulted  string</returns>
        private static string GetHtmlDataString(string html)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            sb.AppendLine(@"<!DOCTYPE HTML>");

            // if given html already provided the  fragments we won't add them
            int fragmentStart, fragmentEnd;
            int fragmentStartIdx = html.IndexOf(StartFragment, StringComparison.OrdinalIgnoreCase);
            int fragmentEndIdx = html.LastIndexOf(EndFragment, StringComparison.OrdinalIgnoreCase);

            // if html tag is missing add it  surrounding the given html (critical)
            int htmlOpenIdx = html.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
            int htmlOpenEndIdx = htmlOpenIdx > -1 ? html.IndexOf('>', htmlOpenIdx) + 1 : -1;
            int htmlCloseIdx = html.LastIndexOf("</html", StringComparison.OrdinalIgnoreCase);

            if (fragmentStartIdx < 0 && fragmentEndIdx < 0)
            {
                int bodyOpenIdx = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
                int bodyOpenEndIdx = bodyOpenIdx > -1 ? html.IndexOf('>', bodyOpenIdx) + 1 : -1;

                if (htmlOpenEndIdx < 0 && bodyOpenEndIdx < 0)
                {
                    // the given html doesn't  contain html or body tags so we need to add them and place start/end fragments  around the given html only
                    sb.Append("<html><body>");
                    sb.Append(StartFragment);
                    fragmentStart = GetByteCount(sb);
                    sb.Append(html);
                    fragmentEnd = GetByteCount(sb);
                    sb.Append(EndFragment);
                    sb.Append("</body></html>");
                }
                else
                {
                    // insert start/end fragments  in the proper place (related to html/body tags if exists) so the paste will  work correctly
                    int bodyCloseIdx = html.LastIndexOf("</body", StringComparison.OrdinalIgnoreCase);

                    if (htmlOpenEndIdx < 0)
                        sb.Append("<html>");
                    else
                        sb.Append(html, 0, htmlOpenEndIdx);

                    if (bodyOpenEndIdx > -1)
                        sb.Append(html, htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0,
                            bodyOpenEndIdx - (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0));

                    sb.Append(StartFragment);
                    fragmentStart = GetByteCount(sb);

                    var innerHtmlStart =
                        bodyOpenEndIdx > -1 ? bodyOpenEndIdx : (htmlOpenEndIdx > -1 ? htmlOpenEndIdx : 0);
                    var innerHtmlEnd = bodyCloseIdx > -1
                        ? bodyCloseIdx
                        : (htmlCloseIdx > -1 ? htmlCloseIdx : html.Length);
                    sb.Append(html, innerHtmlStart, innerHtmlEnd - innerHtmlStart);

                    fragmentEnd = GetByteCount(sb);
                    sb.Append(EndFragment);

                    if (innerHtmlEnd < html.Length)
                        sb.Append(html, innerHtmlEnd, html.Length - innerHtmlEnd);

                    if (htmlCloseIdx < 0)
                        sb.Append("</html>");
                }
            }
            else
            {
                // handle html with existing  startend fragments just need to calculate the correct bytes offset (surround  with html tag if missing)
                if (htmlOpenEndIdx < 0)
                    sb.Append("<html>");
                int start = GetByteCount(sb);
                sb.Append(html);
                fragmentStart = start + GetByteCount(sb, start, start + fragmentStartIdx) + StartFragment.Length;
                fragmentEnd = start + GetByteCount(sb, start, start + fragmentEndIdx);
                if (htmlCloseIdx < 0)
                    sb.Append("</html>");
            }

            // Back-patch offsets (scan only the  header part for performance)
            sb.Replace("<<<<<<<<1", Header.Length.ToString("D9"), 0, Header.Length);
            sb.Replace("<<<<<<<<2", GetByteCount(sb).ToString("D9"), 0, Header.Length);
            sb.Replace("<<<<<<<<3", fragmentStart.ToString("D9"), 0, Header.Length);
            sb.Replace("<<<<<<<<4", fragmentEnd.ToString("D9"), 0, Header.Length);

            return sb.ToString();
        }

        /// <summary>
        /// Calculates the number of bytes produced  by encoding the string in the string builder in UTF-8 and not .NET default  string encoding.
        /// </summary>
        /// <param name="sb">the  string builder to count its string</param>
        /// <param  name="start">optional: the start index to calculate from (default  - start of string)</param>
        /// <param  name="end">optional: the end index to calculate to (default - end  of string)</param>
        /// <returns>the number of bytes  required to encode the string in UTF-8</returns>
        private static int GetByteCount(StringBuilder sb, int start = 0, int end = -1)
        {
            int count = 0;
            end = end > -1 ? end : sb.Length;
            for (int i = start; i < end; i++)
            {
                _byteCount[0] = sb[i];
                count += Encoding.UTF8.GetByteCount(_byteCount);
            }

            return count;
        }


        #region Safe Clipboard Operations


        /// <summary>
        /// Safely sets the clipboard and optionally displays a status error message
        /// </summary>
        /// <param name="text"></param>
        /// <param name="showStatusError"></param>
        /// <returns></returns>
        public static bool SetText(string text, bool showStatusError = true)
        {
            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch (Exception ex)
            {
                if (showStatusError)
                    mmApp.Model.Window.ShowStatusError($"Couldn't save text to clipboard: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// Retrieves text from the clipboard and retries
        /// </summary>
        /// <returns>Clipboard text or null if it fails</returns>
        public static string GetText()
        {
            int x = 0;
            while (x++ < RetyAttempts)
            {
                try
                {
                    return Clipboard.GetText();
                }
                catch
                {
                    Thread.Sleep(5);
                }
            }

            return null;
        }


        /// <summary>
        /// Returns an image source from the clipboard if available
        /// </summary>
        /// <returns>image source or null</returns>
        public static ImageSource GetImageSource()
        {
            if (!Clipboard.ContainsImage())
                return null;

            var dataObject = Clipboard.GetDataObject();
            var formats = dataObject.GetFormats(true);
            var first = formats[0];

            // native image format should work
            if (first == DataFormats.Bitmap ||
                formats.Contains("text/_moz_htmlinfo") /* firefox check */)
            {
                return System.Windows.Clipboard.GetImage();
            }

            using (var bmp = GetImage())
            {
                // couldn't convert image
                if (bmp == null)
                    return null;

                return WindowUtilities.BitmapToBitmapSource(bmp);
            }
        }



        /// <summary>
        /// Returns an image from the clipboard and capture exception
        ///
        /// TODO: Deal with Image Transparency more reliably (DIB)
        /// https://stackoverflow.com/questions/30727343/fast-converting-bitmap-to-bitmapsource-wpf
        /// </summary>
        /// <returns>Bitmap captured or null</returns>
        public static Bitmap GetImage()
        {
            try
            {
                var dataObject = Clipboard.GetDataObject();

                var formats = dataObject.GetFormats(true);
                if (formats == null || formats.Length == 0)
                    return null;

                var first = formats[0];

                foreach (var f in formats)
                    Debug.WriteLine(" - " + f.ToString());

                if (formats.Contains("PNG"))
                {
                    Debug.WriteLine("PNG");

                    using (MemoryStream ms = (MemoryStream) dataObject.GetData("PNG"))
                    {
                        ms.Position = 0;
                        return (Bitmap) new Bitmap(ms);
                    }
                }
                // Guess at Chromium and Moz Web Browsers which can just use WPF's formatting
                else if (first == DataFormats.Bitmap || formats.Contains("text/_moz_htmlinfo"))
                {
                    Debug.WriteLine("First == Bitmap");

                    BitmapSource src = null;
                    for (int i = 0; i < 5; i++)
                    {
                        try
                        {
                            // This is notoriously unreliable so retry multiple time if it fails
                            src = Clipboard.GetImage();
                            break;
                        }
                        catch (Exception e)
                        {
                            Thread.Sleep(10);  // retry
                        }
                    }

                    if (src == null)
                        return null;

                    return WindowUtilities.BitmapSourceToBitmap(src);
                    
                }
                else if (formats.Contains("System.Drawing.Bitmap")) // (first == DataFormats.Dib)
                {
                    Debug.WriteLine("System.Drawing.Bitmap");
                    var bitmap = (Bitmap) dataObject.GetData("System.Drawing.Bitmap");
                    return bitmap;
                }

                Debug.WriteLine("Fall through - WinForms");

                return System.Windows.Forms.Clipboard.GetImage() as Bitmap;
            }
            catch
            {
                return null;
            }
        }




        /// <summary>
        /// Safe way to retrieve whether clipboard contains an image
        /// </summary>
        /// <returns></returns>
        public static bool ContainsImage()
        {
            int x = 0;
            while (x++ < RetyAttempts)
            {
                try
                {
                    return Clipboard.ContainsImage();
                }
                catch
                {
                    Thread.Sleep(5);
                }
            }

            return false;
        }

        /// <summary>
        /// Safe way to retrieve whether clipboard contains an image
        /// </summary>
        /// <returns></returns>
        public static bool ContainsText()
        {
            int x = 0;
            while (x++ < RetyAttempts)
            {
                try
                {
                    return Clipboard.ContainsText();
                }
                catch
                {
                    Thread.Sleep(5);
                }
            }

            return false;
        }

        #endregion
    }

#if false

        // https://thomaslevesque.com/2009/02/05/wpf-paste-an-image-from-the-clipboard/
        // doesn't preserve Transparency
        private static ImageSource ImageFromClipboardDib()
        {
            MemoryStream ms = Clipboard.GetData("Format17") as MemoryStream;
            if (ms != null)
            {
                byte[] dibBuffer = new byte[ms.Length];
                ms.Read(dibBuffer, 0, dibBuffer.Length);

                BITMAPINFOHEADER infoHeader =
                    BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(dibBuffer);

                int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
                int infoHeaderSize = infoHeader.biSize;
                int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

                BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
                fileHeader.bfType = BITMAPFILEHEADER.BM;
                fileHeader.bfSize = fileSize;
                fileHeader.bfReserved1 = 0;
                fileHeader.bfReserved2 = 0;
                fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;

                byte[] fileHeaderBytes =
                    BinaryStructConverter.ToByteArray<BITMAPFILEHEADER>(fileHeader);

                MemoryStream msBitmap = new MemoryStream();
                msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
                msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
                msBitmap.Seek(0, SeekOrigin.Begin);

                return BitmapFrame.Create(msBitmap);
            }
            return null;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct BITMAPFILEHEADER
        {
            public static readonly short BM = 0x4d42; // BM

            public short bfType;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

    }

    public static class BinaryStructConverter
    {
        public static T FromByteArray<T>(byte[] bytes) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                object obj = Marshal.PtrToStructure(ptr, typeof(T));
                return (T) obj;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public static byte[] ToByteArray<T>(T obj) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }
#endif

}
