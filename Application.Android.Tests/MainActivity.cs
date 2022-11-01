using Android.Util;
using CarinaStudio.Android.Configuration;
using CarinaStudio.Collections;
using CarinaStudio.Configuration;
using System;

namespace CarinaStudio.AppBase.App.Android;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    // Constants.
    const string TAG = nameof(MainActivity);


    // Static fields.
    static readonly SettingKey<double> DoubleValueKey = new("MainActivity.DoubleValue", 3.14159);
    static readonly SettingKey<WindowsVersion> EnumValueKey = new("MainActivity.EnumValue");
    static readonly SettingKey<int> IntValueKey = new("MainActivity.IntValue", 1);


    // Fields.
    ISettings? settings;


    /// <inheritdoc/>
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.activity_main);

        this.settings = SharedPreferencesSettings.GetDefault(this);
        this.settings.ResetValue(DoubleValueKey);
        foreach (var key in this.settings.Keys)
            Log.Debug(TAG, $"OnCreate() - {key}: {this.settings.GetRawValue(key)}");
    }


    /// <inheritdoc/>
    protected override void OnPause()
    {
        Log.Debug(TAG, "OnPause()");
        var random = new Random();
        this.settings.SetValue<double>(DoubleValueKey, random.NextDouble());
        this.settings.SetValue<WindowsVersion>(EnumValueKey, Enum.GetValues<WindowsVersion>().SelectRandomElement());
        this.settings.SetValue<int>(IntValueKey, random.Next(65536));
        base.OnPause();
    }


    /// <inheritdoc/>
    protected override void OnResume()
    {
        base.OnResume();
        Log.Debug(TAG, $"OnResume() - {DoubleValueKey.Name}: {this.settings.GetValueOrDefault(DoubleValueKey)}");
        Log.Debug(TAG, $"OnResume() - {EnumValueKey.Name}: {this.settings.GetValueOrDefault(EnumValueKey)}");
        Log.Debug(TAG, $"OnResume() - {IntValueKey.Name}: {this.settings.GetValueOrDefault(IntValueKey)}");
    }
}