using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// Extensions for <see cref="ComboBox"/>.
	/// </summary>
	public static class ComboBoxExtensions
	{
		/// <summary>
		/// Try finding <see cref="ComboBoxItem"/> of given item in <see cref="ComboBox"/>.
		/// </summary>
		/// <param name="comboBox"><see cref="ComboBox"/>.</param>
		/// <param name="item">Item.</param>
		/// <param name="comboBoxItem">Found <see cref="ComboBoxItem"/>.</param>
		/// <returns>True if <see cref="ComboBoxItem"/> found.</returns>
		public static bool TryFindComboBoxItem(this ComboBox comboBox, object item, out ComboBoxItem? comboBoxItem)
		{
			var presenter = comboBox.FindDescendantOfType<ItemsPresenter>();
			if (presenter == null)
			{
				comboBoxItem = null;
				return false;
			}
			foreach (var child in presenter.GetVisualChildren())
			{
				if (child is Panel panel)
				{
					foreach (var panelChild in panel.Children)
					{
						if (panelChild is ComboBoxItem candidate && candidate.DataContext?.Equals(item) == true)
						{
							comboBoxItem = candidate;
							return true;
						}
					}
				}
			}
			comboBoxItem = null;
			return false;
		}
	}
}
