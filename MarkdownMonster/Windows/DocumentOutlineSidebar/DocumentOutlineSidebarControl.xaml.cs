using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MarkdownMonster.Windows.DocumentOutlineSidebar;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for DocumentOutlineSidebarControl.xaml
    /// </summary>
    public partial class DocumentOutlineSidebarControl : UserControl
    {

        public DocumentOutlineModel Model { get; set; }
        public bool IgnoreSelection { get; private set; }

        public DocumentOutlineSidebarControl()
        {
            InitializeComponent();

            Loaded += DocumentOutlineSidebarControl_Loaded;

        }

        private void DocumentOutlineSidebarControl_Loaded(object sender, RoutedEventArgs e)
        {
            Model = new DocumentOutlineModel();
            DataContext = Model;            
        }

      

        public void RefreshOutline()
        {

            var editor = Model.AppModel.ActiveEditor;
            if (editor == null)
                return;

            if (editor.EditorSyntax != "markdown")
            {
                Visibility = Visibility.Collapsed;
                return;
            }

            int line = editor.GetLineNumber();
            var outline = Model.CreateDocumentOutline(editor.MarkdownDocument.CurrentText);
            HeaderItem selectedItem = null;
            for (var index = 0; index < outline.Count; index++)
            {
                var item = outline[index];
                if (item.Line == line)
                {
                    selectedItem = item;
                    break;
                }

                if (item.Line > line && line > 0)
                {
                    index = index - 1;
                    if (index < 0)
                        index = 0;
                    selectedItem = outline[index];
                    break;
                }
            }

            Model.DocumentOutline = outline;
            if (selectedItem != null)
            {
                IgnoreSelection = true;
                ListOutline.SelectedItem = selectedItem;
                ListOutline.ScrollIntoView(selectedItem);
            }
        }


        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            var md = Model.AppModel.ActiveDocument?.CurrentText;
            if (md == null)
            {
                Model.DocumentOutline = null;
                return;
            }

            Model.DocumentOutline = Model.CreateDocumentOutline(md);
        }
                
        private void ListOutline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreSelection)
            {
                IgnoreSelection = false;
                return;
            }

            var selected = ListOutline.SelectedItem as HeaderItem;
            if (selected == null || Model.AppModel.ActiveEditor == null)
                return;

            Model.AppModel.ActiveEditor.GotoLine(selected.Line -1);            
            //ListOutline.SelectedItem = null;
        }
    }
}
