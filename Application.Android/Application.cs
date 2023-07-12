using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using CarinaStudio.Android.Threading;
using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace CarinaStudio.Android;

/// <summary>
/// Implementation of <see cref="IApplication"/> based-on <see cref="global::Android.App.Application"/>.
/// </summary>
public abstract class Application : global::Android.App.Application, IAndroidApplication
{
    // Static fields.
    static volatile Application? CurrentApp;


    // Fields.
    CultureInfo? cultureInfo;
    Looper? looper;
    ISettings? persistentState;
    string? rootPrivateDirectoryPath;
    ISettings? settings;
    readonly IDictionary<string, int> stringResIdMap = new ConcurrentDictionary<string, int>();
    LooperSynchronizationContext? synchronizationContext;


    /// <summary>
    /// Initialize new <see cref="Application"/> instance.
    /// </summary>
    protected Application(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    { 
        this.Assembly = Assembly.GetExecutingAssembly();
        this.Logger = new AndroidLogger(this.GetType().Name);
        this.LoggerFactory = new LoggerFactory(new ILoggerProvider[]{ new AndroidLoggerProvider() });
    }


    /// <inheritdoc/>
    public Assembly Assembly { get; }


    /// <inheritdoc/>
    public bool CheckAccess() =>
        this.looper?.IsCurrentThread == true;


    /// <inheritdoc/>
    Context IContextObject.Context => this;
    

    /// <inheritdoc/>
    public CultureInfo CultureInfo => this.cultureInfo ?? CultureInfo.InvariantCulture;


    /// <summary>
    /// Get <see cref="Application"/> instance of current process.
    /// </summary>
    public static Application Current => CurrentApp ?? throw new InvalidOperationException("No application instance in current process.");


    /// <summary>
    /// Get <see cref="Application"/> instance of current process. Return Null if there is no instance created in current process.
    /// </summary>
    public static Application? CurrentOrNull => CurrentApp;


    /// <inheritdoc/>
    public virtual IObservable<string?> GetObservableString(string key) =>
        new FixedObservableValue<string?>(this.GetString(key));
    

    /// <inheritdoc/>
    public virtual string? GetString(string key, string? defaultValue = null)
    {
        var res = this.Resources.AsNonNull();
        if (!this.stringResIdMap.TryGetValue(key, out var resId))
        {
            resId = res.GetIdentifier(key, "string", this.PackageName);
            if (resId == 0)
            {
                this.Logger.LogWarning("Cannot find string with key '{key}'", key);
                return defaultValue;
            }
            this.stringResIdMap.TryAdd(key, resId);
        }
        return res.GetString(resId);
    }


    /// <summary>
    /// Check whether the process is debuggable or not.
    /// </summary>
    public bool IsDebuggable { get; private set; }
    

    /// <inheritdoc/>
    bool IApplication.IsShutdownStarted => false;


    /// <summary>
    /// Get logger.
    /// </summary>
    protected ILogger Logger { get; }


    /// <inheritdoc/>
    public virtual ILoggerFactory LoggerFactory { get; }


    /// <inheritdoc/>
    public override void OnConfigurationChanged(global::Android.Content.Res.Configuration newConfig)
    {
        this.Logger.LogDebug("Configuration changed");

        // call base
        base.OnConfigurationChanged(newConfig);

        // update culture info
        var locales = newConfig.Locales;
        if (!locales.IsEmpty)
        {
            var newCultureInfo = CultureInfo.GetCultureInfo(locales.Get(0)!.ToLanguageTag());
            if (this.cultureInfo == null || this.cultureInfo.ToString() != newCultureInfo.ToString())
            {
                this.Logger.LogDebug("Culture info changed: {cultureInfo} -> {newCultureInfo}", this.cultureInfo, newCultureInfo);
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
        this.Logger.LogDebug("Creating");

        // check debuggable
#pragma warning disable CA1416
#pragma warning disable CA1422
        var appInfo = this.PackageManager!.GetApplicationInfo(this.PackageName!, PackageInfoFlags.MatchDefaultOnly).AsNonNull();
        this.IsDebuggable = (appInfo.Flags & ApplicationInfoFlags.Debuggable) != 0;
#pragma warning restore CA1416
#pragma warning restore CA1422

        // setup synchronization context
        this.looper = Looper.MyLooper().AsNonNull();
        this.synchronizationContext = new LooperSynchronizationContext(this.looper);
        System.Threading.SynchronizationContext.SetSynchronizationContext(this.synchronizationContext);

        // get current culture
        var locales = this.Resources?.Configuration?.Locales;
        if (locales != null && !locales.IsEmpty)
        {
            this.cultureInfo = CultureInfo.GetCultureInfo(locales.Get(0)!.ToLanguageTag());
            this.Logger.LogDebug("Culture info: {cultureInfo}", this.cultureInfo);
        }

        // call base
        base.OnCreate();

        // create persistent states
        this.persistentState = new Configuration.SharedPreferencesSettings(this.GetSharedPreferences("persistent_state", FileCreationMode.Private).AsNonNull());

        // create settings
        this.settings = Configuration.SharedPreferencesSettings.GetDefault(this);

        // complete
        CurrentApp = this;
        this.Logger.LogDebug("Created");
    }


    /// <summary>
    /// Raise <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    protected virtual void OnPropertyChanged(string propertyName) =>
        this.PropertyChanged?.Invoke(this, new(propertyName));
    

    /// <inheritdoc/>
    public ISettings PersistentState => this.persistentState ?? throw new InvalidOperationException();


    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;


    /// <inheritdoc/>
    public virtual string RootPrivateDirectoryPath
    {
        get
        {
            this.rootPrivateDirectoryPath ??= this.DataDir!.AbsolutePath;
            return this.rootPrivateDirectoryPath;
        }
    }


    /// <inheritdoc/>
    public virtual ISettings Settings => this.settings ?? throw new InvalidOperationException();


    /// <inheritdoc/>
    public event EventHandler? StringsUpdated;


    /// <summary>
    /// Get <see cref="LooperSynchronizationContext"/> of main thread.
    /// </summary>
    public new LooperSynchronizationContext SynchronizationContext => this.synchronizationContext ?? throw new InvalidOperationException("Cannot get SynchronizationContext before calling OnCreate().");


    /// <inheritdoc/>
    SynchronizationContext ISynchronizable.SynchronizationContext => this.SynchronizationContext;
}