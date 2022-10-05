using Avalonia;
using Avalonia.Controls;
using AvnWindow = Avalonia.Controls.Window;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Runtime.InteropServices;
using CarinaStudio.Animation;
using CarinaStudio.Collections;
using CarinaStudio.Controls;
using CarinaStudio.MacOS.AppKit;
using CarinaStudio.MacOS.CoreFoundation;
using CarinaStudio.MacOS.CoreGraphics;
using CarinaStudio.MacOS.ImageIO;
using CarinaStudio.MacOS.ObjectiveC;
using System.ComponentModel;
using System.IO;

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
            static Variable? IntPtrVariableVar;
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
                    IntPtrVariableVar = cls.DefineInstanceVariable<IntPtr>("_intPtrVar");
                    cls.DefineInstanceVariable<byte[]>("byteVar", 10);
                    cls.DefineInstanceVariable<NSObject>("nsObjectVar");
                    cls.DefineInstanceVariable<NSObject[]>("nsObjectsVar", 4);
                });
            }

            // Constructor.
            public MyClass() : base(Initialize(Cls!.Allocate()), true)
            { 
                this.Class.TrySetClrObject(this.Handle, this);
                this.SetVariable(IntPtrVariableVar!, (IntPtr)12345);

                var value = (object)this.GetVariable<IntPtr>(IntPtrVariableVar!);

                var nsObjects = new NSObject?[] { null, this };
                var variable = this.Class.GetInstanceVriable("nsObjectsVar");
                this.SetVariable(variable!, nsObjects);

                value = this.GetVariable<NSObject?[]>(variable!);
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
                MyAppDelegateClass.DefineMethod<IntPtr>(Selector.FromName("applicationWillBecomeActive:"),
                    (self, cmd, notification) =>
                    {
                        if (AvnAppDelegateClass?.HasMethod(cmd) == true)
                            NSObject.SendMessageToSuper(self, cmd, notification);
                        if (MyAppDelegateClass.TryGetClrObject<MyAppDelegate>(self, out var myAppDelegate))
                        {
                            myAppDelegate.window.Show();
                            myAppDelegate.window.Activate();
                        }
                    });
                MyAppDelegateClass.DefineMethod<IntPtr>(Selector.FromName("applicationWillResignActive:"),
                    (self, cmd, notification) =>
                    {
                        if (AvnAppDelegateClass?.HasMethod(cmd) == true)
                            NSObject.SendMessageToSuper(self, cmd, notification);
                    });
                MyAppDelegateClass.DefineMethod<IntPtr>(Selector.FromName("applicationWillUnhide:"),
                    (self, cmd, notification) =>
                    {
                        if (AvnAppDelegateClass?.HasMethod(cmd) == true)
                            NSObject.SendMessageToSuper(self, cmd, notification);
                    });
                MyAppDelegateClass.DefineMethod<IntPtr>(Selector.FromName("applicationWillHide:"),
                    (self, cmd, notification) =>
                    {
                        if (AvnAppDelegateClass?.HasMethod(cmd) == true)
                            NSObject.SendMessageToSuper(self, cmd, notification);
                    });
            }

            // Fields.
            readonly AvnWindow window;

            // Constructor.
            public MyAppDelegate(AvnWindow window) : base(Initialize(MyAppDelegateClass!.Allocate()), true)
            { 
                this.Class.TrySetClrObject(this.Handle, this);
                this.window = window;
            }

            // Dispose.
            protected override void Dispose(bool disposing)
            {
                MyAppDelegateClass?.TrySetClrObject(this.Instance.Handle, null);
                base.Dispose(disposing);
            }
        }


        MyAppDelegate? myAppDelegate;
        MyClass? myClass;


        public async void Test()
        {
            if (Platform.IsMacOS)
            {
                var app = NSApplication.Current.AsNonNull();
                var windows = app.Windows;

                var methods = windows.Class.SuperClass!.SuperClass!.GetMethods();
                Array.Sort(methods, (l, r) => l.Name.CompareTo(r.Name));

                var array2 = new NSArray<NSResponder>(windows[0], windows[0], windows[0]);

                foreach (var window in array2)
                    ;
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
