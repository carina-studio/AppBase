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
		/// <param name="cancellationToken">Cancellation token to cancel waiting.</param>
		/// <returns>True if value of property has been changed to target value before cancellation or timeout.</returns>
		[Obsolete("Use CarinaStudio.ComponentModel.NotifyPropertyChangedExtensions.WaitForPropertyChangeAsync().")]
		public static async Task<bool> WaitForPropertyAsync(this INotifyPropertyChanged obj, string propertyName, object? targetValue, int timeout = Timeout.Infinite, CancellationToken? cancellationToken = null)
		{
			// check property value
			var objType = obj.GetType();
			var propertyInfo = objType.GetProperty(propertyName) ?? throw new ArgumentException($"Cannot find property '{propertyName}' in {objType.FullName}.");
			var value = propertyInfo.GetValue(obj);
			if (targetValue?.Equals(value) ?? value == null)
				return true;

			// add event handler
			using var timeoutTokenSource = new CancellationTokenSource();
			var eventHandler = (PropertyChangedEventHandler?)null;
			eventHandler = new PropertyChangedEventHandler((_, e) =>
			{
				if (e.PropertyName != propertyName)
					return;
				value = propertyInfo.GetValue(obj);
				if (targetValue?.Equals(value) ?? value == null)
				{
					obj.PropertyChanged -= eventHandler;
					timeoutTokenSource.Cancel();
				}
			});
			obj.PropertyChanged += eventHandler;

			// combine cancellation token if needed
			using var cancellationTokenSource = cancellationToken?.Let(it => CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Value, timeoutTokenSource.Token)) ?? timeoutTokenSource;

			// wait for property change
			try
			{
				await Task.Delay(timeout, cancellationTokenSource.Token);
			}
			catch (TaskCanceledException)
			{ }
			obj.PropertyChanged -= eventHandler;

			// check property value
			value = propertyInfo.GetValue(obj);
			return (targetValue?.Equals(value) ?? value == null);
		}
	}
}
