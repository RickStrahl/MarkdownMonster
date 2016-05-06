using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace JoeBlogs
{
    static class ImagesUtility
    {
        /// <summary>
        /// Converts the image to a byte array.
        /// </summary>
        /// <param name="imageToConvert">The image to convert.</param>
        /// <param name="formatOfImage">The format of image.</param>
        /// <returns></returns>
        public static byte[] ConvertImageToByteArray(Image imageToConvert, ImageFormat formatOfImage)
        {
            byte[] Ret;

            using (MemoryStream ms = new MemoryStream())
            {
                imageToConvert.Save(ms, formatOfImage);
                Ret = ms.ToArray();
            }

            return Ret;
        }

        /// <summary>
        /// Converts the byte array to image.
        /// </summary>
        /// <param name="byteArray">The byte array.</param>
        /// <returns></returns>
        public static Image ConvertByteArrayToImage(byte[] byteArray)
        {
            if (byteArray != null)
            {
                MemoryStream ms = new MemoryStream(byteArray, 0,
                byteArray.Length);
                ms.Write(byteArray, 0, byteArray.Length);
                return Image.FromStream(ms, true);
            }
            return null;
        }
    }
}