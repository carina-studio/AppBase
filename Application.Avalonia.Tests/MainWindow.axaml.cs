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


        class MyClass : NSObject
        {
            // Static fields.
            static readonly Selector? BarSelector;
            static readonly Class? Cls;
            static readonly Selector? FooSelector;
            static PropertyDescriptor? TestProperty;

            // Static initializer.
            static MyClass()
            {
                if (Platform.IsNotMacOS)
                    return;
                BarSelector = Selector.FromName("bar");
                FooSelector = Selector.FromName("foo");
                Cls = Class.DefineClass("MyClass", cls =>
                {
                    cls.DefineMethod(BarSelector, new MethodImplForInt32_Int32((self, cmd, value) =>
                    {
                        if (cls.TryGetClrObject(self, out var clrObj) && clrObj is MyClass myClass)
                            return myClass.BarImpl(value);
                        return 0;
                    }));
                    cls.DefineMethod(FooSelector, new MethodImpl((self, cmd) =>
                    {
                        //
                    }));
                    TestProperty = cls.DefineProperty<int>("test",
                        new Int32PropertyGetterImpl((self, _) =>
                        {
                            return 5566;
                        }),
                        new Int32PropertySetterImpl((self, _, value) =>
                        {
                            //
                        }));
                });

            }

            // Constructor.
            public MyClass() : base(Initialize(Cls!.Allocate()), true)
            { 
                this.Class.TrySetClrObject(this.Handle, this);
            }

            // Bar
            public int Bar(int value) =>
                SendMessageForInt32_Int32(this.Handle, BarSelector!.Handle, value);
            int BarImpl(int value)
            {
                return 54321;
            }

            // Dispose.
            protected override void Dispose(bool disposing)
            {
                //this.Class.TrySetClrObject(this.Handle, null);
                base.Dispose(disposing);
            }

            // Foo
            public void Foo() =>
                SendMessage(FooSelector!);
            
            // Test
            public int Test
            {
                get => this.GetInt32Property(TestProperty!);
                set => this.SetProperty(TestProperty!, value);
            }
        }


        public void Test()
        {
            if (Platform.IsMacOS)
            {
                using var obj = new MyClass();
                //
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
