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

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Input;

namespace MarkdownMonster.Controls
{

    /// <summary>
    /// Behavior attaches File System Auto Completion to a Combobox.
    /// </summary>
    public class ComboBoxAutoCompleteBehavior : Behavior<ComboBox>
    {

        public static bool IsDropdownAlwaysVisible(ComboBox combo)
        {
            return (bool)combo.GetValue(IsDropdownAlwaysVisibleProperty);
        }

        // Using a DependencyProperty as the backing store for IsFolderDropdownAlwaysVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDropdownAlwaysVisibleProperty =
            DependencyProperty.Register("IsDropdownAlwaysVisible",
                typeof(bool),
                typeof(ComboBox),
                new PropertyMetadata(true));

        private ComboBox SourceComboBox;

        /// <summary>
        /// Called when the autocomplete items need to be changed either
        /// by updating the underlying ItemsSource or explicitly updating
        /// the ComboItems.
        /// </summary>
        public event Action<ComboBox> UpdateAutoCompleteItems;

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
        }


        /// <summary>
        /// Handle any keys and check against paths
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Escape || e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.LeftShift)
                return;            
            UpdateAutoCompleteItems(SourceComboBox);
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
            
            UpdateAutoCompleteItems?.Invoke(combo);
        }
    }
}
