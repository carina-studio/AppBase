using Android.Content;
using Android.OS;
using CarinaStudio.Android.Threading;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Threading;

namespace CarinaStudio.Android;

/// <summary>
/// <see cref="AndroidX.AppCompat.App.AppCompatActivity"/> which implements <see cref="IApplicationObject"/>.
/// </summary>
public abstract class AppCompatActivity : AndroidX.AppCompat.App.AppCompatActivity, IApplicationObject, IContextObject, INotifyPropertyChanged
{
    // Fields.
    volatile ILogger? logger;
    ActivityState state = ActivityState.New;


    /// <summary>
    /// Initialize new <see cref="AppCompatActivity"/> instance.
    /// </summary>
    protected AppCompatActivity()
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
                    this.Logger.LogDebug("Change state: {state} -> {value}",this.state, value);
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
public abstract class AppCompatActivity<TApp> : AppCompatActivity, IApplicationObject<TApp> where TApp : class, IAndroidApplication
{
    /// <inheritdoc/>
    TApp IApplicationObject<TApp>.Application => (TApp)((IApplicationObject)this).Application;
}