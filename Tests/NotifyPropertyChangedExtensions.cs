using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Tests
{
	/// <summary>
	/// Extensions for <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	public static class NotifyPropertyChangedExtensions
	{
		/// <summary>
		/// Wait for value of specific property to be target value asynchronously.
		/// </summary>
		/// <param name="obj"><see cref="INotifyPropertyChanged"/>.</param>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="targetValue">Target value.</param>
		/// <param name="timeout">Timeout in milliseconds.</param>
		/// <returns>True if value of property has been changed to target value in given timeout.</returns>
		public static async Task<bool> WaitForPropertyAsync(this INotifyPropertyChanged obj, string propertyName, object? targetValue, int timeout = Timeout.Infinite)
		{
			// check property value
			var objType = obj.GetType();
			var propertyInfo = objType.GetProperty(propertyName) ?? throw new ArgumentException($"Cannot find property '{propertyName}' in {objType.FullName}.");
			var value = propertyInfo.GetValue(obj);
			if (targetValue?.Equals(value) ?? value == null)
				return true;

			// add event handler
			var cancellationTokenSource = new CancellationTokenSource();
			var eventHandler = (PropertyChangedEventHandler?)null;
			eventHandler = new PropertyChangedEventHandler((_, e) =>
			{
				if (e.PropertyName != propertyName)
					return;
				value = propertyInfo.GetValue(obj);
				if (targetValue?.Equals(value) ?? value == null)
				{
					obj.PropertyChanged -= eventHandler;
					cancellationTokenSource.Cancel();
				}
			});
			obj.PropertyChanged += eventHandler;

			// wait for property change
			await Task.Delay(timeout, cancellationTokenSource.Token);
			obj.PropertyChanged -= eventHandler;

			// check property value
			value = propertyInfo.GetValue(obj);
			return (targetValue?.Equals(value) ?? value == null);
		}
	}
}
