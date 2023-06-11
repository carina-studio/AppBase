using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace CarinaStudio
{
    public class App : Application
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
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);


        // Called when Avalonia initialized.
        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();
            this.Styles.Add(new StyleInclude(new Uri("avares://CarinaStudio.AppBase.Avalonia"))
            {
                Source = new("Theme/Default.axaml", UriKind.Relative),
            });
            (this.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Let(it =>
            {
                it.MainWindow = new MainWindow();
            });
        }
    }
}
