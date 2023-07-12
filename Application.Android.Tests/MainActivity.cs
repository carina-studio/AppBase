using Android.Widget;
using CarinaStudio.Android.Configuration;
using CarinaStudio.Collections;
using CarinaStudio.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace CarinaStudio.AppBase.App.Android;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : CarinaStudio.Android.AppCompatActivity<App>
{
    // Static fields.
    static readonly SettingKey<double> DoubleValueKey = new("MainActivity.DoubleValue", 3.14159);
    static readonly SettingKey<WindowsVersion> EnumValueKey = new("MainActivity.EnumValue");
    static readonly SettingKey<int> IntValueKey = new("MainActivity.IntValue", 1);


    // Fields.
    Button? button1;
    ISettings? settings;


    /// <inheritdoc/>
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.activity_main);
        this.button1 = this.FindViewById<Button>(Resource.Id.button1).AsNonNull().Also(it =>
        {
            it.Click += (sender, e) => this.Finish();
        });

        this.settings = SharedPreferencesSettings.GetDefault(this);
        this.settings.ResetValue(DoubleValueKey);
        foreach (var key in this.settings.Keys)
            this.Logger.LogDebug($"OnCreate() - {key}: {this.settings.GetRawValue(key)}");
    }


    /// <inheritdoc/>
    protected override void OnPause()
    {
        this.Logger.LogDebug("OnPause()");
        var random = new Random();
        this.settings?.Let(it =>
        {
            it.SetValue<double>(DoubleValueKey, random.NextDouble());
            it.SetValue<WindowsVersion>(EnumValueKey, Enum.GetValues<WindowsVersion>().SelectRandomElement());
            it.SetValue<int>(IntValueKey, random.Next(65536));
        });
        base.OnPause();
    }


    /// <inheritdoc/>
    protected override void OnResume()
    {
        base.OnResume();
        this.settings?.Let(it =>
        {
            this.Logger.LogDebug($"OnResume() - {DoubleValueKey.Name}: {it.GetValueOrDefault(DoubleValueKey)}");
            this.Logger.LogDebug($"OnResume() - {EnumValueKey.Name}: {it.GetValueOrDefault(EnumValueKey)}");
            this.Logger.LogDebug($"OnResume() - {IntValueKey.Name}: {it.GetValueOrDefault(IntValueKey)}");
        });
    }
}