using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace MarkdownMonster.Controls
{
    public class Ex : DependencyObject
    {
        /// <summary>Defines whether an object (such as a textbox) automatically is selected when focus moves into it</summary>
        public static readonly DependencyProperty SelectOnEntryProperty = DependencyProperty.RegisterAttached("SelectOnEntry", typeof(bool), typeof(Ex), new UIPropertyMetadata(false, SelectOnEntryChanged));

        private static void SelectOnEntryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                var text = d as TextBoxBase;
                if (text != null)
                    text.GotFocus += Text_GotFocus;
            }
        }

        private static void Text_GotFocus(object sender, RoutedEventArgs e)
        {
            var text = sender as TextBoxBase;
            text?.SelectAll();            
        }

        /// <summary>Defines whether an object (such as a textbox) automatically is selected when focus moves into it</summary>
        /// <param name="obj">The object to set the value on</param>
        /// <param name="value">True for auto-select</param>
        public static void SetSelectOnEntry(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectOnEntryProperty, value);
        }

        /// <summary>Defines whether an object (such as a textbox) automatically is selected when focus moves into it</summary>
        /// <param name="obj">The object to retrieve the value for</param>
        /// <returns>True if auto-select</returns>
        public static bool GetSelectOnEntry(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectOnEntryProperty);
        }
    }
}
