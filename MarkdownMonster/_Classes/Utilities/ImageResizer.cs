using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Westwind.Utilities;

namespace MarkdownMonster.Utilities
{

    public enum ResizeModes
    {
        ByWidth,
        ByHeight,
        DontKeepAspectRatio,
        Auto // longer of the two sides gets resized
    }
    public class ImageResizer
    {

        /// <summary>
        /// Resizes an image from a bitmap either based on the width or height depending on mode.
        /// Note image will resize to the larger of the two sides
        /// </summary>
        /// <param name="bmp">Bitmap to resize</param>
        /// <param name="width">new width</param>
        /// <param name="height">new height</param>
        /// <returns>resized or original bitmap. Be sure to Dispose this bitmap</returns>
        public static Bitmap ResizeImageByMode(Bitmap bmp, int width, int height, ResizeModes resizeMode,
                                         InterpolationMode mode = InterpolationMode.HighQualityBicubic)
        {
            if (resizeMode == ResizeModes.Auto)
                return ImageUtils.ResizeImage(bmp, width, height, mode);

            Bitmap bmpOut = null;

            try
            {
                decimal ratio;
                int newWidth = 0;
                int newHeight = 0;

                // If the image is smaller than a thumbnail just return original size
                if (resizeMode == ResizeModes.DontKeepAspectRatio)
                {
                    newWidth = width;
                    newHeight = height;
                }
                else if(bmp.Width < width && bmp.Height < height)
                {
                    newWidth = bmp.Width;
                    newHeight = bmp.Height;
                }
                else
                {
                    if (resizeMode == ResizeModes.ByWidth)
                    {
                        ratio = (decimal)width / bmp.Width;
                        newWidth = width;
                        decimal lnTemp = bmp.Height * ratio;
                        newHeight = (int)lnTemp;
                    }
                    else
                    {
                        ratio = (decimal)height / bmp.Height;
                        newHeight = height;
                        decimal lnTemp = bmp.Width * ratio;
                        newWidth = (int)lnTemp;
                    }
                }

                bmpOut = new Bitmap(newWidth, newHeight);
                bmpOut.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

                using (Graphics g = Graphics.FromImage(bmpOut))
                {
                    g.InterpolationMode = mode;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
                    g.DrawImage(bmp, 0, 0, newWidth, newHeight);
                }
            }
            catch
            {
                return null;
            }

            return bmpOut;
        }

    }
}
