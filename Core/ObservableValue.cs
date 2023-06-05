using System;

namespace CarinaStudio
{
	/// <summary>
	/// Value which is observable.
	/// </summary>
	/// <typeparam name="T">Type of value.</typeparam>
	public abstract class ObservableValue<T> : IObservable<T>
	{
		// Holder of observer.
		class ObserverHolder : IDisposable
		{
			// Fields.
			public bool IsDisposed;
			public ObserverHolder? Next;
			public readonly IObserver<T> Observer;
			readonly ObservableValue<T> Owner;

			// Constructor.
			public ObserverHolder(ObservableValue<T>  owner, IObserver<T> observer)
			{
				this.Owner = owner;
				this.Observer = observer;
			}

			// Dispose.
			public void Dispose() => this.Owner.Unsubscribe(this);
		}


		// Fields.
		bool isNotifyingObservers;
		bool isObserverListChanged;
		ObserverHolder? observerListHead;
		ObserverHolder? observerListTail;
		T value;
		int valueVersion;


		/// <summary>
		/// Initialize new <see cref="ObservableValue{T}"/> instance.
		/// </summary>
		/// <param name="initialValue">Initial value.</param>
#pragma warning disable CS8601 // Nullable assignment
		protected ObservableValue(T initialValue = default)
#pragma warning restore CS8601
		{
			this.value = initialValue;
		}


		/// <summary>
		/// Check equality of values.
		/// </summary>
		/// <param name="x">First value.</param>
		/// <param name="y">Second value.</param>
		/// <returns>True if two values are equivalent.</returns>
		protected virtual bool CheckValuesEquality(T x, T y) =>
			x?.Equals(y) ?? y is null;


		/// <summary>
		/// Check whether at least one <see cref="IObserver{T}"/> has been subscribed to this instance or not.
		/// </summary>
		public bool HasObservers => this.observerListHead != null;


		/// <summary>
		/// Check whether value is not null or not.
		/// </summary>
		public bool IsNotNull => this.value != null;


		/// <summary>
		/// Check whether value is null or not.
		/// </summary>
		public bool IsNull => this.value == null;


		// Notify all observers.
		void NotifyObservers()
		{
			this.isNotifyingObservers = true;
			var value = this.value;
			var valueVersion = this.valueVersion;
			var isInterrupted = false;
			try
			{
				var observerHolder = this.observerListHead;
				while (observerHolder != null)
				{
					if (!observerHolder.IsDisposed)
					{
						observerHolder.Observer.OnNext(value);
						if (valueVersion != this.valueVersion)
						{
							isInterrupted = true;
							break;
						}
					}
					observerHolder = observerHolder.Next;
				}
			}
			finally
			{
				if (!isInterrupted)
				{
					this.isNotifyingObservers = false;
					if (this.isObserverListChanged)
					{
						this.isObserverListChanged = false;
						var prevObserverHolder = (ObserverHolder?)null;
						var observerHolder = this.observerListHead;
						while (observerHolder != null)
						{
							if (observerHolder.IsDisposed)
							{
								var nextObserverHolder = observerHolder.Next;
								if (prevObserverHolder != null)
									prevObserverHolder.Next = nextObserverHolder;
								else
									this.observerListHead = nextObserverHolder;
								if (this.observerListTail == observerHolder)
									this.observerListTail = prevObserverHolder;
								observerHolder.Next = null;
								observerHolder = nextObserverHolder;
							}
							else
							{
								prevObserverHolder = observerHolder;
								observerHolder = observerHolder.Next;
							}
						}
					}
				}
			}
		}


		/// <summary>
		/// Convert to value itself explicitly.
		/// </summary>
		/// <param name="value"><see cref="ObservableValue{T}"/>.</param>
		public static explicit operator T(ObservableValue<T> value) => value.value;


		/// <summary>
		/// Subscribe <see cref="IObserver{T}"/> to get notification of change of value.
		/// </summary>
		/// <param name="observer"><see cref="IObserver{T}"/>.</param>
		/// <returns><see cref="IDisposable"/> represents subscribed <see cref="IObserver{T}"/>.</returns>
		public IDisposable Subscribe(IObserver<T> observer)
		{
			var observerHolder = new ObserverHolder(this, observer);
			this.observerListHead ??= observerHolder;
			if (this.observerListTail != null)
				this.observerListTail.Next = observerHolder;
			this.observerListTail = observerHolder;
			if (!this.isNotifyingObservers)
				observer.OnNext(this.value);
			return observerHolder;
		}


		/// <summary>
		/// Convert to readable string.
		/// </summary>
		/// <returns>Readable string.</returns>
		public override string? ToString() => this.value?.ToString();


		// Unsubscribe observer.
		void Unsubscribe(ObserverHolder observerHolder)
		{
			if (observerHolder.IsDisposed)
				return;
			observerHolder.IsDisposed = true;
			if (!this.isNotifyingObservers)
			{
				var prevObserverHolder = (ObserverHolder?)null;
				var candidateHolder = this.observerListHead;
				while (candidateHolder != null)
				{
					if (candidateHolder == observerHolder)
					{
						if (prevObserverHolder != null)
							prevObserverHolder.Next = candidateHolder.Next;
						else
							this.observerListHead = candidateHolder.Next;
						if (this.observerListTail == candidateHolder)
							this.observerListTail = prevObserverHolder;
						candidateHolder.Next = null;
						return;
					}
					prevObserverHolder = candidateHolder;
					candidateHolder = candidateHolder.Next;
				}
			}
			else
				this.isObserverListChanged = true;
		}


		/// <summary>
		/// Get or set value.
		/// </summary>
		public T Value
		{
			get => this.value;
			protected set
			{
				if (this.CheckValuesEquality(this.value, value))
					return;
				++this.valueVersion;
				this.value = value;
				this.NotifyObservers();
			}
		}
	}
}
