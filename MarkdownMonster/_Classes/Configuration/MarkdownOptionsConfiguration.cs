using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarkdownMonster.Annotations;

namespace MarkdownMonster
{

    /// <summary>
    /// Configuration contained class that holds Markdown options
    /// applied as available to the Markdown Parser (mostly to default MarkDig parser)
    /// </summary>
    public class MarkdownOptionsConfiguration : INotifyPropertyChanged
    {

        #region Markdig Configurations

        /// <summary>
        /// Determines whether links are automatically expanded
        /// </summary>
        public bool AutoLinks { get; set; } = true;

        /// <summary>
        /// Use Abbreviations which are linked to definitions
        /// </summary>
        public bool Abbreviations { get; set; } = true;

        /// <summary>
        /// Determines if headers automatically generate 
        /// ids.
        /// </summary>
        public bool AutoHeaderIdentifiers { get; set; }

        /// <summary>
        /// If true strips Yaml FrontMatter headers
        /// </summary>
        public bool StripYamlFrontMatter { get; set; } = true;

        /// <summary>
        /// If true expand Emoji in the format of :smile: and common Smileys  like :-)
        /// </summary>
        public bool EmojiAndSmiley { get; set; } = true;

        /// <summary>
        /// Creates playable media links from music and video files
        /// </summary>
        public bool MediaLinks { get; set; } = true;

        /// <summary>
        /// Adds additional list features like a. b.  and roman numerals i. ii. ix.
        /// </summary>
        public bool ListExtras { get; set; } = true;

        /// <summary>
        /// Figure referencing below images
        /// </summary>
        public bool Figures { get; set; } = true;

        /// <summary>
        /// Creates Github task lists like - [ ] Task 1
        /// </summary>
        public bool GithubTaskLists { get; set; } = true;

        /// <summary>
        /// Converts common typeographic options like -- to em dash
        /// quotes to curly quotes, triple dots to elipsis etc.
        /// </summary>
        public bool SmartyPants { get; set; }

        /// <summary>
        /// Renders Mermaid and Nonoml markup 
        /// </summary>
        public bool Diagrams { get; set; }


        /// <summary>
        /// Renders all links as external links with `target='top'`
        /// </summary>
        public bool RenderLinksAsExternal { get; set; }

        /// <summary>
        /// Gets or sets the Markdig extensions to be enabled.
        /// </summary>
        public string MarkdigExtensions
        {
            get { return _markdigExtensions; }
            set
            {
                if (value == _markdigExtensions) return;                
                _markdigExtensions = value;
                OnPropertyChanged();
            }
        }
        private string _markdigExtensions = "emphasisextras,pipetables,gridtables,footers,footnotes,citations,attributes";
        
        #endregion

        #region Miscellaneous Settings


        /// <summary>
        /// Determines whether the Markdown rendering allows script tags 
        /// in generated HTML output. Set this to true
        /// if you want to allow script tags to be rendered into
        /// HTML script tags and execute - such as embedding
        /// Gists or other Widgets that use scripts.        
        /// </summary>
        public bool AllowRenderScriptTags
        {
            get { return _allowRenderScriptTags; }
            set
            {
                if (value == _allowRenderScriptTags) return;
                _allowRenderScriptTags = value;
                OnPropertyChanged();
            }
        }
        private bool _allowRenderScriptTags;


        /// <summary>
        /// Markdown configuration initialization
        /// </summary>
        public MarkdownOptionsConfiguration()
        {
            _markdownParserName = MarkdownParserFactory.DefaultMarkdownParserName;
        }

        /// <summary>
        /// The name of the Markdown Parser used to render
        /// output. New parsers or parser configurations can be 
        /// added via Addins.
        /// </summary>        
        public string MarkdownParserName
        {
            get { return _markdownParserName; }
            set
            {
                if (value == _markdownParserName) return;
                _markdownParserName = value;
                OnPropertyChanged();
            }
        }

        


        private string _markdownParserName;

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
