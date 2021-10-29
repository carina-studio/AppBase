using CarinaStudio.Configuration;
using System;
using System.Threading;

namespace CarinaStudio
{
    /// <summary>
    /// Base implementation of <see cref="IApplicationObject"/>.
    /// </summary>
    public abstract class BaseApplicationObject : IApplicationObject
    {
        /// <summary>
        /// Initialize new <see cref="BaseApplicationObject"/> instance.
        /// </summary>
        /// <param name="app">Application.</param>
        protected BaseApplicationObject(IApplication app)
        {
            this.Application = app;
        }


        /// <inheritdoc/>.
        public IApplication Application { get; }


        /// <inheritdoc/>.
        public bool CheckAccess() => this.Application.CheckAccess();


        /// <summary>
        /// Get persistent state of application.
        /// </summary>
        protected ISettings PersistentState { get => this.Application.PersistentState; }


        /// <summary>
        /// Get settings of application.
        /// </summary>
        protected ISettings Settings { get => this.Application.Settings; }


        /// <inheritdoc/>.
        public SynchronizationContext SynchronizationContext { get => this.Application.SynchronizationContext; }
    }


    /// <summary>
    /// Base implementation of <see cref="IApplicationObject{TApp}"/>.
    /// </summary>
    public abstract class BaseApplicationObject<TApp> : BaseApplicationObject, IApplicationObject<TApp> where TApp : class, IApplication
    {
        /// <summary>
        /// Initialize new <see cref="BaseApplicationObject{TApp}"/> instance.
        /// </summary>
        /// <param name="app">Application.</param>
        protected BaseApplicationObject(TApp app) : base(app)
        { }


        /// <inheritdoc/>
        public new TApp Application { get => (TApp)base.Application; }
    }
}
