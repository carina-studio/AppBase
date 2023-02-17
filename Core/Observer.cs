using System;

namespace CarinaStudio
{
	/// <summary>
	/// Simple adapter of <see cref="IObserver{T}"/> and methods.
	/// </summary>
	/// <typeparam name="T">Type of value to observe.</typeparam>
	public class Observer<T> : IObserver<T>
	{
		// Fields.
		readonly Action? onCompletedAction;
		readonly Action<T> onNextAction;
		readonly Action<Exception>? onErrorAction;


		/// <summary>
		/// Initialize new <see cref="Observer{T}"/> instance.
		/// </summary>
		/// <param name="onNext">Action of perform when value changed.</param>
		/// <param name="onCompleted">Action of receiving <see cref="IObserver{T}.OnCompleted"/>.</param>
		/// <param name="onError">Action of receiving <see cref="IObserver{T}.OnError(Exception)"/>.</param>
		public Observer(Action onNext, Action? onCompleted = null, Action<Exception>? onError = null)
		{
			this.onCompletedAction = onCompleted;
			this.onErrorAction = onError;
			this.onNextAction = _ => onNext();
		}


		/// <summary>
		/// Initialize new <see cref="Observer{T}"/> instance.
		/// </summary>
		/// <param name="onNext">Action of receiving <see cref="IObserver{T}.OnNext(T)"/>.</param>
		/// <param name="onCompleted">Action of receiving <see cref="IObserver{T}.OnCompleted"/>.</param>
		/// <param name="onError">Action of receiving <see cref="IObserver{T}.OnError(Exception)"/>.</param>
		public Observer(Action<T> onNext, Action? onCompleted = null, Action<Exception>? onError = null)
		{
			this.onCompletedAction = onCompleted;
			this.onErrorAction = onError;
			this.onNextAction = onNext;
		}


		// Interface implementations.
		void IObserver<T>.OnCompleted() => this.onCompletedAction?.Invoke();
		void IObserver<T>.OnError(Exception error) => this.onErrorAction?.Invoke(error);
		void IObserver<T>.OnNext(T value) => this.onNextAction(value);
	}
}
