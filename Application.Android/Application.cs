using Android.Content;
using Android.OS;
using CarinaStudio.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace CarinaStudio.Android;

/// <summary>
/// Implementation of <see cref="IApplication"/> based-on <see cref="global::Android.App.Application"/>.
/// </summary>
public abstract class Application : global::Android.App.Application, IApplication
{
    // Fields.
    CultureInfo? cultureInfo;
    Looper? looper;
    ISettings? persistentState;
    string? rootPrivateDirectoryPath;
    ISettings? settings;
    SynchronizationContext? synchronizationContext;


    /// <summary>
    /// Initialize new <see cref="Application"/> instance.
    /// </summary>
    protected Application()
    { 
        this.Assembly = Assembly.GetExecutingAssembly();
    }


    /// <inheritdoc/>
    public Assembly Assembly { get; }


    /// <inheritdoc/>
    public bool CheckAccess() =>
        this.looper?.IsCurrentThread == true;
    

    /// <inheritdoc/>
    public CultureInfo CultureInfo { get => this.cultureInfo ?? CultureInfo.InvariantCulture; }
    

    /// <inheritdoc/>
    public virtual IObservable<string?> GetObservableString(string key) =>
        new FixedObservableValue<string?>(this.GetString(key));
    

    /// <inheritdoc/>
    public abstract string? GetString(string key, string? defaultValue = null);
    

    /// <inheritdoc/>
    bool IApplication.IsShutdownStarted => false;


    /// <inheritdoc/>
    public virtual ILoggerFactory LoggerFactory { get => throw new NotImplementedException(); }


    /// <inheritdoc/>
    public override void OnConfigurationChanged(global::Android.Content.Res.Configuration newConfig)
    {
        // call base
        base.OnConfigurationChanged(newConfig);

        // update culture info
        var locales = newConfig.Locales;
        if (locales != null && !locales.IsEmpty)
        {
            var newCultureInfo = CultureInfo.GetCultureInfo(locales.Get(0)!.ToLanguageTag());
            if (this.cultureInfo == null || this.cultureInfo.ToString() != newCultureInfo.ToString())
            {
                this.cultureInfo = newCultureInfo;
                this.OnPropertyChanged(nameof(CultureInfo));
            }
        }

        // update strings
        this.StringsUpdated?.Invoke(this, EventArgs.Empty);
    }


    /// <inheritdoc/>
    public override void OnCreate()
    {
        // setup synchronization context
        this.looper = Looper.MyLooper().AsNonNull();
        this.synchronizationContext = new Threading.LooperSynchronizationContext(this.looper);
        System.Threading.SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);

        // get current culture
        var locales = this.Resources?.Configuration?.Locales;
        if (locales != null && !locales.IsEmpty)
            this.cultureInfo = CultureInfo.GetCultureInfo(locales.Get(0)!.ToLanguageTag());

        // call base
        base.OnCreate();

        // create persistent states
        this.persistentState = new Configuration.SharedPreferencesSettings(this.GetSharedPreferences("persistent_state", FileCreationMode.Private).AsNonNull());

        // create settings
        this.settings = Configuration.SharedPreferencesSettings.GetDefault(this);
    }


    /// <summary>
    /// Raise <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    protected virtual void OnPropertyChanged(string propertyName) =>
        this.PropertyChanged?.Invoke(this, new(propertyName));
    

    /// <inheritdoc/>
    public ISettings PersistentState { get => this.persistentState ?? throw new InvalidOperationException(); }


    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;


    /// <inheritdoc/>
    public virtual string RootPrivateDirectoryPath
    {
        get
        {
            if (this.rootPrivateDirectoryPath == null)
                this.rootPrivateDirectoryPath = this.DataDir!.AbsolutePath;
            return this.rootPrivateDirectoryPath;
        }
    }


    /// <inheritdoc/>
    public virtual ISettings Settings { get => this.settings ?? throw new InvalidOperationException(); }


    /// <inheritdoc/>
    public event EventHandler? StringsUpdated;


    /// <inheritdoc/>
    public new SynchronizationContext SynchronizationContext { get => this.synchronizationContext ?? throw new InvalidOperationException("Cannot get SynchronizationContext before calling OnCreate()."); }
}