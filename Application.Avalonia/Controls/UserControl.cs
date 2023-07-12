using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// <see cref="Avalonia.Controls.UserControl"/> which implements <see cref="IApplicationObject"/>.
    /// </summary>
    public abstract class UserControl : Avalonia.Controls.UserControl, IApplicationObject
    {
        // Fields.
        IApplication? app;
        volatile ILogger? logger;


        /// <summary>
        /// Initialize new <see cref="UserControl"/> instance.
        /// </summary>
        protected UserControl()
        {
            this.SynchronizationContext = this.Application.SynchronizationContext as DispatcherSynchronizationContext ?? DispatcherSynchronizationContext.UIThread;
        }


        /// <inheritdoc/>
        public IApplication Application
        {
            get
            {
                if (this.app != null)
                    return this.app;
                this.app = Avalonia.Application.Current as IApplication ?? throw new NotSupportedException("Application instance doesn't implement IApplication interface.");
                return this.app;
            }
        }


        /// <summary>
        /// Get logger.
        /// </summary>
        protected ILogger Logger
        {
            get
            {
                if (this.logger != null)
                    return this.logger;
                lock (this)
                {
                    if (this.logger != null)
                        return this.logger;
                    this.logger = this.Application.LoggerFactory.CreateLogger(this.GetType().Name);
                }
                return this.logger;
            }
        }


        /// <summary>
        /// Get persistent state.
        /// </summary>
        protected ISettings PersistentState => this.Application.PersistentState;


        /// <summary>
        /// Get application settings.
        /// </summary>
        protected ISettings Settings => this.Application.Settings;


        /// <summary>
        /// Get <see cref="DispatcherSynchronizationContext"/> for UI thread.
        /// </summary>
        public DispatcherSynchronizationContext SynchronizationContext { get; }


        /// <inheritdoc/>
        SynchronizationContext ISynchronizable.SynchronizationContext => this.SynchronizationContext;
    }


    /// <summary>
    /// <see cref="Avalonia.Controls.UserControl"/> which implements <see cref="IApplicationObject{TApp}"/>.
    /// </summary>
    /// <typeparam name="TApp">Type of application.</typeparam>
    public abstract class UserControl<TApp> : UserControl, IApplicationObject<TApp> where TApp : class, IAvaloniaApplication
    {
        /// <inheritdoc/>
        public new TApp Application => (TApp)base.Application;
    }
}
