using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class PasteCode : MetroWindow
    {

        public AppModel AppModel { get; set; }


        public string Code
        {
            get { return _code; }
            set
            {
                _code = StringUtils.NormalizeIndentation(value);
            }
        }
        private string _code;
        

        public string CodeLanguage
        {
            get { return mmApp.Configuration.DefaultCodeSyntax; }
            set { mmApp.Configuration.DefaultCodeSyntax = value; }
        }

        public Dictionary<string,string> LanguageNames { get; set; }

        private MarkdownEditorSimple editor;

        public Brush ComboBackground { get; set; }

        public PasteCode()
        {

            LanguageNames = new Dictionary<string, string>()
            {
                {"csharp", "CSharp"},
                {"html", "Html"},
                {"css", "Css"},
                {"javascript", "JavaScript"},
                {"typescript", "TypeScript"},
                {"json", "Json"},
                {"xml", "Xml" },                

                {"sql","SQL" },
                {"vbnet", "Vb.Net"},
                {"fsharp", "Fsharp"},
                {"cpp", "C++"},
                {"foxpro", "FoxPro"},

                {"ruby", "Ruby"},
                {"python", "Python"},
                {"php", "PHP"},
                {"java", "Java"},
                {"swift", "Swift"},
                {"objectivec", "Objective C"},                                
                {"vbscript", "VB Script"},
                {"haskell", "Haskel" },
                {"go", "Go" },
                
                {"dockerfile", "Docker file"},
                {"makefile", "Make file"},
                {"nginx", "NgInx"},
                
                {"markdown","Markdown" },
                {"yaml", "Yaml" },

                { "powershell", "PowerShell"},
                {"dos", "DOS"},
                {"bash", "Bash" },
                {"ini", "INI files" },
                {"dns", "DNS"},
                {"perl", "Perl"},
                {"diff", "Diff file"},
                

                {"txt", "Text - plain text, no formatting" }                
            };
            CodeLanguage = mmApp.Configuration.DefaultCodeSyntax;

            InitializeComponent();

            AppModel = mmApp.Model;
          
            mmApp.SetThemeWindowOverride(this);            

            Loaded += PasteCode_Loaded;
            PreviewKeyDown += PasteCode_PreviewKeyDown;

            WebBrowserCode.Visibility = Visibility.Hidden;
        }

     

        private void PasteCode_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBackground = TextCodeLanguage.Background;

            DataContext = this;
            WebBrowserCode.Focus();
            

            editor = new MarkdownEditorSimple(WebBrowserCode, Code, CodeLanguage);
            editor.IsDirtyAction = () =>
            {
                Code = editor.GetMarkdown();
                return true;
            };

            Dispatcher.InvokeAsync(() =>
            {
                if (!string.IsNullOrEmpty(Code))
                {
                    TextCodeLanguage.Focus();
                    TextCodeLanguage.Background = Brushes.SlateGray;

                    var sb = Resources["StoryboardLanguageCombo"] as Storyboard;
                    sb.Begin();
                }
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);

        }

       
        /// <summary>
        /// Handle default keys but ignore the Code editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasteCode_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (TextCodeLanguage.IsDropDownOpen)
                    return;

                Button_Click(ButtonOk, null);
                return;
            }
            if (e.Key == Key.Enter && e.OriginalSource != WebBrowserCode)
            {
                if (TextCodeLanguage.IsDropDownOpen)
                    return;

                Button_Click(ButtonOk, null);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == ButtonCancel)
                DialogResult = false;
            else
                DialogResult = true;

            Close();
        }

        private void TextCodeLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            editor?.SetEditorSyntax(CodeLanguage);
        }

        private void TextCodeLanguage_LostFocus(object sender, RoutedEventArgs e)
        {
            ((ComboBox) sender).Background = this.ComboBackground;

        }

        
        
    }
}
