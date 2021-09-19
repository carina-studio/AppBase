using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;
using System;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// Extensions for <see cref="ListBox"/>.
	/// </summary>
	public static class ListBoxExtensions
	{
		/// <summary>
		/// Try finding <see cref="ListBoxItem"/> of given item in <see cref="ListBox"/>.
		/// </summary>
		/// <param name="listBox"><see cref="ListBox"/>.</param>
		/// <param name="item">Item.</param>
		/// <param name="listBoxItem">Found <see cref="ListBoxItem"/>.</param>
		/// <returns>True if <see cref="ListBoxItem"/> found.</returns>
		public static bool TryFindListBoxItem(this ListBox listBox, object item, out ListBoxItem? listBoxItem)
		{
			var presenter = listBox.FindDescendantOfType<ItemsPresenter>();
			if (presenter == null)
			{
				listBoxItem = null;
				return false;
			}
			foreach (var child in presenter.GetVisualChildren())
			{
				if (child is Panel panel)
				{
					foreach (var panelChild in panel.Children)
					{
						if (panelChild is ListBoxItem candidate && candidate.DataContext?.Equals(item) == true)
						{
							listBoxItem = candidate;
							return true;
						}
					}
				}
			}
			listBoxItem = null;
			return false;
		}
	}
}
