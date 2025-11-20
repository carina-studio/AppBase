using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace CarinaStudio.Controls;

/// <summary>
/// <see cref="Avalonia.Controls.UserControl"/> which implements <see cref="IApplicationObject"/>.
/// </summary>
public abstract class UserControl : Avalonia.Controls.UserControl, IApplicationObject
{
    // Fields.
#if !NET10_0_OR_GREATER
    ILogger? logger;
#endif


    /// <summary>
    /// Initialize new <see cref="UserControl"/> instance.
    /// </summary>
    protected UserControl()
    {
#if !NET10_0_OR_GREATER
        this.Application = Avalonia.Application.Current as IApplication ?? throw new NotSupportedException("Application instance doesn't implement IApplication interface.");
        this.SynchronizationContext = this.Application.SynchronizationContext as DispatcherSynchronizationContext ?? DispatcherSynchronizationContext.UIThread;
#endif
    }


    /// <inheritdoc/>
    [ThreadSafe]
    public IApplication Application
    {
#if NET10_0_OR_GREATER
        get
        {
            field ??= Avalonia.Application.Current as IApplication ?? throw new NotSupportedException("Application instance doesn't implement IApplication interface.");
            return field;
        }
#else
        get;
#endif
    }


    /// <summary>
    /// Get logger.
    /// </summary>
    [ThreadSafe]
    protected ILogger Logger
    {
        get
        {
#if NET10_0_OR_GREATER
            field ??= this.Application.LoggerFactory.CreateLogger(this.LoggerCategoryName);
            return field;
#else
            this.logger ??= this.Application.LoggerFactory.CreateLogger(this.LoggerCategoryName);
            return this.logger;
#endif
        }
    }
    
    
    /// <summary>
    /// Get name of category for logger.
    /// </summary>
    [ThreadSafe]
    protected virtual string LoggerCategoryName => this.GetType().Name;


    /// <summary>
    /// Get persistent state.
    /// </summary>
    [ThreadSafe]
    protected ISettings PersistentState => this.Application.PersistentState;


    /// <summary>
    /// Get application settings.
    /// </summary>
    [ThreadSafe]
    protected ISettings Settings => this.Application.Settings;


    /// <summary>
    /// Get <see cref="DispatcherSynchronizationContext"/> for UI thread.
    /// </summary>
    [ThreadSafe]
    public DispatcherSynchronizationContext SynchronizationContext
    {
#if NET10_0_OR_GREATER
        get
        {
            field ??= this.Application.SynchronizationContext as DispatcherSynchronizationContext ?? DispatcherSynchronizationContext.UIThread;
            return field;
        }
#else
        get;
#endif
    }


    /// <inheritdoc/>
    [ThreadSafe]
    SynchronizationContext ISynchronizable.SynchronizationContext => this.SynchronizationContext;
}


/// <summary>
/// <see cref="Avalonia.Controls.UserControl"/> which implements <see cref="IApplicationObject{TApp}"/>.
/// </summary>
/// <typeparam name="TApp">Type of application.</typeparam>
public abstract class UserControl<TApp> : UserControl, IApplicationObject<TApp> where TApp : class, IAvaloniaApplication
{
    /// <inheritdoc/>
    [ThreadSafe]
    public new TApp Application => (TApp)base.Application;
}