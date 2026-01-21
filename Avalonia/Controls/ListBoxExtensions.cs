using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CarinaStudio.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CarinaStudio.Controls;

/// <summary>
/// Extensions for <see cref="ListBox"/>.
/// </summary>
public static class ListBoxExtensions
{
#if NET10_0_OR_GREATER
	extension(ListBox listBox)
	{
		/// <summary>
		/// Check whether the selected item in the <see cref="ListBox"/> is focused or not.
		/// </summary>
		public bool IsSelectedItemFocused
		{
			get
			{
				var selectedIndex = listBox.SelectedIndex;
				return selectedIndex >= 0 && listBox.ContainerFromIndex(selectedIndex)?.IsFocused == true;
			}
		}
	}
#endif
	
	
	/// <summary>
	/// Set focus on selected item of <see cref="ListBox"/>.
	/// </summary>
	/// <param name="listBox"><see cref="ListBox"/>.</param>
	public static void FocusSelectedItem(this ListBox listBox)
	{
		if (listBox.SelectedItem is not { } selectedItem)
			return;
		if (listBox.ContainerFromIndex(listBox.SelectedIndex) is { } container)
		{
			container.Focus();
			return;
		}
		Dispatcher.UIThread.Post(() =>
		{
			if (selectedItem.Equals(listBox.SelectedItem) && listBox.ContainerFromIndex(listBox.SelectedIndex) is { } container)
				container.Focus();
		});
	}
	
	
#if !NET10_0_OR_GREATER
	/// <summary>
	/// Check whether the selected item in the <see cref="ListBox"/> is focused or not.
	/// </summary>
	/// <param name="listBox"><see cref="ListBox"/>.</param>
	/// <returns>True if the selected item is focused.</returns>
	/// <remarks>The method is available only for .NET 9 and previous versions. For .NET 10 and newer versions please use extension property.</remarks>
	public static bool IsSelectedItemFocused(this ListBox listBox)
	{
		var selectedIndex = listBox.SelectedIndex;
		return selectedIndex >= 0 && listBox.ContainerFromIndex(selectedIndex)?.IsFocused == true;
	}
#endif
	
	
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
			if (itemsSource is IList list && list.IsReadOnly)
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
				else if (itemsSource is not INotifyCollectionChanged)
					return false;
				else if (itemsSource is IList<T> genericList)
				{
					moveItemFunc = (_, _, _) =>
					{
						var item = genericList[index];
						if (item is not null)
						{
							genericList.RemoveAt(index);
							genericList.Insert(newIndex, item);
							return true;
						}
						return false;
					};
				}
				else
					return false;
			}
			var item = listBox.Items[index];
			var selectionMode = listBox.SelectionMode;
			var restoreSelection = selectionMode switch
			{
				SelectionMode.AlwaysSelected or SelectionMode.Single => listBox.SelectedIndex == index,
				_ => listBox.SelectedItems?.Let(selectedItems =>
				{
					foreach (var selectedItem in selectedItems)
					{
						if (selectedItem?.Equals(item) == true)
							return true;
					}
					return false;
				}) == true,
			};
			var restoreFocus = restoreSelection && listBox.ContainerFromIndex(index)?.IsFocused == true;
			if (!moveItemFunc(itemsSource, index, newIndex))
				return false;
			if (restoreSelection)
				listBox.SelectedItems?.Add(item);
			else
				listBox.SelectedItems?.Remove(item);
			Dispatcher.UIThread.Post(() =>
			{
				if (restoreFocus && listBox.SelectedItems?.Contains(item) == true && listBox.ContainerFromIndex(newIndex) is { } container)
					container.Focus();
				if (scrollIntoView)
					listBox.ScrollIntoView(newIndex);
			});
		}
		return true;
	}
}