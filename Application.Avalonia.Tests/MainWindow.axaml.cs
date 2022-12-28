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
using CarinaStudio.Threading;
using System.ComponentModel;
using System.Linq;
using System.IO;

namespace CarinaStudio
{
    partial class MainWindow : Controls.ApplicationWindow<IApp>
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
                    cls.DefineMethod<int, double, NSSize, string, DateTime, double>(FooSelector, (self, cmd, arg1, arg2, arg3, arg4, arg5) =>
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

            // Foo
            public double Foo(int arg1, double arg2, NSSize arg3, string arg4, DateTime arg5) =>
                SendMessage<double>(FooSelector!, arg1, arg2, arg3, arg4, arg5);
            
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
            protected override void OnRelease()
            {
                MyAppDelegateClass?.TrySetClrObject(this.Handle, null);
                base.OnRelease();
            }
        }


        MyAppDelegate? myAppDelegate;
        MyClass? myClass;
        NSProgressIndicator? progressIndicator;
        ScheduledAction? animateDockTileProgressAction;


        public async void Test()
        {
            if (Platform.IsMacOS)
            {
                var app = NSApplication.Current.AsNonNull();
                
                if (this.progressIndicator == null)
                {
                    var dockTileView = new NSView(new(default, app.DockTile.Size));

                    var dockTileViewImageView = new NSImageView(new(default, app.DockTile.Size))
                    {
                        Image = app.ApplicationIconImage,
                        ImageAlignment = NSImageAlignment.Center,
                        ImageScaling = NSImageScaling.ProportionallyUpOrDown,
                    };
                    dockTileView.AddSubView(dockTileViewImageView);

                    this.progressIndicator = new NSProgressIndicator(new(16, 0, 100, 20))
                    {
                        IsBezeled = true,
                        IsIndeterminate = false,
                        Style = NSProgressIndicatorStyle.Bar,
                    };
                    dockTileView.AddSubView(this.progressIndicator);

                    app.DockTile.ContentView = dockTileView;
                    app.DockTile.Display();

                    this.animateDockTileProgressAction ??= new(() =>
                    {
                        if (this.progressIndicator == null)
                            return;
                        this.progressIndicator.Increment(5);
                        if (Math.Abs(this.progressIndicator.MaxValue - this.progressIndicator.DoubleValue) < 1)
                        {
                            app.DockTile.ContentView = null;
                            this.progressIndicator = this.progressIndicator.Let(it =>
                            {
                                it.RemoveFromSuperView();
                                it.Release();
                                return (NSProgressIndicator?)null;
                            });

                            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        }
                        else
                            this.animateDockTileProgressAction!.Schedule(100);
                        app.DockTile.Display();
                    });

                    this.animateDockTileProgressAction!.Schedule(100);
                }
                else
                {
                    this.progressIndicator.DoubleValue = this.progressIndicator.MaxValue;
                    this.animateDockTileProgressAction?.Execute();
                }
            }
            
            if (this.testDialog == null)
            {
                this.testDialog = new TestDialog().Also(it =>
                {
                    it.Closed += (_, e) => this.testDialog = null;
                });
                this.testDialog.Show();
            }
            else
                new TestDialog().ShowDialog(this.testDialog);
            
            
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
