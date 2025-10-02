using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using System.Threading;

namespace CarinaStudio;

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
    [ThreadSafe]
    public IApplication Application { get; }


    /// <inheritdoc/>.
    [ThreadSafe]
    public bool CheckAccess() => this.Application.CheckAccess();


    /// <summary>
    /// Get persistent state of application.
    /// </summary>
    [ThreadSafe]
    protected ISettings PersistentState => this.Application.PersistentState;


    /// <summary>
    /// Get settings of application.
    /// </summary>
    [ThreadSafe]
    protected ISettings Settings => this.Application.Settings;


    /// <inheritdoc/>.
    [ThreadSafe]
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
    [ThreadSafe]
    public new TApp Application => (TApp)base.Application;
}