using System;

namespace CarinaStudio
{
	/// <summary>
	/// Base implementation of <see cref="IShareableDisposable{TSelf}"/>.
	/// </summary>
	/// <typeparam name="TSelf">Self type.</typeparam>
	public abstract class BaseShareableDisposable<TSelf> : IShareableDisposable<TSelf> where TSelf : IShareableDisposable<TSelf>
	{
		/// <summary>
		/// Base implementation of internal resource holder.
		/// </summary>
		public abstract class BaseResourceHolder
		{
			// Call Release().
			internal void CallRelease() => this.Release();

			/// <summary>
			/// Release internal resources.
			/// </summary>
			protected abstract void Release();

			// Reference counter.
			internal volatile int ReferenceCount = 1;
		}


		// Fields.
		volatile bool isDisposed;
		volatile bool isDisposing;
		readonly BaseResourceHolder resourceHolder;


		/// <summary>
		/// Initialize new <see cref="BaseShareableDisposable{TSelf}"/> instance.
		/// </summary>
		/// <param name="resourceHolder">Resource holder.</param>
		protected BaseShareableDisposable(BaseResourceHolder resourceHolder)
		{
			this.resourceHolder = resourceHolder;
		}


		/// <summary>
		/// Finalize <see cref="BaseShareableDisposable{TSelf}"/> instance.
		/// </summary>
		~BaseShareableDisposable() => this.Dispose(false);


		/// <summary>
		/// Dispose the instance.
		/// </summary>
		public void Dispose()
		{
			lock (this)
			{
				if (this.isDisposing || this.isDisposed)
					return;
				this.isDisposing = true;
			}
			GC.SuppressFinalize(this);
			try
			{
				this.Dispose(true);
			}
			finally
			{
				this.isDisposing = false;
				this.isDisposed = true;
			}
		}


		/// <summary>
		/// Dispose the instance.
		/// </summary>
		/// <param name="disposing">True to dispose managed resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			lock (this.resourceHolder)
			{
				--this.resourceHolder.ReferenceCount;
				if (this.resourceHolder.ReferenceCount == 0)
					this.resourceHolder.CallRelease();
			}
		}


		/// <summary>
		/// Get resource holder as specific type.
		/// </summary>
		/// <typeparam name="T">Specific type of <see cref="BaseResourceHolder"/>.</typeparam>
		/// <returns>Resource holder.</returns>
		protected T GetResourceHolder<T>() where T : BaseResourceHolder
		{
			this.ThrowIfDisposed();
			return (T)this.resourceHolder;
		}


		/// <summary>
		/// Check whether instance has been disposed or not.
		/// </summary>
		protected bool IsDisposed { get => this.isDisposed; }


		/// <summary>
		/// Create a new instance which shares internal resources.
		/// </summary>
		/// <returns>New instance which shares internal resources.</returns>
		public TSelf Share()
		{
			this.ThrowIfDisposed();
			lock (this.resourceHolder)
			{
				if (this.resourceHolder.ReferenceCount <= 0)
					throw new InvalidOperationException("Resource has been released.");
				++this.resourceHolder.ReferenceCount;
			}
			try
			{
				var newInstance = this.Share(this.resourceHolder);
				if (!object.ReferenceEquals(newInstance, this))
					return newInstance;
				throw new InvalidOperationException("Cannot share the same instance.");
			}
			catch
			{
				lock (this.resourceHolder)
				{
					--this.resourceHolder.ReferenceCount;
					if (this.resourceHolder.ReferenceCount == 0)
						this.resourceHolder.CallRelease();
				}
				throw;
			}
		}


		/// <summary>
		/// Create a new instance which shares internal resources.
		/// </summary>
		/// <param name="resourceHolder">Resource holder.</param>
		/// <returns>New instance which shares internal resources.</returns>
		protected abstract TSelf Share(BaseResourceHolder resourceHolder);


		/// <summary>
		/// Throw <see cref="ObjectDisposedException"/> if instance has been disposed.
		/// </summary>
		protected void ThrowIfDisposed()
		{
			if (this.isDisposed)
				throw new ObjectDisposedException(this.GetType().Name);
		}
	}
}
