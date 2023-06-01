using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Text;
using System.Threading;

namespace CarinaStudio.Controls;

/// <summary>
/// <see cref="TextBox"/> to let user input a real number.
/// </summary>
public class RealNumberTextBox : ValueTextBox<double>
{
    /// <summary>
    /// Property of <see cref="IsNaNAllowed"/>.
    /// </summary>
    public static readonly StyledProperty<bool> IsNaNAllowedProperty = AvaloniaProperty.Register<RealNumberTextBox, bool>(nameof(IsNaNAllowed), false);
    /// <summary>
    /// Property of <see cref="Maximum"/>.
    /// </summary>
    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<RealNumberTextBox, double>(nameof(Maximum), double.MaxValue, validate: double.IsFinite);
    /// <summary>
    /// Property of <see cref="Minimum"/>.
    /// </summary>
    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<RealNumberTextBox, double>(nameof(Minimum), double.MinValue, validate: double.IsFinite);


    /// <summary>
    /// Initialize new <see cref="RealNumberTextBox"/> instance.
    /// </summary>
    public RealNumberTextBox()
    { }


    /// <summary>
    /// Get or set whether <see cref="double.NaN"/> is allowed or not.
    /// </summary>
    public bool IsNaNAllowed
    {
        get => this.GetValue(IsNaNAllowedProperty);
        set => this.SetValue(IsNaNAllowedProperty, value);
    }
    
    
    /// <summary>
    /// Get or set maximum value.
    /// </summary>
    public double Maximum
    {
        get => this.GetValue(MaximumProperty);
        set
        {
            if (value < this.Minimum)
                value = this.Minimum;
            this.SetValue(MaximumProperty, value);
        }
    }


    /// <summary>
    /// Get or set minimum value.
    /// </summary>
    public double Minimum
    {
        get => this.GetValue(MinimumProperty);
        set
        {
            if (value > this.Maximum)
                value = this.Maximum;
            this.SetValue(MinimumProperty, value);
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
                        var min = this.Minimum;
                        if (value > min)
                        {
                            this.Value = value > (min + 1) ? (value - 1) : min;
                            this.SelectAll();
                        }
                    }
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (this.Validate() && this.Value.HasValue)
                    {
                        var value = this.Value.GetValueOrDefault();
                        var max = this.Maximum;
                        if (value < max)
                        {
                            this.Value = value < (max - 1) ? (value + 1) : max;
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
            if (property == DefaultValueProperty)
            {
                if (this.DefaultValue < this.Minimum)
                    this.DefaultValue = this.Minimum;
                else if (this.DefaultValue > this.Maximum)
                    this.DefaultValue = this.Maximum;
            }
            else if (property == IsNaNAllowedProperty)
            {
                if (!(bool)change.NewValue!)
                    this.Validate();
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
                if (change.NewValue is string s)
                {
                    var length = s.Length;
                    var areValidChars = true;
                    for (var i = length - 1; i >= 0 && areValidChars; --i)
                    {
                        var c = s[i];
                        if (c >= '0' && c <= '9')
                            continue;
                        switch (c)
                        {
                            case '+':
                            case '-':
                            case '.':
                            case 'a':
                            case 'e':
                            case 'i':
                            case 'n':
                            case 'p':
                                break;
                            default:
                                areValidChars = false;
                                break;
                        }
                    }
                    if (!areValidChars)
                    {
                        var newText = new StringBuilder();
                        for (var i = 0; i < length; ++i)
                        {
                            var c = s[i];
                            if (c >= '0' && c <= '9')
                                newText.Append(c);
                            else
                            {
                                switch (c)
                                {
                                    case '+':
                                    case '-':
                                    case '.':
                                    case 'a':
                                    case 'e':
                                    case 'i':
                                    case 'n':
                                    case 'p':
                                        newText.Append(c);
                                        break;
                                }
                            }
                        }
                        SynchronizationContext.Current?.Post(_ =>
                        {
                            if (this.Text == (change.NewValue as string))
                                this.Text = newText.ToString();
                        }, null);
                    }
                }
            }
        }
    
    
    /// <inheritdoc/>
    protected override void OnTextInput(TextInputEventArgs e)
    {
        var s = e.Text;
        if (!string.IsNullOrEmpty(s))
        {
            var c = char.ToLower(s[0]);
            if (c < '0' || c > '9')
            {
                switch (c)
                {
                    case '+':
                    case '-':
                    case '.':
                    case 'a':
                    case 'e':
                    case 'i':
                    case 'n':
                    case 'p':
                        break;
                    default:
                        e.Handled = true;
                        break;
                }
            }
        }
        base.OnTextInput(e);
    }
    
    
    /// <inheritdoc/>
    protected override bool TryConvertToValue(string text, out double? value)
    {
        // NaN
        if (string.Equals(text, "nan", StringComparison.OrdinalIgnoreCase))
        {
            if (this.GetValue(IsNaNAllowedProperty))
            {
                value = double.NaN;
                return true;
            }
            value = null;
            return false;
        }
        
        // convert to number
        double n;
        if (string.Equals(text, "e", StringComparison.OrdinalIgnoreCase))
            n = Math.E;
        else if (string.Equals(text, "pi", StringComparison.OrdinalIgnoreCase))
            n = Math.PI;
        else if (!double.TryParse(text, out n))
        {
            value = null;
            return false;
        }
        
        // check bounds
        if (n >= this.GetValue(MinimumProperty) && n <= this.GetValue(MaximumProperty))
        {
            value = n;
            return true;
        }
        value = null;
        return false;
    }
}