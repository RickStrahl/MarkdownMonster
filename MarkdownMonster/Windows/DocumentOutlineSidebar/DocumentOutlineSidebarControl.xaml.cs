using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MarkdownMonster.Windows.DocumentOutlineSidebar;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for DocumentOutlineSidebarControl.xaml
    /// </summary>
    public partial class DocumentOutlineSidebarControl : UserControl
    {

        public DocumentOutlineModel Model { get; set; }


        /// <summary>
        ///  Set this value to UtcNow to avoid next navigation
        /// </summary>
        public DateTime IgnoreSelection { get; set; }


        public DocumentOutlineSidebarControl()
        {
            InitializeComponent();
            DataContext = null;
            Loaded += DocumentOutlineSidebarControl_Loaded;
        }

        private void DocumentOutlineSidebarControl_Loaded(object sender, RoutedEventArgs e)
        {
            Model = new DocumentOutlineModel();
            DataContext = Model;
        }

        /// <summary>
        /// Refreshes the document outline if if it is visible
        /// and 
        /// </summary>
        public void RefreshOutline(int editorLineNumber = -1)
        {
            if (IgnoreSelection > DateTime.UtcNow.AddMilliseconds(-850))
                return;
            else
                IgnoreSelection = DateTime.MinValue;

            if (Model == null || Model.AppModel == null || Model.Window == null ||
                !Model.AppModel.Configuration.IsDocumentOutlineVisible) return;

            var editor = Model.AppModel.ActiveEditor;
            if (editor == null || editor.MarkdownDocument.EditorSyntax != "markdown")
            {
                Model.Window.TabDocumentOutline.Visibility = Visibility.Collapsed;
                Model.DocumentOutline = null;

                if (Model.Window.SidebarContainer.SelectedItem == Model.Window.TabDocumentOutline)
                    Model.Window.SidebarContainer.SelectedItem = Model.Window.TabFolderBrowser;

                return;
            }

            // make the tab visible
            Model.Window.TabDocumentOutline.Visibility = Visibility.Visible;
            Visibility = Visibility.Visible;

            // if we're not selected - don't update the outline
            if (Model.Window.SidebarContainer.SelectedItem != Model.Window.TabDocumentOutline)
                return;

            int line = editorLineNumber;
            if (line < 0)
                line = editor.GetLineNumber();

            var outline = Model.CreateDocumentOutline(editor.MarkdownDocument.CurrentText);
            if (outline == null)
            {
                Model.DocumentOutline = new ObservableCollection<HeaderItem>();
                return;
            }

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
                IgnoreSelection = DateTime.UtcNow;
                if (selectedItem != ListOutline.SelectedItem)
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

            IgnoreSelection = DateTime.UtcNow;
            Model.DocumentOutline = Model.CreateDocumentOutline(md);
        }

        private const string STR_StartDocumentOutline = "<!-- Start Document Outline -->";
        private const string STR_EndDocumentOutline = "<!-- End Document Outline -->";

        private void ButtonEmbedOutline_Click(object sender, RoutedEventArgs e)
        {
            var doc = Model.AppModel.ActiveDocument;
            if (doc == null)
                return;

            var editor = Model.AppModel.ActiveEditor;

            // get latest
            string markdown = editor.GetMarkdown();

            if (doc.CurrentText.Contains(STR_StartDocumentOutline))
            {
                var oldToc = StringUtils.ExtractString(markdown, STR_StartDocumentOutline, STR_EndDocumentOutline,
                    returnDelimiters: true);

                //editor.ReplaceContent(markdown);
                editor.FindAndReplaceText(oldToc, "");
            }

            // render the outline with any content below the current one
            var lineNo = editor.GetLineNumber();
            var md = Model.CreateMarkdownOutline(doc, lineNo);

            if (md != null)
            {
                md = STR_StartDocumentOutline + "\r\n\r\n" + md.TrimEnd() + "\r\n\r\n" + STR_EndDocumentOutline;
                editor.SetSelectionAndFocus(md);
            }
            else
                Model.Window.ShowStatusError("Couldn't create Markdown Outline to embed. Not embedded.");
        }

        private void ListOutlineItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var selected = ListOutline.SelectedItem as HeaderItem;
            if (selected == null || Model.AppModel.ActiveEditor == null)
                return;

            IgnoreSelection = DateTime.UtcNow;  // prevent editor navigating outline again
            Model.AppModel.ActiveEditor.GotoLine(selected.Line -1, noRefresh: false);  // refresh the preview
            Model.AppModel.ActiveEditor.SetSelectionRange(selected.Line, 0, selected.Line, 0);
        }

        private void TextBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
                ListOutlineItem_MouseUp(sender, null);

            if (e.Key == Key.Tab)
            {
                Model.AppModel.ActiveEditor.SetEditorFocus();
            }
        }

        private void MenuItem_CopyHeaderId_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListOutline.SelectedItem as HeaderItem;
            if (selected == null)
                return;

            if (ClipboardHelper.SetText("#" + selected.LinkId))
                Model.Window.ShowStatusSuccess($"Pasted id to clipboard: #{selected.LinkId}");
        }


        private void TextMaxIndentLevel_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextMaxIndentLevel.Text))
                return;

            RefreshOutline();
        }
    }
}
