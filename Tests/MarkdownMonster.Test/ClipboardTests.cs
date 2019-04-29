using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
		string TempImagePath = Path.Combine(Path.GetTempPath(),"c:\\temp\\test_Bitmap.png");

        public ClipboardTests()
        {
            if (!System.Windows.Forms.Clipboard.ContainsImage())
                System.Windows.Forms.Clipboard.SetImage(new Bitmap(".\\ClipboardImage.png"));
        }

        [TestMethod]
        public void BitmapPngSaveTest()
        {
            Assert.IsTrue(System.Windows.Forms.Clipboard.ContainsImage(), "No image on clipboard");

            
            
            using (var img = System.Windows.Forms.Clipboard.GetImage())
            {
                using (var bmp = new Bitmap(img))
                {
                    bmp.Save(TempImagePath, ImageFormat.Png);
                }

            }

            //ShellUtils.GoUrl(of);
        }

        [TestMethod]
        public void BitmapJpgSaveTest()
        {
            Assert.IsTrue(System.Windows.Forms.Clipboard.ContainsImage(), "No image on clipboard");

			string of = Path.Combine(Path.GetTempPath(), "test_Bitmap.png");

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

            

            var bmpSource = System.Windows.Clipboard.GetImage();

            using (var fileStream = new FileStream(TempImagePath, FileMode.Create))
            {
                BitmapEncoder encoder = null;
                encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(bmpSource));
                encoder.Save(fileStream);

                //if (ext == ".png")
                //    mmFileUtils.OptimizePngImage(sd.FileName,5); // async
            }
            Assert.IsTrue(File.Exists(TempImagePath));

             
            ShellUtils.GoUrl(TempImagePath);
        }
    }
}
