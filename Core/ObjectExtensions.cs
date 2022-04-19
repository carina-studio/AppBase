using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CarinaStudio
{
	/// <summary>
	/// Extensions for all types.
	/// </summary>
	public static class ObjectExtensions
	{
		// Adapter of weak event handler.
		class WeakEventHandlerAdapter : IDisposable
		{
			// Fields.
			readonly EventInfo eventInfo;
			readonly EventHandler handlerEntry;
			readonly WeakReference<EventHandler> handlerRef;
			volatile bool isDisposed;
			readonly object target;

			// Constructor.
			public WeakEventHandlerAdapter(object target, string eventName, EventHandler handler)
			{
				this.eventInfo = target.GetType().GetEvent(eventName) ?? throw new ArgumentException($"Cannot find event '{eventName}' in {target.GetType().Name}.");
				this.handlerEntry = this.OnEventReceived;
				this.handlerRef = new WeakReference<EventHandler>(handler);
				this.target = target;
				this.eventInfo.AddEventHandler(target, this.handlerEntry);
			}

			// Dispose.
			public void Dispose()
			{
				lock (this)
				{
					if (this.isDisposed)
						return;
					this.isDisposed = true;
				}
				this.eventInfo.RemoveEventHandler(target, this.handlerEntry);
			}

			// Entry of event handler.
			void OnEventReceived(object? sender, EventArgs e)
			{
				if (this.handlerRef.TryGetTarget(out var handler) && handler != null)
					handler(sender, e);
				else
				{
					var syncContext = SynchronizationContext.Current;
					if (syncContext != null)
						syncContext.Post(_ => this.Dispose(), null);
					else
						this.Dispose();
				}
			}
		}


		// Adapter of weak event handler.
		class WeakEventHandlerAdapter<TArgs> : IDisposable where TArgs : EventArgs
		{
			// Fields.
			readonly EventInfo eventInfo;
			readonly EventHandler<TArgs> handlerEntry;
			readonly WeakReference<EventHandler<TArgs>> handlerRef;
			volatile bool isDisposed;
			readonly object target;

			// Constructor.
			public WeakEventHandlerAdapter(object target, string eventName, EventHandler<TArgs> handler)
			{
				this.eventInfo = target.GetType().GetEvent(eventName) ?? throw new ArgumentException($"Cannot find event '{eventName}' in {target.GetType().Name}.");
				this.handlerEntry = this.OnEventReceived;
				this.handlerRef = new WeakReference<EventHandler<TArgs>>(handler);
				this.target = target;
				this.eventInfo.AddEventHandler(target, this.handlerEntry);
			}

			// Dispose.
			public void Dispose()
			{
				lock (this)
				{
					if (this.isDisposed)
						return;
					this.isDisposed = true;
				}
				this.eventInfo.RemoveEventHandler(target, this.handlerEntry);
			}

			// Entry of event handler.
			void OnEventReceived(object? sender, TArgs e)
			{
				if (this.handlerRef.TryGetTarget(out var handler) && handler != null)
					handler(sender, e);
				else
				{
					var syncContext = SynchronizationContext.Current;
					if (syncContext != null)
						syncContext.Post(_ => this.Dispose(), null);
					else
						this.Dispose();
				}
			}
		}


		/// <summary>
		/// Add weak event handler.
		/// </summary>
		/// <param name="target"><see cref="object"/>.</param>
		/// <param name="eventName">Name of event.</param>
		/// <param name="handler">Event handler.</param>
		/// <returns><see cref="IDisposable"/> which represents added weak event handler. You can call <see cref="IDisposable.Dispose"/> to remove weak event handler explicitly.</returns>
		public static IDisposable AddWeakEventHandler(this object target, string eventName, EventHandler handler) =>
			new WeakEventHandlerAdapter(target, eventName, handler);


		/// <summary>
		/// Add weak event handler.
		/// </summary>
		/// <param name="target"><see cref="object"/>.</param>
		/// <param name="eventName">Name of event.</param>
		/// <param name="handler">Event handler.</param>
		/// <returns><see cref="IDisposable"/> which represents added weak event handler. You can call <see cref="IDisposable.Dispose"/> to remove weak event handler explicitly.</returns>
		public static IDisposable AddWeakEventHandler<TArgs>(this object target, string eventName, EventHandler<TArgs> handler) where TArgs : EventArgs =>
			new WeakEventHandlerAdapter<TArgs>(target, eventName, handler);


		/// <summary>
		/// Perform action on the given value, and return it.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="value">Given value.</param>
		/// <param name="action">Action to perform on <paramref name="value"/>.</param>
		/// <returns>Value which is same as <paramref name="value"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Also<T>(this T value, RefAction<T> action) where T : struct
		{
			action(ref value);
			return value;
		}


		/// <summary>
		/// Perform action on the given value, and return it.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="value">Given value.</param>
		/// <param name="action">Action to perform on <paramref name="value"/>.</param>
		/// <returns>Value which is same as <paramref name="value"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T? Also<T>(this T? value, RefAction<T?> action) where T : struct
		{
			action(ref value);
			return value;
		}


		/// <summary>
		/// Perform action on the given value, and return it.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="value">Given value.</param>
		/// <param name="action">Action to perform on <paramref name="value"/>.</param>
		/// <returns>Value which is same as <paramref name="value"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Also<T>(this T value, Action<T> action) where T : class?
		{
			action(value);
			return value;
		}


		/// <summary>
		/// Treat given nullable value as non-null value, or throw <see cref="NullReferenceException"/>.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="obj">Given nullable value.</param>
		/// <returns>Non-null value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T AsNonNull<T>(this T? obj) where T : class => obj ?? throw new NullReferenceException();


		/// <summary>
		/// Perform action on the given value.
		/// </summary>
		/// <typeparam name="T">Type of given value.</typeparam>
		/// <param name="value">Given value.</param>
		/// <param name="action">Action to perform on <paramref name="value"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Let<T>(this T value, Action<T> action) => action(value);


		/// <summary>
		/// Perform action on the given value, and return a custom value.
		/// </summary>
		/// <typeparam name="T">Type of given value.</typeparam>
		/// <typeparam name="R">Type of return value.</typeparam>
		/// <param name="value">Given value.</param>
		/// <param name="action">Action to perform on <paramref name="value"/>.</param>
		/// <returns>Custom return value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Let<T, R>(this T value, Func<T, R> action) => action(value);


		/// <summary>
		/// Perform action on the given value, and return a reference to custom variable.
		/// </summary>
		/// <typeparam name="T">Type of given value.</typeparam>
		/// <typeparam name="R">Type of return value.</typeparam>
		/// <param name="value">Given value.</param>
		/// <param name="action">Action to perform on <paramref name="value"/>.</param>
		/// <returns>Reference to custom variable.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref R Let<T, R>(this T value, RefFunc<T, R> action) => ref action(value);


		/// <summary>
		/// Acquire lock on given object and generate value after releasing lock.
		/// </summary>
		/// <typeparam name="T">Type of given object.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="obj">Object to acquire lock on.</param>
		/// <param name="func">Function to generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Lock<T, R>(this T obj, Func<R> func) where T : class
		{
			Monitor.Enter(obj);
			try
			{
				return func();
			}
			finally
			{
				Monitor.Exit(obj);
			}
		}


		/// <summary>
		/// Acquire lock on given object and generate reference to value after releasing lock.
		/// </summary>
		/// <typeparam name="T">Type of given object.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="obj">Object to acquire lock on.</param>
		/// <param name="func">Function to generate reference to value.</param>
		/// <returns>Generated reference to value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref R Lock<T, R>(this T obj, RefFunc<R> func) where T : class
		{
			Monitor.Enter(obj);
			try
			{
				return ref func();
			}
			finally
			{
				Monitor.Exit(obj);
			}
		}


		/// <summary>
		/// Acquire lock on given object and generate value after releasing lock.
		/// </summary>
		/// <typeparam name="T">Type of given object.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="obj">Object to acquire lock on.</param>
		/// <param name="func">Function to generate value.</param>
		/// <returns>Generated value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static R Lock<T, R>(this T obj, Func<T, R> func) where T : class
		{
			Monitor.Enter(obj);
			try
			{
				return func(obj);
			}
			finally
			{
				Monitor.Exit(obj);
			}
		}


		/// <summary>
		/// Acquire lock on given object and generate reference to value after releasing lock.
		/// </summary>
		/// <typeparam name="T">Type of given object.</typeparam>
		/// <typeparam name="R">Type of generated value.</typeparam>
		/// <param name="obj">Object to acquire lock on.</param>
		/// <param name="func">Function to generate reference to value.</param>
		/// <returns>Generated reference to value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref R Lock<T, R>(this T obj, RefFunc<T, R> func) where T : class
		{
			Monitor.Enter(obj);
			try
			{
				return ref func(obj);
			}
			finally
			{
				Monitor.Exit(obj);
			}
		}
	}
}
