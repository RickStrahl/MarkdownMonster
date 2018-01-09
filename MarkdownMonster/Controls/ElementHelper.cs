using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MarkdownMonster.Controls
{
    /// <summary>
    /// This class provides utility features related to WPF elements
    /// </summary>
    public static class ElementHelper
    {
        /// <summary>
        /// Finds the parent by walking parentElement.Parent until a parent of a certain type is found, or the end of the chain is reached
        /// </summary>
        /// <typeparam name="TType">The type of the parent parentElement that is to be found.</typeparam>
        /// <param name="element">The parentElement.</param>
        /// <returns>FrameworkElement.</returns>
        public static TType FindParent<TType>(FrameworkElement element) where TType : UIElement
        {
            var currentElement = element;
            while (true)
            {
                if (currentElement == null) return null;
                if (currentElement is TType) return currentElement as TType;
                if (currentElement.Parent == null) return null;
                currentElement = currentElement.Parent as FrameworkElement;
            }
        }

        /// <summary>
        /// Finds the parent by walking the complete visual tree until a parent of a certain type is found, or the end of the chain is reached
        /// </summary>
        /// <typeparam name="TType">The type of the parent parentElement that is to be found.</typeparam>
        /// <param name="element">The parentElement.</param>
        /// <returns>FrameworkElement.</returns>
        public static TType FindVisualTreeParent<TType>(FrameworkElement element) where TType : UIElement
        {
            var currentElement = element;
            while (true)
            {
                if (currentElement == null) return null;
                if (currentElement is TType) return currentElement as TType;
                currentElement = VisualTreeHelper.GetParent(currentElement) as FrameworkElement;
                if (currentElement == null) return null;
            }
        }

        /// <summary>
        /// Detaches an element from its current parrent
        /// </summary>
        /// <param name="elementToDetach">The element to detach.</param>
        /// <param name="parentElement">The parent element.</param>
        /// <exception cref="System.NotSupportedException">Can't remove element from current parent control of type  + parentElement.GetType()</exception>
        /// <remarks>
        /// Supports various ways of removing an element from its parent, such as content controls, child elements, and more.
        /// </remarks>
        public static void DetachElementFromParent(UIElement elementToDetach, DependencyObject parentElement)
        {
            if (parentElement == null) return;
            var contentControl = parentElement as ContentControl;
            if (contentControl != null)
            {
                contentControl.Content = null;
                return;
            }
            var itemsControl = parentElement as ItemsControl;
            if (itemsControl != null)
            {
                itemsControl.Items.Remove(elementToDetach);
                return;
            }
            var childControl = parentElement as Panel;
            if (childControl != null)
            {
                childControl.Children.Remove(elementToDetach);
                return;
            }
            var contentPresenter = parentElement as ContentPresenter;
            if (contentPresenter != null)
            {
                contentPresenter.Content = null;
                return;
            }
            throw new NotSupportedException("Can't remove element from current parent control of type " + parentElement.GetType());
        }

        /// <summary>
        /// Finds the parentElement's parent and detaches it
        /// </summary>
        /// <param name="element">The parentElement.</param>
        /// <remarks>
        /// Looks for different types of parent objects in different types of containers
        /// </remarks>
        public static void DetachElementFromParent(FrameworkElement element)
        {
            var existingParent = element.Parent;
            if (existingParent != null)
            {
                DetachElementFromParent(element, existingParent);
                return;
            }
            existingParent = VisualTreeHelper.GetParent(element);
            if (existingParent != null) DetachElementFromParent(element, existingParent);
        }
    }
}
