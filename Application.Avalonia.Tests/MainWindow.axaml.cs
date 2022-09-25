using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Runtime.InteropServices;
using CarinaStudio.Animation;
using CarinaStudio.MacOS.AppKit;
using CarinaStudio.MacOS.ObjectiveC;

namespace CarinaStudio
{
    partial class MainWindow : Controls.Window<IApp>
    {
        DoubleAnimator? animator;
        TestDialog? testDialog;


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


        public void Test()
        {
            if (Platform.IsMacOS)
            {
                //var cls = Class.GetClass("NSApplication");
                //var properties = cls.GetProperties().Also(it => Array.Sort(it, (l, r) => l.Name.CompareTo(r.Name)));
                //var ivars = cls.GetInstanceVariables().Also(it => Array.Sort(it, (l, r) => l.Name.CompareTo(r.Name)));

                var app = NSApplication.Current;
            }
            
            /*
            if (this.testDialog == null)
            {
                this.testDialog = new TestDialog().Also(it =>
                {
                    it.Closed += (_, e) => this.testDialog = null;
                });
                this.testDialog.Show(this);
            }
            else
                new TestDialog().ShowDialog(this.testDialog);
            */
            
            /*
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
            */
        }
    }
}
