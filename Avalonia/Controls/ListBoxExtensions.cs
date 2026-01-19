using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CarinaStudio.Collections;
using System;
using System.Collections;
using System.Collections.ObjectModel;

namespace CarinaStudio.Controls;

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
	
	
	/// <summary>
	/// Try moving specific item shown in the <see cref="ListBox"/>.
	/// </summary>
	/// <param name="listBox"><see cref="ListBox"/>.</param>
	/// <param name="index">Index of item to be moved.</param>
	/// <param name="newIndex">Index of new position of item.</param>
	/// <param name="moveItemFunc">Function to perform moving item, or Null to use default way to move item.</param>
	/// <param name="scrollIntoView">True to scroll the item into view after moving.</param>
	/// <typeparam name="T">Type of element in the collection shown by the <see cref="ListBox"/>.</typeparam>
	/// <returns>True if the item has been moved successfully.</returns>
	public static bool TryMoveItem<T>(this ListBox listBox, int index, int newIndex, Func<IEnumerable, int, int, bool>? moveItemFunc = null, bool scrollIntoView = true)
	{
		var itemCount = listBox.ItemCount;
		if (index < 0 || index >= itemCount || newIndex < 0 || newIndex >= itemCount)
			return false;
		if (index != newIndex)
		{
			if (listBox.ItemsSource is not { } itemsSource)
				return false;
			if (moveItemFunc is null)
			{
				if (itemsSource is AvaloniaList<T> avaloniaList)
				{
					moveItemFunc = (_, _, _) =>
					{
						avaloniaList.Move(index, newIndex);
						return true;
					};
				}
				else if (itemsSource is ObservableList<T> observableList)
				{
					moveItemFunc = (_, _, _) =>
					{
						observableList.Move(index, newIndex);
						return true;
					};
				}
				else if (itemsSource is ObservableCollection<T> observableCollection)
				{
					moveItemFunc = (_, _, _) =>
					{
						observableCollection.Move(index, newIndex);
						return true;
					};
				}
				else
					return false;
			}
			var item = listBox.Items[index];
			var restoreFocus = listBox.SelectedIndex == index && listBox.ContainerFromIndex(index)?.IsFocused == true;
			if (!moveItemFunc(itemsSource, index, newIndex))
				return false;
			listBox.SelectedIndex = newIndex;
			Dispatcher.UIThread.Post(() =>
			{
				if (restoreFocus && listBox.SelectedItem?.Equals(item) == true && listBox.ContainerFromIndex(newIndex) is { } container)
					container.Focus();
				if (scrollIntoView)
					listBox.ScrollIntoView(newIndex);
			});
		}
		return true;
	}
}