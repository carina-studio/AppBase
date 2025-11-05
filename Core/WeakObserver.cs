using System;

namespace CarinaStudio;

class WeakObserver<T>(IObserver<T> target) : IObserver<T>
{
	// Fields.
	public IDisposable? SubscriptionToken;
	readonly WeakReference<IObserver<T>> targetRef = new(target);
	

	/// <inheritdoc/>
	void IObserver<T>.OnCompleted()
	{
		if (this.targetRef.TryGetTarget(out var target))
			target.OnCompleted();
		else
			this.SubscriptionToken = this.SubscriptionToken.DisposeAndReturnNull();
	}


	/// <inheritdoc/>
	void IObserver<T>.OnError(Exception error)
	{
		if (this.targetRef.TryGetTarget(out var target))
			target.OnError(error);
		else
			this.SubscriptionToken = this.SubscriptionToken.DisposeAndReturnNull();
	}


	/// <inheritdoc/>
	void IObserver<T>.OnNext(T value)
	{
		if (this.targetRef.TryGetTarget(out var target))
			target.OnNext(value);
		else
			this.SubscriptionToken = this.SubscriptionToken.DisposeAndReturnNull();
	}
}