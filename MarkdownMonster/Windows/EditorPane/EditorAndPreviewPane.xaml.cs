using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;



namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for FolderBrowerSidebar.xaml
    /// </summary>
    public partial class EditorAndPreviewPane : UserControl
    {  
        public EditorAndPreviewPane()
        {
            InitializeComponent();            
        }

        private void Separator_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            bool zoom =  EditorWebBrowserEditorColumn.Width == GridLengthHelper.Star;
            if (zoom)
            {
                EditorWebBrowserPreviewColumn.Width = GridLengthHelper.Star;
                EditorWebBrowserEditorColumn.Width = GridLengthHelper.Zero;
                
            }
            else
            {                
                EditorWebBrowserPreviewColumn.Width = new GridLength(mmApp.Configuration.WindowPosition.InternalPreviewWidth);
                EditorWebBrowserEditorColumn.Width = GridLengthHelper.Star;
            }
        }
    }

}
