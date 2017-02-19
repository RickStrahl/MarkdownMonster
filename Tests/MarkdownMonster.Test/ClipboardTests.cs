using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{

    /// <summary>
    /// To run these tests make sure an image is on the clipboard
    /// </summary>
    [TestClass]
    public class ClipboardTests
    {

        public ClipboardTests()
        {
            if (!System.Windows.Forms.Clipboard.ContainsImage())
                System.Windows.Forms.Clipboard.SetImage(new Bitmap(".\\ClipboardImage.png"));
        }

        [TestMethod]
        public void BitmapPngSaveTest()
        {
            Assert.IsTrue(System.Windows.Forms.Clipboard.ContainsImage(), "No image on clipboard");

            string of = "c:\\temp\\test_Bitmap.png";

            byte[] bytes;
            using (var img = System.Windows.Forms.Clipboard.GetImage())
            {
                using (var bmp = new Bitmap(img))
                {
                    bmp.Save(of, ImageFormat.Png);
                }

            }

            //ShellUtils.GoUrl(of);
        }

        [TestMethod]
        public void BitmapJpgSaveTest()
        {
            Assert.IsTrue(System.Windows.Forms.Clipboard.ContainsImage(), "No image on clipboard");

            string of = "c:\\temp\\test_Bitmap.jpg";

            byte[] bytes;
            using (var img = System.Windows.Forms.Clipboard.GetImage())
            {
                using (var bmp = new Bitmap(img))
                {
                    bmp.Save(of, ImageFormat.Jpeg);
                }

            }
            //ShellUtils.GoUrl(of);
        }


        /// <summary>
        /// THIS FAILS WITH IMAGES PASTED FROM SNAGIT'S EdITOR WHEN SVING TO PNG
        /// but works with images from other sources.
        /// Tests above work        
        /// </summary>
        [TestMethod]
        public void BitmapWpfPngSaveTest()
        {
            Assert.IsTrue(System.Windows.Clipboard.ContainsImage(), "No image on clipboard");

            string of = "c:\\temp\\test_Bitmap.png";

            var bmpSource = System.Windows.Clipboard.GetImage();

            using (var fileStream = new FileStream(of, FileMode.Create))
            {
                BitmapEncoder encoder = null;
                encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(bmpSource));
                encoder.Save(fileStream);

                //if (ext == ".png")
                //    mmFileUtils.OptimizePngImage(sd.FileName,5); // async
            }
            Assert.IsTrue(File.Exists(of));

             
            ShellUtils.GoUrl(of);
        }
    }
}
