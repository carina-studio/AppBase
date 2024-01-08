using CarinaStudio.MacOS.ObjectiveC;
using CarinaStudio.MacOS.AppKit;
using System;
using System.Linq;
using System.Runtime.InteropServices; 

namespace CarinaStudio.MacOS
{
    internal unsafe class Program
    {
        class AppDelegate : NSObject
        {
            // Static fields.
            static readonly Class AppDelegateClass;

            // Static initializer.
            static AppDelegate()
            {
                AppDelegateClass = Class.DefineClass("CsAppDelegate", cls =>
                {
                    cls.DefineMethod<IntPtr>("applicationDidFinishLaunching:", (self, cmd, notification) =>
                    {
                        var style = NSWindow.StyleMask.Closable
                            | NSWindow.StyleMask.Resizable
                            | NSWindow.StyleMask.Titled;
                        new NSWindow(new(20, 20, 300, 200), style, NSWindow.BackingStoreType.Buffered, true)
                        {
                            Title = "Hello",
                        }.MakeKeyAndOrderFront();
                        NSApplication.Shared.Activate(true);
                    });
                    cls.DefineMethod<IntPtr>("applicationWillFinishLaunching:", (self, cmd, notification) =>
                    {
                        //
                    });
                });
            }

            // Constructor.
            public AppDelegate() : base(Initialize(AppDelegateClass.Allocate()), true)
            { }
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            //var app = NSApplication.Shared;
            //app.Delegate = new AppDelegate();

            //app.Run();
        }
    }
}