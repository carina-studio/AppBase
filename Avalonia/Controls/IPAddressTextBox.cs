using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Net;

namespace CarinaStudio.Controls
{
    /// <summary>
	/// <see cref="TextBox"/> which treat input text as <see cref="IPAddress"/>.
	/// </summary>
    public class IPAddressTextBox : ObjectTextBox<IPAddress>
    {
        /// <summary>
        /// Property of <see cref="IPv4Only"/>.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IPv4OnlyProperty = AvaloniaProperty.Register<IPAddressTextBox, bool>(nameof(IPv4Only), false);


        /// <summary>
        /// Initialize new <see cref="IPAddressTextBox"/> instance.
        /// </summary>
        public IPAddressTextBox()
        {
            this.MaxLength = 1024;
        }


        /// <summary>
        /// Get or set whether only IPv4 can be used or not.
        /// </summary>
        public bool IPv4Only
        {
            get => this.GetValue<bool>(IPv4OnlyProperty);
            set => this.SetValue<bool>(IPv4OnlyProperty, value);
        }


        /// <inheritdoc/>.
        protected override void OnPropertyChanged<TProperty>(AvaloniaPropertyChangedEventArgs<TProperty> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IPv4OnlyProperty)
                this.Validate();
        }


        /// <inheritdoc/>
        protected override void OnTextInput(TextInputEventArgs e)
        {
            var s = e.Text;
            if (s != null && s.Length > 0)
            {
                var c = s[0];
                switch (c)
                {
                    case '.':
                        break;
                    case ':':
                        if (this.GetValue<bool>(IPv4OnlyProperty))
                            e.Text = "";
                        break;
                    default:
                        if (c >= '0' && c <= '9')
                            break;
                        if (!this.GetValue<bool>(IPv4OnlyProperty))
                        {
                            if (c >= 'a' && c <= 'z')
                                break;
                            if (c >= 'A' && c <= 'Z')
                                break;
                        }
                        e.Text = "";
                        break;
                }
            }
            base.OnTextInput(e);
        }


        /// <inheritdoc/>
        protected override bool TryConvertToObject(string text, out IPAddress? obj) 
        {
            if (IPAddress.TryParse(text, out obj))
            {
                if (obj.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    || !this.GetValue<bool>(IPv4OnlyProperty))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
