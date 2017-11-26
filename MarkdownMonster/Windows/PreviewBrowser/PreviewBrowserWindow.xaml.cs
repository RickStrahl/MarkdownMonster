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
    /// Interaction logic for PreviewBrowserWindow.xaml
    /// </summary>
    public partial class PreviewBrowserWindow : MetroWindow, IPreviewBrowser
    {

        public PreviewWebBrowser PreviewBrowser;


        public PreviewBrowserWindow()
        {
            InitializeComponent();

            PreviewBrowser = new PreviewWebBrowser(Browser);
        }

        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor, bool keepScrollPosition)
        {
            PreviewBrowser.PreviewMarkdownAsync(editor, keepScrollPosition);
        }

        public void PreviewMarkdown(MarkdownDocumentEditor editor, bool keepScrollPosition, bool showInBrowser)
        {
            PreviewBrowser.PreviewMarkdown(editor, keepScrollPosition);
        }

    }
}
