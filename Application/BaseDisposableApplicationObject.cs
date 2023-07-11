using System;
using System.Threading;

namespace CarinaStudio
{
    /// <summary>
    /// Base implementation of <see cref="IApplicationObject"/> and <see cref="IDisposable"/>.
    /// </summary>
    public abstract class BaseDisposableApplicationObject : BaseApplicationObject, IDisposable
    {
        // Fields.
		volatile int isDisposed;


        /// <summary>
        /// Initialize new <see cref="BaseDisposableApplicationObject"/> instance.
        /// </summary>
        /// <param name="app">Application.</param>
        protected BaseDisposableApplicationObject(IApplication app) : base(app)
        { }


        /// <summary>
		/// Finalizer.
		/// </summary>
		~BaseDisposableApplicationObject() => this.Dispose(false);


		/// <summary>
		/// Dispose instance.
		/// </summary>
		public void Dispose()
		{
			if (Interlocked.Exchange(ref this.isDisposed, 1) != 0)
				return;
			GC.SuppressFinalize(this);
			this.Dispose(true);
		}


		/// <summary>
		/// Called to dispose instance.
		/// </summary>
		/// <param name="disposing">True to release managed resources also.</param>
		protected abstract void Dispose(bool disposing);


		/// <summary>
		/// Check whether instance has disposed or not.
		/// </summary>
		protected bool IsDisposed => this.isDisposed != 0;


		/// <summary>
		/// Throw <see cref="ObjectDisposedException"/> if instance has been disposed.
		/// </summary>
		protected void VerifyDisposed()
		{
			if (this.isDisposed != 0)
				throw new ObjectDisposedException(this.GetType().Name);
		}
    }


    /// <summary>
    /// Base implementation of <see cref="IApplicationObject"/> and <see cref="IDisposable"/>.
    /// </summary>
    /// <typeparam name="TApp">Type of application.</typeparam>
    public abstract class BaseDisposableApplicationObject<TApp> : BaseDisposableApplicationObject, IApplicationObject<TApp> where TApp : class, IApplication
    {
        /// <summary>
        /// Initialize new <see cref="BaseDisposableApplicationObject{TApp}"/> instance.
        /// </summary>
        /// <param name="app">Application.</param>
        protected BaseDisposableApplicationObject(TApp app) : base(app)
        { }


        /// <inheritdoc/>
        public new TApp Application => (TApp)base.Application;
    }
    
    
    /// <summary>
    /// Base implementation of <see cref="IApplicationObject"/> and <see cref="IDisposable"/>.
    /// </summary>
    /// <typeparam name="TApp">Type of application.</typeparam>
    /// <typeparam name="TSyncContext">Type of <see cref="SynchronizationContext"/>.</typeparam>
    public abstract class BaseDisposableApplicationObject<TApp, TSyncContext> : BaseDisposableApplicationObject<TApp>, IApplicationObject<TApp, TSyncContext> where TApp : class, IApplication<TSyncContext> where TSyncContext : SynchronizationContext
    {
	    /// <summary>
	    /// Initialize new <see cref="BaseDisposableApplicationObject{TApp, TSyncContext}"/> instance.
	    /// </summary>
	    /// <param name="app">Application.</param>
	    protected BaseDisposableApplicationObject(TApp app) : base(app)
	    { }


	    /// <inheritdoc/>
	    public new TSyncContext SynchronizationContext => this.Application.SynchronizationContext;
    }
}