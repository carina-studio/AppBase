using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extensions for <see cref="Control"/>.
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// Remove control from its logical parent.
        /// </summary>
        /// <param name="control">Control to remove.</param>
        /// <returns>True if control has been removed successfully.</returns>
        public static bool RemoveFromParent(this Control control)
        {
            var parent = control.Parent;
            if (parent == null)
                return false;
            if (parent is ContentControl contentControl)
            {
                if (contentControl.Content != control)
                    return false;
                contentControl.Content = null;
            }
            else if (parent is ContentPresenter contentPresenter)
            {
                if (contentPresenter.Content != control)
                    return false;
                contentPresenter.Content = null;
            }
            else if (parent is Decorator decorator)
            {
                if (decorator.Child != control)
                    return false;
                decorator.Child = null;
            }
            else if (parent is HeaderedContentControl headeredContentControl)
            {
                if (headeredContentControl.Header == control)
                    headeredContentControl.Header = null;
                else if (headeredContentControl.Content == control)
                    headeredContentControl.Content = null;
                else
                    return false;
            }
            else if (parent is HeaderedItemsControl headeredItemsControl)
            {
                if (headeredItemsControl.Header != control)
                    return false;
                headeredItemsControl.Header = null;
            }
            else if (parent is HeaderedSelectingItemsControl headeredSelectingItemsControl)
            {
                if (headeredSelectingItemsControl.Header != control)
                    return false;
                headeredSelectingItemsControl.Header = null;
            }
            else if (parent is Panel panel)
                return panel.Children.Remove(control);
            else
                return false;
            return true;
        }
    }
}