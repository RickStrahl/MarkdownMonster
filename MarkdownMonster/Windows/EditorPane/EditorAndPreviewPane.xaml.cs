using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FontAwesome.WPF;
using MarkdownMonster.Annotations;
using MarkdownMonster.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Westwind.Utilities;


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
