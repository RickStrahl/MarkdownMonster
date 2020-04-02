using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarkdownMonster.Annotations;

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

        public PdfPageOrientation Orientation { get; set; } = PdfPageOrientation.Portrait;

        public string FooterText { get; set; } = "Page [page] of [topage]";

        public int ImageDpi { get; set; } = 300;

        public bool GenerateTableOfContents { get; set; } = true;
        
        public bool DisplayPdfAfterGeneration { get; set; } = true;

        
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
