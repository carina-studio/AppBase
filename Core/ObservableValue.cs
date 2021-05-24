using System;
using System.Diagnostics.CodeAnalysis;

namespace CarinaStudio
{
	/// <summary>
	/// Value which is observable.
	/// </summary>
	/// <typeparam name="T">Type of value.</typeparam>
	public abstract class ObservableValue<T> : IEquatable<T>, IObservable<T>
	{
		// Holder of observer.
		class ObserverHolder : IDisposable
		{
			// Fields.
			public bool IsDisposed;
			public ObserverHolder? Next;
			public readonly IObserver<T> Observer;
			public readonly ObservableValue<T> Owner;

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
		/// Check whether given value should be treated as same as value of the instance or not.
		/// </summary>
		/// <param name="value">Value to check.</param>
		/// <returns>True if two values are treated as same value.</returns>
		public virtual bool Equals([AllowNull] T value)
		{
			if (value != null)
				return value.Equals(this.value);
			return this.value == null;
		}


		/// <summary>
		/// Check whether given object should be treated as same as the instance or not.
		/// </summary>
		/// <param name="obj">Object to check.</param>
		/// <returns>True if given object should be treated as same as the instance.</returns>
		public override bool Equals(object? obj)
		{
			if (obj is T value)
				return this.Equals(value);
			if (obj is ObservableValue<T> observableValue)
				return this.Equals(observableValue.value);
			return false;
		}


		/// <summary>
		/// Calculate hash-code based-on its value.
		/// </summary>
		/// <returns>Hash-code.</returns>
		public override int GetHashCode() => this.value?.GetHashCode() ?? 0;


		/// <summary>
		/// Check whether value is not null or not.
		/// </summary>
		public bool IsNotNull { get => this.value != null; }


		/// <summary>
		/// Check whether value is null or not.
		/// </summary>
		public bool IsNull { get => this.value == null; }


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
		/// Equality operator.
		/// </summary>
		/// <param name="x"><see cref="ObservableValue{T}"/>.</param>
		/// <param name="y">Raw value.</param>
		/// <returns>True if operands are equavalent.</returns>
		public static bool operator ==(ObservableValue<T> x, T y) => x.Equals(y);


		/// <summary>
		/// Equality operator.
		/// </summary>
		/// <param name="x"><see cref="ObservableValue{T}"/>.</param>
		/// <param name="y"><see cref="ObservableValue{T}"/>.</param>
		/// <returns>True if operands are equavalent.</returns>
		public static bool operator ==(ObservableValue<T> x, ObservableValue<T> y) => x.Equals(y.value);


		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="x"><see cref="ObservableValue{T}"/>.</param>
		/// <param name="y">Raw value.</param>
		/// <returns>True if operands are not equavalent.</returns>
		public static bool operator !=(ObservableValue<T> x, T y) => !x.Equals(y);


		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="x"><see cref="ObservableValue{T}"/>.</param>
		/// <param name="y"><see cref="ObservableValue{T}"/>.</param>
		/// <returns>True if operands are not equavalent.</returns>
		public static bool operator !=(ObservableValue<T> x, ObservableValue<T> y) => !x.Equals(y.value);


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
			if (this.observerListHead == null)
				this.observerListHead = observerHolder;
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


		// Ubsubscribe observer.
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
				if (this.Equals(value))
					return;
				++this.valueVersion;
				this.value = value;
				this.NotifyObservers();
			}
		}
	}
}
