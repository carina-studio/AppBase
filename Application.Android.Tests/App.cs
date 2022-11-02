using Android.Runtime;
using CarinaStudio.Android;
using System;

namespace CarinaStudio.AppBase.App.Android;

[global::Android.App.Application(Theme="@style/AppTheme")]
public class App : CarinaStudio.Android.Application
{
    // Constructor.
    public App(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    { }
}