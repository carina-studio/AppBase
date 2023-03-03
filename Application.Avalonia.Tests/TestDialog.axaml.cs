using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CarinaStudio.Controls;
using CarinaStudio.Threading;
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

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            this.SynchronizationContext.Post(this.Get<Button>("generateResultButton").Focus);
            //if (this.IsShownAsDialog)
                //this.SynchronizationContext.PostDelayed(this.Close, 7000);
        }
    }
}
