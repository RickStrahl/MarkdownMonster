using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarkdownMonster.Annotations;

namespace MarkdownMonster.Configuration
{
    /// <summary>
    /// Holds all Image Configuration Options related configuration options
    /// </summary>
    public class ImageConfiguration
    {
        /// <summary>
        /// Image editor used to edit images. Empty uses system default editor
        /// </summary>
        public string ImageEditor { get; set; }

        /// <summary>
        /// Image viewer used to open images. Empty setting uses the default viewer
        /// </summary>
        public string ImageViewer { get; set; }


        /// <summary>
        /// Jpeg Image Compression level from 50 to 100. Defaults 80.
        /// </summary>
        public int JpegImageCompressionLevel { get; set; } = 80;


        /// <summary>
        /// Last Image Width Saved in Image Dialog so it can be restored
        /// </summary>
        public int LastImageWidth { get; set; } = 970;

        /// <summary>
        /// Last Image Height Saved in Image Dialog so it can be restored
        /// </summary>
        public int LastImageHeight { get; set; } = 700;
    }


}
