using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CarinaStudio.Controls;
using CarinaStudio.Input.Platform;
using System;

namespace CarinaStudio
{
    partial class MainWindow : Avalonia.Controls.Window
    {
        static readonly DirectProperty<MainWindow, DateTime?> DateTimeValueProperty = AvaloniaProperty.RegisterDirect<MainWindow, DateTime?>(nameof(DateTimeValue), 
            w => w.dateTimeValue, 
            (w, d) => w.DateTimeValue = d);
        static readonly DirectProperty<MainWindow, IPAddress?> IPAddressObjectProperty = AvaloniaProperty.RegisterDirect<MainWindow, IPAddress?>(nameof(IPAddressObject), 
            w => w.ipAddressObject, 
            (w, o) => w.IPAddressObject = o);


        DateTime? dateTimeValue = DateTime.Now;
        IPAddress? ipAddressObject = IPAddress.Loopback;
        
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
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
        

        string LongText { get; } = new string(new char[65536 * 10].Also(it =>
        {
            var r = new Random();
            for (var i = it.Length - 1; i >= 0; --i)
            {
                if ((i % 1024) == 0 && i > 0 && false)
                    it[i] = '\n';
                else
                {
                    var n = r.Next(36);
                    it[i] = n < 10 ? (char)('0' + n) : (char)('A' + (n - 10));
                }
            }
        }));


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
