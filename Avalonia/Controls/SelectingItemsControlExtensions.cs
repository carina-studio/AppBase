using Avalonia.Controls.Primitives;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extensions for <see cref="SelectingItemsControl"/>.
    /// </summary>
    public static class SelectingItemsControlExtensions
    {
		/// <summary>
		/// Select first item in <see cref="SelectingItemsControl"/>.
		/// </summary>
		/// <param name="control"><see cref="SelectingItemsControl"/>.</param>
		/// <param name="scrollIntoView">True to scroll new selected item into view.</param>
		public static void SelectFirstItem(this SelectingItemsControl control, bool scrollIntoView = true)
		{
			var itemCount = control.ItemCount;
			if (itemCount > 0)
			{
				control.SelectedIndex = 0;
				if (scrollIntoView)
					control.ScrollIntoView(0);
			}
		}


		/// <summary>
		/// Select last item in <see cref="SelectingItemsControl"/>.
		/// </summary>
		/// <param name="control"><see cref="SelectingItemsControl"/>.</param>
		/// <param name="scrollIntoView">True to scroll new selected item into view.</param>
		public static void SelectLastItem(this SelectingItemsControl control, bool scrollIntoView = true)
		{
			var itemCount = control.ItemCount;
			if (itemCount > 0)
			{
				control.SelectedIndex = itemCount - 1;
				if (scrollIntoView)
					control.ScrollIntoView(itemCount - 1);
			}
		}


		/// <summary>
		/// Select next item in <see cref="SelectingItemsControl"/>.
		/// </summary>
		/// <param name="control"><see cref="SelectingItemsControl"/>.</param>
		/// <param name="scrollIntoView">True to scroll new selected item into view.</param>
		/// <returns>New index of selected item.</returns>
		public static int SelectNextItem(this SelectingItemsControl control, bool scrollIntoView = true)
		{
			var itemCount = control.ItemCount;
			if (itemCount > 0)
			{
				var newIndex = control.SelectedIndex;
				if (newIndex < itemCount - 1)
				{
					++newIndex;
					control.SelectedIndex = newIndex;
					if (scrollIntoView)
						control.ScrollIntoView(newIndex);
				}
				return newIndex;
			}
			return control.SelectedIndex;
		}


		/// <summary>
		/// Select previous item in <see cref="SelectingItemsControl"/>.
		/// </summary>
		/// <param name="control"><see cref="SelectingItemsControl"/>.</param>
		/// <param name="scrollIntoView">True to scroll new selected item into view.</param>
		/// <returns>New index of selected item.</returns>
		public static int SelectPreviousItem(this SelectingItemsControl control, bool scrollIntoView = true)
		{
			var itemCount = control.ItemCount;
			if (itemCount > 0)
			{
				var newIndex = control.SelectedIndex;
				if (newIndex < 0)
					newIndex = itemCount - 1;
				else if (newIndex > 0)
					--newIndex;
				else
					return newIndex;
				control.SelectedIndex = newIndex;
				if (scrollIntoView)
					control.ScrollIntoView(newIndex);
				return newIndex;
			}
			return control.SelectedIndex;
		}
	}
}
