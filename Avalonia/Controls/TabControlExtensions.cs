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
		/// <param name="tabControl"><see cref="TabControl"/>.</param>
		/// <param name="item">Item.</param>
		/// <param name="tabItem">Found <see cref="TabItem"/>.</param>
		/// <returns>True if <see cref="TabItem"/> found.</returns>
		public static bool TryFindTabItem(this TabControl tabControl, object item, out TabItem? tabItem)
		{
			var presenter = tabControl.FindDescendantOfType<ItemsPresenter>();
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


		/// <summary>
		/// Try moving specific item shown in the <see cref="TabControl"/>, and keep the item selected if it was selected before moving.
		/// </summary>
		/// <param name="tabControl"><see cref="TabControl"/>.</param>
		/// <param name="index">Index of item to be moved.</param>
		/// <param name="newIndex">Index of new position of item.</param>
		/// <param name="moveItemFunc">Function to perform moving item, or Null to use default way to move item.</param>
		/// <param name="scrollIntoView">True to scroll header of the item into view after moving.</param>
		/// <typeparam name="T">Type of element in the collection shown by the <see cref="TabControl"/>.</typeparam>
		/// <returns>True if the item has been moved successfully.</returns>
		public static bool TryMoveItem<T>(this TabControl tabControl, int index, int newIndex, Func<IEnumerable, int, int, bool>? moveItemFunc = null, bool scrollIntoView = true)
		{
			// check parameters
			var itemCount = tabControl.ItemCount;
			if (index < 0 || index >= itemCount || newIndex < 0 || newIndex >= itemCount)
				return false;
			if (index == newIndex)
				return true;

			// check source of items
			if (tabControl.ItemsSource is not { } itemsSource)
				return false;
			if (itemsSource is IList list && list.IsReadOnly)
				return false;

			// select default way to move item
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
						if (item is null)
							return false;
						genericList.RemoveAt(index);
						genericList.Insert(newIndex, item);
						return true;
					};
				}
				else
					return false;
			}

			// move item
			var movingItem = tabControl.Items[index];
			var restoreSelection = tabControl.SelectedIndex == index;
			if (!moveItemFunc(itemsSource, index, newIndex))
				return false;

			// [Workaround] Moving item is treated as removing and inserting by selection of Avalonia, so selection will be moved to first item if the moved item was selected
			if (restoreSelection && !ReferenceEquals(tabControl.SelectedItem, movingItem))
				tabControl.SelectedItem = movingItem;

			// scroll header of item into view
			if (scrollIntoView)
				Dispatcher.UIThread.Post(() => tabControl.ContainerFromIndex(newIndex)?.BringIntoView());
			return true;
		}
	}
}
