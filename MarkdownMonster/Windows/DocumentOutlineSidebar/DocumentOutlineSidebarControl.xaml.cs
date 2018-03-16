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
            var selected = ListOutline.SelectedItem as HeaderItem;
            if (selected == null)
                return;

            Model.AppModel.ActiveEditor.GotoLine(selected.Line);            
            //Model.AppModel.Window.PreviewMarkdownAsync();
        }
    }
}
