using System;
using System.Reflection;
using System.Threading;

namespace CarinaStudio.Tests
{
	/// <summary>
	/// Class to monitor whether specific event has been raised or not.
	/// </summary>
	/// <typeparam name="THandler">Type of event handler.</typeparam>
	/// <typeparam name="TArgs">Type of event data.</typeparam>
	public class EventMonitor<THandler, TArgs> : BaseDisposable where THandler : Delegate where TArgs : EventArgs
	{
		// Fields.
		volatile int eventCount;
		readonly Delegate eventHandler;
		readonly EventInfo eventInfo;
		readonly object? target;


		/// <summary>
		/// Initialize new <see cref="EventMonitor{THandler, TArgs}"/> for instance event.
		/// </summary>
		/// <param name="target">Target object which raises event.</param>
		/// <param name="eventName">Name of event.</param>
		public EventMonitor(object target, string eventName)
		{
			this.eventInfo = target.GetType().GetEvent(eventName) ?? throw new ArgumentException($"Cannot find event '{eventName}' in {target.GetType().FullName}.");
			this.eventHandler = Delegate.CreateDelegate(typeof(THandler), this, nameof(OnEventReceived));
			this.eventInfo.AddEventHandler(target, this.eventHandler);
			this.target = target;
		}


		/// <summary>
		/// Initialize new <see cref="EventMonitor{THandler, TArgs}"/> for static event.
		/// </summary>
		/// <param name="type">Type which raises event.</param>
		/// <param name="eventName">Name of event.</param>
		public EventMonitor(Type type, string eventName)
		{
			this.eventInfo = type.GetEvent(eventName) ?? throw new ArgumentException($"Cannot find event '{eventName}' in {type.FullName}.");
			this.eventHandler = Delegate.CreateDelegate(typeof(THandler), this, nameof(OnEventReceived));
			this.eventInfo.AddEventHandler(null, this.eventHandler);
			this.target = null;
		}


#pragma warning disable CS1591
		// Dispose.
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				this.eventInfo.RemoveEventHandler(this.target, this.eventHandler);
		}
#pragma warning restore CS1591


		/// <summary>
		/// Get event data of latest received event.
		/// </summary>
		public TArgs? EventArgs { get; private set; }


		/// <summary>
		/// Get number of received events.
		/// </summary>
		public int EventCount { get => this.eventCount; }


		// Event handler.
		void OnEventReceived(object? sender, EventArgs e)
		{
			this.EventArgs = (TArgs)e;
			Interlocked.Increment(ref this.eventCount);
		}


		/// <summary>
		/// Reset all state of received event.
		/// </summary>
		public void Reset()
		{
			this.EventArgs = null;
			Interlocked.Exchange(ref this.eventCount, 0);
		}
	}


	/// <summary>
	/// Class to monitor whether specific event has been raised or not.
	/// </summary>
	/// <typeparam name="TArgs">Type of event data.</typeparam>
	public class EventMonitor<TArgs> : EventMonitor<EventHandler<TArgs>, TArgs> where TArgs : EventArgs
	{
		/// <summary>
		/// Initialize new <see cref="EventMonitor{TArgs}"/> for instance event.
		/// </summary>
		/// <param name="target">Target object which raises event.</param>
		/// <param name="eventName">Name of event.</param>
		public EventMonitor(object target, string eventName) : base(target, eventName)
		{ }


		/// <summary>
		/// Initialize new <see cref="EventMonitor{TArgs}"/> for static event.
		/// </summary>
		/// <param name="type">Type which raises event.</param>
		/// <param name="eventName">Name of event.</param>
		public EventMonitor(Type type, string eventName) : base(type, eventName)
		{ }
	}
}
