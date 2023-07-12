using Android.Content;
using Android.OS;
using CarinaStudio.Android.Threading;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Threading;

namespace CarinaStudio.Android;

/// <summary>
/// Base class of Activity which implements <see cref="IApplicationObject"/>.
/// </summary>
public abstract class Activity : global::Android.App.Activity, IApplicationObject, IContextObject, INotifyPropertyChanged
{
    // Fields.
    volatile ILogger? logger;
    ActivityState state = ActivityState.New;


    /// <summary>
    /// Initialize new <see cref="Activity"/> instance.
    /// </summary>
    protected Activity()
    {
        this.Application = Application.Current;
    }


    /// <summary>
    /// Get application.
    /// </summary>
    public new Application Application { get; }


    /// <inheritdoc/>
    IApplication IApplicationObject.Application => this.Application;


    /// <inheritdoc/>
    public bool CheckAccess() =>
        this.Application.CheckAccess();


    /// <inheritdoc/>
    Context IContextObject.Context => this;
    

    /// <inheritdoc/>
    public override void Finish()
    {
        base.Finish();
        this.OnPropertyChanged(nameof(IsFinishing));
    }
    

    /// <summary>
    /// Check whether the window of activity has been focused or not.
    /// </summary>
    public bool IsWindowFocused { get; private set; }
    

    /// <summary>
    /// Get logger.
    /// </summary>
    protected virtual ILogger Logger =>
        this.logger ?? this.Application.LoggerFactory.CreateLogger(this.GetType().Name).Also(it =>
        {
            this.logger = it;
        });


    /// <inheritdoc/>
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        this.State = ActivityState.Created;
    }


    /// <inheritdoc/>
    protected override void OnDestroy()
    {
        this.State = ActivityState.Destroyed;
        this.OnPropertyChanged(nameof(IsDestroyed));
        base.OnDestroy();
    }


    /// <inheritdoc/>
    public override void OnMultiWindowModeChanged(bool isInMultiWindowMode, global::Android.Content.Res.Configuration? newConfig)
    {
        base.OnMultiWindowModeChanged(isInMultiWindowMode, newConfig);
        this.OnPropertyChanged(nameof(IsInMultiWindowMode));
    }


    /// <inheritdoc/>
    protected override void OnPause()
    {
        this.State = ActivityState.Started;
        base.OnPause();
    }


    /// <inheritdoc/>
    public override void OnPictureInPictureModeChanged(bool isInPictureInPictureMode, global::Android.Content.Res.Configuration? newConfig)
    {
        base.OnPictureInPictureModeChanged(isInPictureInPictureMode, newConfig);
        this.OnPropertyChanged(nameof(IsInPictureInPictureMode));
    }


    /// <summary>
    /// Raise <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    protected virtual void OnPropertyChanged(string propertyName) =>
        this.PropertyChanged?.Invoke(this, new(propertyName));
    

    /// <inheritdoc/>
    protected override void OnResume()
    {
        base.OnResume();
        this.State = ActivityState.Resumed;
    }
    

    /// <inheritdoc/>
    protected override void OnStart()
    {
        base.OnStart();
        this.State = ActivityState.Started;
    }


    /// <inheritdoc/>
    protected override void OnStop()
    {
        this.State = ActivityState.Created;
        base.OnStop();
    }


    /// <inheritdoc/>
    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);
        this.IsWindowFocused = hasFocus;
        this.OnPropertyChanged(nameof(IsWindowFocused));
    }


    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;


    /// <summary>
    /// Get current state of activity.
    /// </summary>
    public ActivityState State
    {
        get => this.state;
        private set
        {
            if (this.state != value)
            {
                if (this.Application.IsDebuggable)
                    this.Logger.LogDebug("Change state: {state} -> {value}", this.state, value);
                this.state = value;
                this.OnPropertyChanged(nameof(State));
            }
        }
    }
    

    /// <summary>
    /// Get <see cref="LooperSynchronizationContext"/> of main thread.
    /// </summary>
    public LooperSynchronizationContext SynchronizationContext => this.Application.SynchronizationContext;
    
    
    /// <inheritdoc/>
    SynchronizationContext ISynchronizable.SynchronizationContext => this.Application.SynchronizationContext;
}


/// <summary>
/// Activity which implements <see cref="IApplicationObject{T}"/>.
/// </summary>
/// <typeparam name="TApp">Type of application.</typeparam>
public abstract class Activity<TApp> : Activity, IApplicationObject<TApp> where TApp : class, IAndroidApplication
{
    /// <inheritdoc/>
    TApp IApplicationObject<TApp>.Application => (TApp)((IApplicationObject)this).Application;
}


/// <summary>
/// State of activity.
/// </summary>
public enum ActivityState
{
    /// <summary>
    /// The instance has just been created.
    /// </summary>
    New,
    /// <summary>
    /// After calling <see cref="global::Android.App.Activity.OnCreate(Bundle)"/> or <see cref="global::Android.App.Activity.OnStop"/>.
    /// </summary>
    Created,
    /// <summary>
    /// After calling <see cref="global::Android.App.Activity.OnStart"/> or <see cref="global::Android.App.Activity.OnPause"/>.
    /// </summary>
    Started,
    /// <summary>
    /// After calling <see cref="global::Android.App.Activity.OnResume"/>.
    /// </summary>
    Resumed,
    /// <summary>
    /// After calling <see cref="global::Android.App.Activity.OnDestroy"/>.
    /// </summary>
    Destroyed,
}