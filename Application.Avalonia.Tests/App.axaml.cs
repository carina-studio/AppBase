using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CarinaStudio.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;

namespace CarinaStudio
{
    public class App : Application, IApp
    {
        // Build application.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();


        // Initialize.
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }


        // Program entry.
        public static void Main(string[] args)
        {
            //Environment.SetEnvironmentVariable("AVALONIA_SCREEN_SCALE_FACTORS", "Virtual-1=2");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }


        // Called when Avalonia initialized.
        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();
            (this.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Let(it =>
            {
                //if (Platform.IsMacOS)
                    //it.ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown;
                it.MainWindow = new MainWindow();
            });
        }


        // Implementations.
        public override CultureInfo CultureInfo => CultureInfo.CurrentCulture;
        public override IObservable<string?> GetObservableString(string key) => new FixedObservableValue<string?>(null);
        public override string? GetString(string key, string? defaultValue = null) => defaultValue;
        public override bool IsShutdownStarted => false;
        public override ILoggerFactory LoggerFactory { get; } = new LoggerFactory();
        public override ISettings PersistentState { get; } = new MemorySettings();
        public override ISettings Settings { get; } = new MemorySettings();
    }
}
