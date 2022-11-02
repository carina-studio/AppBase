using Android.OS;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Threading;

namespace CarinaStudio.Android;

/// <summary>
/// <see cref="AndroidX.AppCompat.App.AppCompatActivity"/> which implements <see cref="IApplicationObject"/>.
/// </summary>
public abstract class AppCompatActivity : AndroidX.AppCompat.App.AppCompatActivity, IApplicationObject
{
    // Fields.
    volatile ILogger? logger;
    ActivityState state = ActivityState.New;


    /// <summary>
    /// Initialize new <see cref="AppCompatActivity"/> instance.
    /// </summary>
    protected AppCompatActivity()
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
                    this.Logger.LogDebug($"Change state: {this.state} -> {value}");
                this.state = value;
                this.OnPropertyChanged(nameof(State));
            }
        }
    }
    

    /// <inheritdoc/>
    public SynchronizationContext SynchronizationContext { get => this.Application.SynchronizationContext; }
}