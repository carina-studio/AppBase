using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Text;
using System.Threading;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// <see cref="TextBox"/> to let user input an integer in decimal.
    /// </summary>
    public class IntegerTextBox : ValueTextBox<long>
    {
        /// <summary>
        /// Property of <see cref="AcceptsPositiveSign"/>.
        /// </summary>
        public static readonly StyledProperty<bool> AcceptsPositiveSignProperty = AvaloniaProperty.Register<IntegerTextBox, bool>(nameof(AcceptsPositiveSign), false);
        /// <summary>
        /// Property of <see cref="Maximum"/>.
        /// </summary>
        public static readonly StyledProperty<long> MaximumProperty = AvaloniaProperty.Register<IntegerTextBox, long>(nameof(Maximum), long.MaxValue);
        /// <summary>
        /// Property of <see cref="Minimum"/>.
        /// </summary>
        public static readonly StyledProperty<long> MinimumProperty = AvaloniaProperty.Register<IntegerTextBox, long>(nameof(Minimum), long.MinValue);


        // Constants.
        const int MaxTextLength = 20; // max value is +9223372036854775807


        /// <summary>
        /// Initialize new <see cref="IntegerTextBox"/> instance.
        /// </summary>
        public IntegerTextBox()
        {
            this.PseudoClasses.Set(":integerTextBox", true);
        }


        /// <summary>
        /// Get or set whether positive sign (+) can be accpeted or not.
        /// </summary>
        public bool AcceptsPositiveSign
        {
            get => this.GetValue<bool>(AcceptsPositiveSignProperty);
            set => this.SetValue<bool>(AcceptsPositiveSignProperty, value);
        }


        /// <inheritdoc/>
        protected override long CoerceValue(long value)
        {
            if (value < this.Minimum)
                return Minimum;
            else if (value > this.Maximum)
                return this.Maximum;
            return value;
        }


        /// <summary>
        /// Get or set maximum value.
        /// </summary>
        public long Maximum
        {
            get => this.GetValue<long>(MaximumProperty);
            set
            {
                if (value < this.Minimum)
                    value = this.Minimum;
                this.SetValue<long>(MaximumProperty, value);
            }
        }


        /// <summary>
        /// Get or set minimum value.
        /// </summary>
        public long Minimum
        {
            get => this.GetValue<long>(MinimumProperty);
            set
            {
                if (value > this.Maximum)
                    value = this.Maximum;
                this.SetValue<long>(MinimumProperty, value);
            }
        }


        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled && e.KeyModifiers == 0)
            {
                switch (e.Key)
                {
                    case Key.Down:
                        if (this.Validate() && this.Value.HasValue)
                        {
                            var value = this.Value.GetValueOrDefault();
                            if (value > this.Minimum)
                            {
                                this.Value = (value - 1);
                                this.SelectAll();
                            }
                        }
                        e.Handled = true;
                        break;
                    case Key.Up:
                        if (this.Validate() && this.Value.HasValue)
                        {
                            var value = this.Value.GetValueOrDefault();
                            if (value < this.Maximum)
                            {
                                this.Value = (value + 1);
                                this.SelectAll();
                            }
                        }
                        e.Handled = true;
                        break;
                }
            }
        }


        /// <inheritdoc/>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            var property = change.Property;
            if (property == AcceptsPositiveSignProperty)
            {
                if (!this.AcceptsPositiveSign)
                {
                    var s = this.Text;
                    if (s != null && s.Length > 0 && s[0] == '+')
                        this.Text = s[1..^0];
                }
            }
            else if (property == DefaultValueProperty)
            {
                if (this.DefaultValue < this.Minimum)
                    this.DefaultValue = this.Minimum;
                else if (this.DefaultValue > this.Maximum)
                    this.DefaultValue = this.Maximum;
            }
            else if (property == MaximumProperty || property == MinimumProperty)
            {
                this.Validate();
                if (this.DefaultValue < this.Minimum)
                    this.DefaultValue = this.Minimum;
                else if (this.DefaultValue > this.Maximum)
                    this.DefaultValue = this.Maximum;
                if (!this.IsNullValueAllowed)
                    this.Value = this.CoerceValue(this.Value.GetValueOrDefault());
            }
            else if (property == TextProperty)
            {
                var s = (change.NewValue as string);
                if (s != null)
                {
                    if (s.Length > MaxTextLength)
                        s = s[0..MaxTextLength];
                    for (var i = 0; i < s.Length; ++i)
                    {
                        var c = s[i];
                        if (c >= '0' && c <= '9')
                            continue;
                        if (c == '+')
                        {
                            if (this.AcceptsPositiveSign && i == 0)
                                continue;
                        }
                        else if (c == '-')
                        {
                            if (i == 0)
                                continue;
                        }
                        var newString = new StringBuilder(s[0..i]);
                        for (++i; i < s.Length; ++i)
                        {
                            c = s[i];
                            if (c >= '0' && c <= '9')
                                newString.Append(c);
                        }
                        SynchronizationContext.Current?.Post(_ =>
                        {
                            if (this.Text == (change.NewValue as string))
                                this.Text = newString.ToString();
                        }, null);
                        break;
                    }
                }
            }
        }


        /// <inheritdoc/>
        protected override void OnTextInput(TextInputEventArgs e)
        {
            var s = e.Text;
            if (s != null && s.Length > 0)
            {
                if (this.Text?.Length > MaxTextLength)
                    e.Text = "";
                else
                {
                    var c = s[0];
                    if (c < '0' || c > '9')
                    {
                        if (c == '+')
                        {
                            if (!this.AcceptsPositiveSign || this.Maximum <= 0 || this.SelectionStart != 0)
                                e.Text = "";
                        }
                        else if (c == '-')
                        {
                            if (this.Minimum >= 0 || this.SelectionStart != 0)
                                e.Text = "";
                        }
                        else
                            e.Text = "";
                    }
                }
            }
            base.OnTextInput(e);
        }


        /// <inheritdoc/>
        protected override bool TryConvertToValue(string text, out long? value)
        {
            if (long.TryParse(text, out var number) && number >= this.Minimum && number <= this.Maximum)
            {
                value = number;
                return true;
            }
            value = null;
            return false;
        }
    }
}
