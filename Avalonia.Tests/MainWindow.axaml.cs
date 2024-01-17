using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using CarinaStudio.Controls;
using CarinaStudio.Input.Platform;
using CarinaStudio.Threading;
using System;
using System.Diagnostics;

namespace CarinaStudio
{
    partial class MainWindow : Controls.Window
    {
        static readonly DirectProperty<MainWindow, DateTime?> DateTimeValueProperty = AvaloniaProperty.RegisterDirect<MainWindow, DateTime?>(nameof(DateTimeValue), 
            w => w.dateTimeValue, 
            (w, d) => w.DateTimeValue = d);
        static readonly DirectProperty<MainWindow, IPAddress?> IPAddressObjectProperty = AvaloniaProperty.RegisterDirect<MainWindow, IPAddress?>(nameof(IPAddressObject), 
            w => w.ipAddressObject, 
            (w, o) => w.IPAddressObject = o);
        static readonly DirectProperty<MainWindow, string> LongTextProperty = AvaloniaProperty.RegisterDirect<MainWindow, string>(nameof(LongText), 
            w => w.longText);


        DateTime? dateTimeValue = DateTime.Now;
        IPAddress? ipAddressObject = IPAddress.Loopback;
        string longText = "";
        readonly DispatcherScheduledAction scheduledAction;
        readonly Stopwatch stopwatch = new Stopwatch().Also(it => it.Start());
        
        public MainWindow()
        {
            this.RefreshLongText();
            AvaloniaXamlLoader.Load(this);

            var startTime = 0L;
            this.scheduledAction = new(this, () =>
            {
                var duration = (this.stopwatch.ElapsedMilliseconds - startTime);
            }, DispatcherPriority.Send);

            startTime = this.stopwatch.ElapsedMilliseconds;
            this.scheduledAction.Schedule(5566);
            //this.scheduledAction.Cancel();
        }


        public async void ClipboardTest()
        {
            var clipboard = this.Clipboard!;
            var data = new byte[] { 128 };
            await clipboard.SetTextAndDataAsync("Text", "CustomDataFormat", data);
            var dataAndText = await clipboard.GetDataOrTextAsync("CustomDataFormat");
            var textAndData = await clipboard.GetTextOrDataAsync("CustomDataFormat");
        }


        public DateTime? DateTimeValue
        {
            get => this.dateTimeValue;
            set => this.SetAndRaise(DateTimeValueProperty, ref this.dateTimeValue, value);
        }


        public void ExecuteLinkTextBlockCommand(object? parameter) =>
            (parameter as Avalonia.Controls.TextBlock)?.Let(it => it.Text = "Command executed!!!");
        

        public void IncreateProgressRingValue() => 
            this.FindControl<ProgressRing>("progressRing2")?.Let(it => it.Value = it.Minimum + (it.Value + 9) % (it.Maximum - it.Minimum));
        
        
        public IPAddress? IPAddressObject
        {
            get => this.ipAddressObject;
            set => this.SetAndRaise(IPAddressObjectProperty, ref this.ipAddressObject, value);
        }


        string LongText => this.GetValue(LongTextProperty);


        public void RefreshLongText()
        {
            var s = new string(new char[65536 * 10].Also(it =>
            {
                var r = new Random();
                for (var i = it.Length - 1; i >= 0; --i)
                {
                    if (r.NextDouble() < 0.1)
                        it[i] = ' ';
                    else if ((i % 1024) == 0 && i > 0 && false)
                        it[i] = '\n';
                    else
                    {
                        var n = r.Next(36);
                        it[i] = n < 10 ? (char)('0' + n) : (char)('A' + (n - 10));
                    }
                }
            }));
            this.SetAndRaise(LongTextProperty, ref this.longText, s);
        }


        public void SetDateTimeToDateTimeValue() =>
            this.DateTimeValue = DateTime.Now;


        public void SetDateTimeToDateTimeTextBox() =>
            this.FindControl<DateTimeTextBox>("dateTimeTextBox3")?.Let(it => it.Value = DateTime.Now);


        public void SetIPAddressToIPAddressObject() =>
            this.IPAddressObject = IPAddress.Loopback;


        public void SetIPAddressToIPAddressTextBox() =>
            this.FindControl<IPAddressTextBox>("ipAddressTextBox3")?.Let(it => it.Object = IPAddress.Loopback);
        

        public void SetTimeSpanToTimeSpanTextBox() =>
            this.FindControl<TimeSpanTextBox>("timeSpanTextBox3")?.Let(it => it.Value = DateTime.Now - new DateTime(1970, 1, 1));

        public void SetUriToUriTextBox() =>
            this.FindControl<UriTextBox>("uriTextBox4")?.Let(it => it.Object = new Uri("https://github.com/"));
    }
}
