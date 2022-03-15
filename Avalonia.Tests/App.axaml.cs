using Avalonia;
using Avalonia.Markup.Xaml;

namespace CarinaStudio
{
    public class App : Application
    {
        // Initialize.
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }


        // Program entry.
        public static void Main(string[] args)
        { }


        // Called when Avalonia initialized.
        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();
        }
    }
}
