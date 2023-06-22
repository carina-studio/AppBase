using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// Extensions for <see cref="TabControl"/>.
	/// </summary>
	public static class TabControlExtensions
	{
		/// <summary>
		/// Try finding <see cref="TabItem"/> of given item in <see cref="TabControl"/>.
		/// </summary>
		/// <param name="listBox"><see cref="TabControl"/>.</param>
		/// <param name="item">Item.</param>
		/// <param name="tabItem">Found <see cref="TabItem"/>.</param>
		/// <returns>True if <see cref="TabItem"/> found.</returns>
		public static bool TryFindTabItem(this TabControl listBox, object item, out TabItem? tabItem)
		{
			var presenter = listBox.FindDescendantOfType<ItemsPresenter>();
			if (presenter == null)
			{
				tabItem = null;
				return false;
			}
			foreach (var child in presenter.GetVisualChildren())
			{
				if (child is Panel panel)
				{
					foreach (var panelChild in panel.Children)
					{
						if (panelChild is TabItem candidate && candidate.DataContext?.Equals(item) == true)
						{
							tabItem = candidate;
							return true;
						}
					}
				}
			}
			tabItem = null;
			return false;
		}
	}
}
