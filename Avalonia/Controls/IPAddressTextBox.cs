﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
        public static readonly StyledProperty<bool> IPv4OnlyProperty = AvaloniaProperty.Register<IPAddressTextBox, bool>(nameof(IPv4Only), false);
        /// <summary>
        /// Property of <see cref="ObjectTextBox{IPAddress}.Object"/>.
        /// </summary>
        public static new readonly DirectProperty<IPAddressTextBox, IPAddress?> ObjectProperty = AvaloniaProperty.RegisterDirect<IPAddressTextBox, IPAddress?>(nameof(Object), t => t.Object, (t, o) => t.Object = o);


        /// <summary>
        /// Initialize new <see cref="IPAddressTextBox"/> instance.
        /// </summary>
        public IPAddressTextBox()
        {
            this.MaxLength = 1024;
            this.PseudoClasses.Set(":ipAddressTextBox", true);
        }


        /// <summary>
        /// Get or set whether only IPv4 can be used or not.
        /// </summary>
        public bool IPv4Only
        {
            get => this.GetValue(IPv4OnlyProperty);
            set => this.SetValue(IPv4OnlyProperty, value);
        }


        /// <inheritdoc/>.
        public override IPAddress? Object
        {
            get => (IPAddress?)((ObjectTextBox)this).Object;
            set => ((ObjectTextBox)this).Object = value;
        }


        /// <inheritdoc/>.
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IPv4OnlyProperty)
                this.Validate();
        }


        /// <inheritdoc/>
        protected override void OnTextInput(TextInputEventArgs e)
        {
            var s = e.Text;
            if (!string.IsNullOrEmpty(s))
            {
                var c = s[0];
                switch (c)
                {
                    case '.':
                        break;
                    case ':':
                        if (this.GetValue(IPv4OnlyProperty))
                            e.Handled = true;
                        break;
                    default:
                        if (c >= '0' && c <= '9')
                            break;
                        if (!this.GetValue(IPv4OnlyProperty))
                        {
                            if (c >= 'a' && c <= 'z')
                                break;
                            if (c >= 'A' && c <= 'Z')
                                break;
                        }
                        e.Handled = true;
                        break;
                }
            }
            base.OnTextInput(e);
        }
        
        
        /// <inheritdoc/>.
        protected override void RaiseObjectChanged(IPAddress? oldValue, IPAddress? newValue) =>
            this.RaisePropertyChanged(ObjectProperty, oldValue, newValue);


        /// <inheritdoc/>
        protected override bool TryConvertToObject(string text, out IPAddress? obj) 
        {
            if (IPAddress.TryParse(text, out obj))
            {
                if (obj.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    || !this.GetValue(IPv4OnlyProperty))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
