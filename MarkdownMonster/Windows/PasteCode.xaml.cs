using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class PasteCode : MetroWindow
    {

        public string Code { get; set; }        
        public string CodeLanguage { get; set; }
        public Dictionary<string,string> LanguageNames { get; set; }
        
        public PasteCode()
        {

            LanguageNames = new Dictionary<string, string>()
            {
                {"cs", "Csharp"},
                {"vbnet", "Vb.Net"},
                {"cpp", "C++"},
                {"foxpro", "FoxPro"},
                {"fsharp", "Fsharp"},
              
                {"html", "Html"},
                {"css", "Css"},
                {"javascript", "JavaScript"},
                {"typescript", "TypeScript"},
                {"json", "Json"},

                {"sql","SQL" },                

                {"ruby", "Ruby"},
                {"python", "Python"},
                {"php", "PHP"},
                {"java", "Java"},
                {"swift", "Swift"},
                {"objectivec", "Objective C"},                                
                {"vbscript", "VB Script"},

                {"dockerfile", "Docker file"},
                {"makefile", "Make file"},
                {"nginx", "NgInx"},

                { "markdown","Markdown" },

                {"powershell", "PowerShell"},
                {"dos", "DOS"},
                {"dns", "DNS"},
                {"perl", "Perl"},
                {"diff", "Diff file"},
                
            };
            CodeLanguage = "cs";

            InitializeComponent();

            DataContext = this;
            mmApp.SetThemeWindowOverride(this);

            Loaded += PasteCode_Loaded;
        }

        private void PasteCode_Loaded(object sender, RoutedEventArgs e)
        {
            this.TextCodeLanguage.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == ButtonCancel)
                DialogResult = false;
            else
            {
                DialogResult = true;                
            }

            Close();
        }
    }
}
