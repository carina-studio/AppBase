using CarinaStudio.Configuration;
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
        { }


        /// <inheritdoc/>
        public virtual IApplication Application
        {
            get
            {
                if (this.app != null)
                    return this.app;
                this.app = CarinaStudio.Application.Current;
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
        protected ISettings PersistentState { get => this.Application.PersistentState; }


        /// <summary>
        /// Get application settings.
        /// </summary>
        protected ISettings Settings { get => this.Application.Settings; }


        /// <inheritdoc/>
        public SynchronizationContext SynchronizationContext => this.Application.SynchronizationContext;
    }


    /// <summary>
    /// <see cref="Avalonia.Controls.UserControl"/> which implements <see cref="IApplicationObject{TApp}"/>.
    /// </summary>
    public abstract class UserControl<TApp> : UserControl, IApplicationObject<TApp> where TApp : class, IApplication
    {
        /// <inheritdoc/>
        public new TApp Application { get => (TApp)base.Application; }
    }
}
