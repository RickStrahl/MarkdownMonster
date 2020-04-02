

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MarkdownMonster.Utilities
{

    /// <summary>
    /// Same as Westwind.Utilities.ImageUtils moved here so it's accessible in .NET Core 3.0
    /// </summary>
    public static class mmImageUtils
    {
        /// <summary>
        /// Saves a jpeg BitMap  to disk with a jpeg quality setting.
        /// Does not dispose the bitmap.
        /// </summary>
        /// <param name="bmp">Bitmap to save</param>
        /// <param name="outputFileName">file to write it to</param>
        /// <param name="jpegQuality"></param>
        /// <returns></returns>
        public static bool SaveJpeg(Bitmap bmp, string outputFileName, long jpegQuality = 90)
        {
            try
            {
                //get the jpeg codec
                ImageCodecInfo jpegCodec = null;
                if (Encoders.ContainsKey("image/jpeg"))
                    jpegCodec = Encoders["image/jpeg"];

                EncoderParameters encoderParams = null;
                if (jpegCodec != null)
                {
                    //create an encoder parameter for the image quality
                    var qualityParam = new EncoderParameter(Encoder.Quality, jpegQuality);

                    //create a collection of all parameters that we will pass to the encoder
                    encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = qualityParam;
                }

                bmp.Save(outputFileName, jpegCodec, encoderParams);
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// A quick lookup for getting image encoders
        /// </summary>
        public static Dictionary<string, ImageCodecInfo> Encoders
        {
            //get accessor that creates the dictionary on demand
            get
            {
                //if the quick lookup isn't initialised, initialise it
                if (_encoders != null)
                    return _encoders;

                _encoders = new Dictionary<string, ImageCodecInfo>();

                //get all the codecs
                foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                {
                    //add each codec to the quick lookup
                    _encoders.Add(codec.MimeType.ToLower(), codec);
                }

                //return the lookup
                return _encoders;
            }
        }

        private static Dictionary<string, ImageCodecInfo> _encoders = null;


        /// <summary>
        /// Tries to return an image format 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Image format or ImageFormat.Emf if no match was found</returns>
        public static ImageFormat GetImageFormatFromFilename(string filename)
        {
            string ext = Path.GetExtension(filename).ToLower();

            ImageFormat imageFormat;

            if (ext == ".jpg" || ext == ".jpeg")
                imageFormat = ImageFormat.Jpeg;
            else if (ext == ".png")
                imageFormat = ImageFormat.Png;
            else if (ext == ".gif")
                imageFormat = ImageFormat.Gif;
            else if (ext == ".bmp")
                imageFormat = ImageFormat.Bmp;
            else
                imageFormat = ImageFormat.Emf;

            return imageFormat;
        }


        /// <summary>
        /// Returns the image media type for a give file extension based
        /// on a filename or url passed in.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetImageMediaTypeFromFilename(string file)
        {
            if (string.IsNullOrEmpty(file))
                return file;

            string ext = System.IO.Path.GetExtension(file).ToLower();
            if (ext == ".jpg" || ext == ".jpeg")
                return "image/jpeg";
            if (ext == ".png")
                return "image/png";
            if (ext == ".gif")
                return "image/gif";
            if (ext == ".bmp")
                return "image/bmp";
            if (ext == ".tif" || ext == ".tiff")
                return "image/tiff";

            return "application/image";
        }

        /// <summary>
        /// Checks to see if a font is a FixedWidth Font
        /// </summary>
        /// <param name="fontName"></param>
        /// <returns></returns>
        public static bool IsFixedWidthFont(string fontName){

            var fonts = System.Drawing.FontFamily.Families;
            var family = fonts.Where(fnt => fnt.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (family == null)
                return false;

            var f = new Form();
            var g = f.CreateGraphics();
            var font = new Font(family, 12);

            char[] charSizes = new char[] { 'i', 'a', 'Z', '%', '#', 'a', 'B', 'l', 'm', ',', '.' };
            float charWidth = g.MeasureString("I", font).Width;

            bool fixedWidth = true;

            foreach (char c in charSizes)
            {
                if (g.MeasureString(c.ToString(), font).Width != charWidth)
                    fixedWidth = false;
            }

            return fixedWidth;
        }
    }
}

