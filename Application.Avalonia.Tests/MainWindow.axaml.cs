using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using CarinaStudio.Animation;

namespace CarinaStudio
{
    partial class MainWindow : Controls.Window<IApp>
    {
        DoubleAnimator? animator;


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
            /*
            if (this.OwnedWindows.Count > 0)
                _ = new TestDialog().ShowDialog(this);
            else
                new TestDialog().Show(this);
            */

            var transform = this.Find<Rectangle>("rect")?.RenderTransform as TranslateTransform;
            if (transform == null)
                return;

            animator?.Cancel();
            animator = new DoubleAnimator(transform.X, transform.X >= 50 ? 0 : 100).Also(it =>
            {
                it.Completed += (_, e) => transform.X = it.EndValue;
                it.Delay = TimeSpan.FromMilliseconds(500);
                it.Duration = TimeSpan.FromSeconds(1);
                it.Interpolator = Interpolators.Deceleration;
                it.ProgressChanged += (_, e) => transform.X = it.Value;
                it.Start();
            });
        }
    }
}
