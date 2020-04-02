using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarkdownMonster.Annotations;
using Newtonsoft.Json;

namespace MarkdownMonster.Configuration
{
    public class PdfOutputConfiguration 
    {
        
        /// <summary>
        /// The document title. If null or empty the first 
        /// header is used which is the default..		
        /// </summary>
        public string Title { get; set; }


        /// <summary>
        /// Documents paper size Letter, 
        /// </summary>
        public PdfPageSizes PageSize { get; set; } = PdfPageSizes.Letter;

        /// <summary>
        /// Page orientation
        /// </summary>
        public PdfPageOrientation Orientation { get; set; } = PdfPageOrientation.Portrait;

        /// <summary>
        /// Text used for footers
        /// </summary>
        public string FooterText { get; set; } = "Page [page] of [topage]";


        /// <summary>
        /// Dots per inch used for images embedded in PDF
        /// </summary>
        public int ImageDpi { get; set; } = 300;


        /// <summary>
        /// Set to true if table of contents should be embedded into the PDF
        /// </summary>
        public bool GenerateTableOfContents { get; set; } = true;

        /// <summary>
        /// If true opens the PDF in the configured Windows Viewer
        /// </summary>
        public bool DisplayPdfAfterGeneration { get; set; } = true;

        public PdfPageMargins Margins { get; set; } = new PdfPageMargins();
        

        /// <summary>
        /// Location of the last saved file
        /// </summary>
        [JsonIgnore]
        public string LastOutputPath {get; set; }
    }
}
