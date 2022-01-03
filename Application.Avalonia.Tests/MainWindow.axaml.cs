using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace CarinaStudio
{
    partial class MainWindow : Controls.Window<IApp>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        void Test()
        {
            //new TestDialog().ShowDialog(this);
            Platform.OpenFileManager(Environment.CurrentDirectory);
        }
    }
}
