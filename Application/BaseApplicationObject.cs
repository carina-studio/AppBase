using CarinaStudio.Configuration;
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
        protected ISettings PersistentState => this.Application.PersistentState;


        /// <summary>
        /// Get settings of application.
        /// </summary>
        protected ISettings Settings => this.Application.Settings;


        /// <inheritdoc/>.
        public SynchronizationContext SynchronizationContext => this.Application.SynchronizationContext;
    }


    /// <summary>
    /// Base implementation of <see cref="IApplicationObject{TApp}"/>.
    /// </summary>
    /// <typeparam name="TApp">Type of application.</typeparam>
    public abstract class BaseApplicationObject<TApp> : BaseApplicationObject, IApplicationObject<TApp> where TApp : class, IApplication
    {
        /// <summary>
        /// Initialize new <see cref="BaseApplicationObject{TApp}"/> instance.
        /// </summary>
        /// <param name="app">Application.</param>
        protected BaseApplicationObject(TApp app) : base(app)
        { }


        /// <inheritdoc/>
        public new TApp Application => (TApp)base.Application;
    }
    
    
    /// <summary>
    /// Base implementation of <see cref="IApplicationObject{TApp}"/>.
    /// </summary>
    /// <typeparam name="TApp">Type of application.</typeparam>
    /// <typeparam name="TSyncContext">Type of <see cref="SynchronizationContext"/>.</typeparam>
    public abstract class BaseApplicationObject<TApp, TSyncContext> : BaseApplicationObject<TApp>, IApplicationObject<TApp, TSyncContext> where TApp : class, IApplication<TSyncContext> where TSyncContext : SynchronizationContext 
    {
        /// <summary>
        /// Initialize new <see cref="BaseApplicationObject{TApp, TSyncContext}"/> instance.
        /// </summary>
        /// <param name="app">Application.</param>
        protected BaseApplicationObject(TApp app) : base(app)
        { }


        /// <inheritdoc/>
        public new TSyncContext SynchronizationContext => this.Application.SynchronizationContext;
    }
}
