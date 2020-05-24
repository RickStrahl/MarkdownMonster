using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MarkdownMonster.Annotations;

namespace MarkdownMonster
{

    /// <summary>
    /// Configuration contained class that holds Markdown options applied as
    /// available to the Markdown Parser (mostly to default MarkDig parser)
    /// </summary>
    public class MarkdownOptionsConfiguration : INotifyPropertyChanged
    {

        #region Markdig Configurations

        /// <summary>
        /// Determines whether links are automatically expanded
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/AutoLinks.md
        /// </summary>
        public bool AutoLinks { get; set; } = true;

        /// <summary>
        /// Determines if headers automatically generate ids. We use the Github Flavored version of it.
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/AutoIdentifierSpecs.md
        /// </summary>
        public bool AutoHeaderIdentifiers { get; set; } = true;

        /// <summary>
        /// If true strips Yaml FrontMatter from markdown header
        /// </summary>
        public bool StripYamlFrontMatter { get; set; } = true;

        /// <summary>
        /// Sets support for PipeTables and GridTables
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/PipeTableSpecs.md
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/GridTableSpecs.md
        /// </summary>
        public bool UseTables { get; set; } = true;

        /// <summary>
        /// Enables Footers and Footnotes
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/FigureFooterAndCiteSpecs.md
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/FootnotesSpecs.md
        /// </summary>
        public bool FootersAndFootnotes { get; set; } = true;


        /// <summary>
        /// Adds additional list features like a. b.  and roman numerals i. ii. ix.
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/ListExtraSpecs.md
        /// </summary>
        public bool ListExtras { get; set; } = true;


        /// <summary>
        /// If true expand Emoji in the format of :smile: and common Smileys  like :-)
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/EmojiSpecs.md
        /// </summary>
        public bool EmojiAndSmiley { get; set; } = true;

        /// <summary>
        /// Creates playable media links from music and video files
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/MediaSpecs.md
        /// </summary>
        public bool MediaLinks { get; set; } = true;


        /// <summary>
        /// Figure referencing below images
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/FigureFooterAndCiteSpecs.md
        /// </summary>
        public bool Figures { get; set; } = true;

        /// <summary>
        /// Creates Github task lists like - [ ] Task 1
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/TaskListSpecs.md
        /// </summary>
        public bool GithubTaskLists { get; set; } = true;


        /// <summary>
        /// Allows displaying mathematic formulas.
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/MathSpecs.md
        /// 
        /// Note: Also requires that 'Edit->Allow Script Tags in Markdown' is enabled.
        /// <script type="text/javascript" src="http://cdn.mathjax.org/mathjax/latest/MathJax.js?config=TeX-AMS-MML_HTMLorMML"></script>
        /// <script type="text/javascript" src="http://code.jquery.com/jquery-1.8.0.min.js"></script>
        /// <script type="text/javascript" src="http://js2math.github.com/JsMath/jsmath.tablet.js"></script>
        /// </summary>
        public bool UseMathematics { get; set; } = true;


        /// <summary>
        /// Allows for Mermaid to be rendered via external graphics library and embedded script
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/DiagramsSpecs.md
        ///
        /// Note: Also requires that 'Edit->Allow Script Tags in Markdown' is enabled.
        /// </summary>
        public bool MermaidDiagrams { get; set; } = false;

        /// <summary>
        /// Use Abbreviations which are linked to definitions
        /// </summary>
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/AbbreviationSpecs.md
        public bool Abbreviations { get; set; } = true;

        /// <summary>
        /// Fenced code blocks for a &lt;div&gt; wrapper using :::notebox / :::
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/CustomContainerSpecs.md
        /// </summary>
        public bool CustomContainers { get; set; } = true;

        /// <summary>
        /// Allows for attribute syntax `{#id .class [color:red]}`
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/GenericAttributesSpecs.md
        /// </summary>
        public bool GenericAttributes { get; set; } = false;


        /// <summary>
        /// Converts common typeographic options like -- to em dash
        /// quotes to curly quotes, triple dots to elipsis etc.
        /// https://github.com/lunet-io/markdig/blob/master/src/Markdig.Tests/Specs/SmartyPantsSpecs.md
        /// </summary>
        public bool SmartyPants { get; set; }


        /// <summary>
        /// If true inline HTML blocks are not rendered
        /// </summary>
        public bool NoHtml { get; set; }

        /// <summary>
        /// Renders all links as external links with `target='top'`
        /// </summary>
        public bool RenderLinksAsExternal { get; set; }


        /// <summary>
        /// Gets or sets the Markdig extensions to be enabled.
        /// Allows you to add extensions dynamically at runtime
        /// or set non-supported (via these options) settings
        ///
        /// This shouldn't be needed - use the options instead
        /// but this can be used in case Markdig adds extensions
        /// that aren't exposed here.
        ///
        /// Comma or + separated list of extension names:
        /// gridtables+pipetables+customcontainers
        ///
        /// If options are availe
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
        private string _markdigExtensions = string.Empty;

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

        /// <summary>
        /// Parses some DocFx expressions like note boxes, and file includes
        /// This setting requires a restart to be applied.
        /// </summary>
        public bool ParseDocFx
        {
            get => _parseDocFxIncludeFiles;
            set
            {
                if (value == _parseDocFxIncludeFiles) return;
                _parseDocFxIncludeFiles = value;
                OnPropertyChanged();
            }
        }
        private bool _parseDocFxIncludeFiles = true;


        private string _markdownParserName;
        private bool _markdownLinting;


        /// <summary>
        /// Determines whether the Markdown Linting window is visible.
        /// </summary>
        public bool MarkdownLinting
        {
            get => _markdownLinting;
            set
            {
                if (value == _markdownLinting) return;
                _markdownLinting = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Allows configuration of certain markdown sequences that have multiple
        /// representations.
        /// </summary>
        public MarkdownSymbolsConfiguration MarkdownSymbolsConfiguration { get; set; } = new MarkdownSymbolsConfiguration();


        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    /// <summary>
    /// Markdown Symbols to use for certain text expansions.
    /// </summary>
    public class MarkdownSymbolsConfiguration
    {
        public string Italic { get; set; } = "*";   // "_"
        public string SoftReturn { get; set; } = "  ";  // "\\" or "<br />"
    }
}
