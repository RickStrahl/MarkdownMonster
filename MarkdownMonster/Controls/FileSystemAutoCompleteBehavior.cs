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
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace MarkdownMonster.Controls
{

    /// <summary>
    /// Behavior attaches File System Auto Completion to a Combobox.
    /// </summary>
    public class FileSystemAutoCompleteBehavior : ComboBoxAutoCompleteBehavior        
    {

        protected override void OnAttached()
        {
            base.OnAttached();
            UpdateAutoCompleteItems += HandleFolderPathTextAutoComplete;
        }        

        /// <summary>
        /// Main 
        /// </summary>
        /// <param name="combo"></param>
        private static void HandleFolderPathTextAutoComplete(ComboBox combo)
        {
            var typed = combo?.Text;
            if (string.IsNullOrEmpty(typed))
                return;

            Debug.WriteLine($"Combo: {typed}");

            string path = null;
            try
            {
                path = Path.GetDirectoryName(typed);
            }
            catch
            { }

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
                    if (folderPart.ToLower().StartsWith(typedPart))
                        foldersList.Add(folder);
                }

                folders = foldersList.ToArray();
            }

            foreach (var folder in folders)
                combo.Items.Add(folder);

            if (ComboBoxAutoCompleteBehavior.IsDropdownAlwaysVisible(combo))
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
