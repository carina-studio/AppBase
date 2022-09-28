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
            static Property? TestProperty;

            // Static initializer.
            static MyClass()
            {
                if (Platform.IsNotMacOS)
                    return;
                BarSelector = Selector.FromName("bar");
                FooSelector = Selector.FromName("foo");
                Cls = Class.DefineClass("MyClass", cls =>
                {
                    Class.GetProtocol("NSApplicationDelegate")?.Let(it => cls.AddProtocol(it));
                    cls.DefineMethod<int, int>(BarSelector, (self, cmd, value) =>
                    {
                        if (cls.TryGetClrObject<MyClass>(self, out var clrObj))
                            return clrObj.BarImpl(value);
                        return 0;
                    });
                    cls.DefineMethod<int, double, NSSize, double>(FooSelector, (self, cmd, arg1, arg2, arg3) =>
                    {
                        return 3.14159;
                    });
                    TestProperty = cls.DefineProperty<int>("test",
                        (self, _) =>
                        {
                            return 5566;
                        },
                        (self, _, value) =>
                        {
                            //
                        });
                });

            }

            // Constructor.
            public MyClass() : base(Initialize(Cls!.Allocate()), true)
            { 
                this.Class.TrySetClrObject(this.Handle, this);
            }

            // Bar
            public int Bar(int value) =>
                this.SendMessage<int>(BarSelector!, value);
            int BarImpl(int value)
            {
                return 54321;
            }

            // Dispose.
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
            }

            // Foo
            public double Foo(int arg1, double arg2, NSSize arg3) =>
                SendMessage<double>(FooSelector!, arg1, arg2, arg3);
            
            // Test
            public int Test
            {
                get => this.GetProperty<int>(TestProperty!);
                set => this.SetProperty(TestProperty!, value);
            }
        }


        class MyAppDelegate : NSObject
        {
            // Static fields.
            static readonly Class? AvnAppDelegateClass;
            static readonly Class? MyAppDelegateClass;

            // Static initializer.
            static MyAppDelegate()
            {
                if (Platform.IsNotMacOS)
                    return;
                AvnAppDelegateClass = Class.GetClass("AvnAppDelegate");
                MyAppDelegateClass = Class.DefineClass(AvnAppDelegateClass ?? Class.GetClass("NSObject"), "MyAppDelegate", cls => {});
                Class.GetProtocol("NSApplicationDelegate")?.Let(it => MyAppDelegateClass.AddProtocol(it));
                /*
                MyAppDelegateClass.DefineMethod(Selector.FromUid("applicationWillBecomeActive:"),
                    new MethodImpl_IntPtr((self, _, notification) =>
                    {
                        //
                    }));
                */
            }

            // Constructor.
            public MyAppDelegate() : base(Initialize(MyAppDelegateClass!.Allocate()), true)
            { 
                //this.Class.TrySetClrObject(this.Handle, this);
            }
        }


        MyAppDelegate? myAppDelegate;
        MyClass? myClass;


        public void Test()
        {
            if (Platform.IsMacOS)
            {
                using var myClass = new MyClass();
                var r = myClass.Bar(1234);
                var d = myClass.Foo(5566, 1.41421, new NSSize(1920, 1080));

                var p = myClass.Test;
                myClass.Test = 9521;
                //myAppDelegate ??= new();

                //var app = NSApplication.Current.AsNonNull();
                
                //var currentDelegate = app.Delegate;
                
                //if (currentDelegate != this.myAppDelegate)
                    //app.Delegate = this.myAppDelegate;
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
