using Android.OS;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading;

namespace CarinaStudio.Android;

/// <summary>
/// Base class of Activity which implements <see cref="IApplicationObject"/>.
/// </summary>
public abstract class Activity : global::Android.App.Activity, IApplicationObject, INotifyPropertyChanged
{
    // Fields.
    volatile ILogger? logger;
    ActivityState state = ActivityState.New;


    /// <summary>
    /// Initialize new <see cref="Activity"/> instance.
    /// </summary>
    protected Activity()
    {
        this.Application = Android.Application.Current;
    }


    /// <summary>
    /// Get application.
    /// </summary>
    public new Android.Application Application { get; }


    /// <inheritdoc/>
    IApplication IApplicationObject.Application { get => this.Application; }


    /// <inheritdoc/>
    public bool CheckAccess() =>
        this.Application.CheckAccess();
    

    /// <inheritdoc/>
    public override void Finish()
    {
        base.Finish();
        this.OnPropertyChanged(nameof(IsFinishing));
    }
    

    /// <summary>
    /// Check whether the window of activity has been focued or not.
    /// </summary>
    public bool IsWindowFocused { get; private set; }
    

    /// <summary>
    /// Get logger.
    /// </summary>
    protected virtual ILogger Logger
    {
        get => this.logger ?? this.Application.LoggerFactory.CreateLogger(this.GetType().Name).Also(it =>
        {
            this.logger = it;
        });
    }


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
    public override void OnPictureInPictureModeChanged(bool isInPictureInPictureMode)
    {
        base.OnPictureInPictureModeChanged(isInPictureInPictureMode);
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
                    this.Logger.LogDebug($"Change state: {this.state} -> {value}");
                this.state = value;
                this.OnPropertyChanged(nameof(State));
            }
        }
    }
    

    /// <inheritdoc/>
    public SynchronizationContext SynchronizationContext { get => this.Application.SynchronizationContext; }
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