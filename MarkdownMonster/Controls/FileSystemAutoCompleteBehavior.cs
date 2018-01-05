#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 
 *          http://www.west-wind.com/
 * 
 * Created: 01/05/2018
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace MarkdownMonster.Controls
{

    /// <summary>
    /// Behavior attaches File System Auto Completion to a Combobox.
    /// </summary>
    public class FileSystemAutoCompleteBehavior : Behavior<ComboBox>
    {

        public static bool GetIsFolderDropdownAlwaysVisible(ComboBox combo)
        {
            return (bool)combo.GetValue(IsFolderDropdownAlwaysVisibleProperty);
        }

        // Using a DependencyProperty as the backing store for IsFolderDropdownAlwaysVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFolderDropdownAlwaysVisibleProperty =
            DependencyProperty.Register("IsFolderDropdownAlwaysVisible",
                typeof(bool),
                typeof(ComboBox),
                new PropertyMetadata(true));

        private ComboBox SourceComboBox;

        protected override void OnAttached()
        {
            base.OnAttached();

            SourceComboBox = AssociatedObject;           
            
            AssociatedObject.IsEditable = true;
            AssociatedObject.IsTextSearchEnabled = true;

            AssociatedObject.DropDownOpened += AssociatedObject_DropDownOpened;

            // Need to delay attaching the text handler
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)(AssociatedObject.Template.FindName("PART_EditableTextBox", AssociatedObject));
            textBox.PreviewKeyUp += TextBox_PreviewKeyUp;
        }

        protected override void OnDetaching()
        {
            TextBox textBox = (TextBox)(AssociatedObject.Template.FindName("PART_EditableTextBox", AssociatedObject));
            textBox.PreviewKeyUp -= TextBox_PreviewKeyUp;

            base.OnDetaching();
            //AssociatedObject.RemoveHandler(ComboBox.PreviewKeyUpEvent, new System.Windows.Input.KeyEventHandler(FileSystemAutoCompleteComboBox_PreviewKeyUp));
        }


        /// <summary>
        /// Handle any keys and check against paths
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HandleFolderPathTextAutoComplete(SourceComboBox);
        }
        

        /// <summary>
        /// When the ComboBox opens initially make sure there are items to display
        /// for the current path. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssociatedObject_DropDownOpened(object sender, System.EventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo == null || combo.Items.Count > 0)
                return;

            HandleFolderPathTextAutoComplete(combo);
        }


        /// <summary>
        /// Main 
        /// </summary>
        /// <param name="combo"></param>
        public static void HandleFolderPathTextAutoComplete(ComboBox combo)
        {
            if (combo == null)
                return;

            var typed = combo.Text;
            if (string.IsNullOrEmpty(typed))
                return;

            string path = null;
            try
            {
                path = System.IO.Path.GetDirectoryName(typed);
            }
            catch { }

            if (string.IsNullOrEmpty(path))
                return;

            // Capture selection - we have to reset it after we've
            // updated the items
            TextBox textBox = (TextBox)(combo.Template.FindName("PART_EditableTextBox", combo));
            var selStart = textBox.SelectionStart;
            var selLength = textBox.SelectionLength;

            combo.Items.Clear();

            string[] folders = { };
            try
            {
                folders = Directory.GetDirectories(path);
            }
            catch
            {
                return;
            }

            // strip out partials that don't match last part typed
            var typedPart = Path.GetFileName(typed);

            if (!string.IsNullOrEmpty(typedPart))
            {
                typedPart = typedPart.ToLower();
                var foldersList = new List<string>();
                foreach (var folder in folders)
                {
                    var folderPart = Path.GetFileName(folder);
                    if (folderPart.ToLower().Contains(typedPart))
                        foldersList.Add(folder);
                }

                folders = foldersList.ToArray();
            }

            foreach (var folder in folders)
                combo.Items.Add(folder);

            if (GetIsFolderDropdownAlwaysVisible(combo))
            {
                if (combo.Items.Count > 0)
                    combo.IsDropDownOpen = true;
                else
                    combo.IsDropDownOpen = false;
            }

            combo.Text = typed;

            textBox.SelectionStart = selStart;
            textBox.SelectionLength = selLength;

        }        
    }
}
