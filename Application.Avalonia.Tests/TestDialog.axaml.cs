using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CarinaStudio.Controls;
using System;

namespace CarinaStudio
{
    partial class TestDialog : Dialog<App>
    {
        public TestDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void GenerateResult()
        {
            var random = new Random();
            this.Close(random.Next(256));
        }
    }
}
