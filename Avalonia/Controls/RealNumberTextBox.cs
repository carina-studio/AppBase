using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Globalization;
using System.Text;
using System.Threading;

namespace CarinaStudio.Controls;

/// <summary>
/// <see cref="TextBox"/> to let user input a real number.
/// </summary>
public class RealNumberTextBox : ValueTextBox<double>
{
    /// <summary>
    /// Property of <see cref="ActualMaximum"/>.
    /// </summary>
    public static readonly DirectProperty<RealNumberTextBox, double> ActualMaximumProperty = AvaloniaProperty.RegisterDirect<RealNumberTextBox, double>(nameof(ActualMaximum), t => t.actualMaximum);
    /// <summary>
    /// Property of <see cref="ActualMinimum"/>.
    /// </summary>
    public static readonly DirectProperty<RealNumberTextBox, double> ActualMinimumProperty = AvaloniaProperty.RegisterDirect<RealNumberTextBox, double>(nameof(ActualMinimum), t => t.actualMinimum);
    /// <summary>
    /// Property of <see cref="DecimalPlaces"/>.
    /// </summary>
    public static readonly StyledProperty<int> DecimalPlacesProperty = AvaloniaProperty.Register<RealNumberTextBox, int>(nameof(DecimalPlaces), -1, validate: v => v >= -1 && v <= 15);
    /// <summary>
    /// Property of <see cref="Increment"/>.
    /// </summary>
    public static readonly StyledProperty<double> IncrementProperty = AvaloniaProperty.Register<RealNumberTextBox, double>(nameof(Increment), 1, validate: v => double.IsFinite(v) && v > 0);
    /// <summary>
    /// Property of <see cref="IsNaNAllowed"/>.
    /// </summary>
    public static readonly StyledProperty<bool> IsNaNAllowedProperty = AvaloniaProperty.Register<RealNumberTextBox, bool>(nameof(IsNaNAllowed), false);
    /// <summary>
    /// Property of <see cref="Maximum"/>.
    /// </summary>
    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<RealNumberTextBox, double>(nameof(Maximum), double.MaxValue, validate: double.IsFinite);
    /// <summary>
    /// Property of <see cref="MinDecimalPlaces"/>.
    /// </summary>
    public static readonly StyledProperty<int> MinDecimalPlacesProperty = AvaloniaProperty.Register<RealNumberTextBox, int>(nameof(MinDecimalPlaces), 0, validate: v => v >= 0);
    /// <summary>
    /// Property of <see cref="Minimum"/>.
    /// </summary>
    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<RealNumberTextBox, double>(nameof(Minimum), double.MinValue, validate: double.IsFinite);
    /// <summary>
    /// Property of <see cref="ValueTextBox{Double}.Value"/>.
    /// </summary>
    public static new readonly DirectProperty<RealNumberTextBox, double?> ValueProperty = AvaloniaProperty.RegisterDirect<RealNumberTextBox, double?>(nameof(Value), t => t.Value, (t, v) => t.Value = v);


    // Constants.
    const double MaxExactInteger = 9007199254740992; // 2^53


    // Fields.
    double actualMaximum = double.MaxValue;
    double actualMinimum = double.MinValue;


    /// <summary>
    /// Initialize new <see cref="RealNumberTextBox"/> instance.
    /// </summary>
    public RealNumberTextBox()
    {
        this.PseudoClasses.Set(":realNumberTextBox", true);
    }


    /// <summary>
    /// Get actual maximum value, which is <see cref="Maximum"/> rounded down to <see cref="DecimalPlaces"/>.
    /// </summary>
    public double ActualMaximum => this.actualMaximum;


    /// <summary>
    /// Get actual minimum value, which is <see cref="Minimum"/> rounded up to <see cref="DecimalPlaces"/>.
    /// </summary>
    public double ActualMinimum => this.actualMinimum;


    /// <summary>
    /// Calculate actual range of value by rounding minimum value up and maximum value down to given number of decimal places.
    /// If there is no value with given number of decimal places in the range, the actual maximum value will be the same as the actual minimum value.
    /// </summary>
    /// <param name="minimum">Minimum value.</param>
    /// <param name="maximum">Maximum value.</param>
    /// <param name="decimalPlaces">Number of decimal places, or negative number to keep the range.</param>
    /// <returns>Actual minimum and maximum value.</returns>
    internal static (double Minimum, double Maximum) CalculateActualRange(double minimum, double maximum, int decimalPlaces)
    {
        if (decimalPlaces < 0)
            return (minimum, maximum);
        var scale = Math.Pow(10, decimalPlaces);
        var limit = MaxExactInteger / scale;
        var actualMinimum = minimum;
        if (Math.Abs(minimum) < limit)
        {
            var scaledMinimum = Math.Ceiling(minimum * scale);
            actualMinimum = scaledMinimum / scale;
            if (actualMinimum < minimum)
                actualMinimum = (scaledMinimum + 1) / scale;
        }
        var actualMaximum = maximum;
        if (Math.Abs(maximum) < limit)
        {
            var scaledMaximum = Math.Floor(maximum * scale);
            actualMaximum = scaledMaximum / scale;
            if (actualMaximum > maximum)
                actualMaximum = (scaledMaximum - 1) / scale;
        }
        if (actualMinimum > actualMaximum)
            actualMaximum = actualMinimum;
        return (actualMinimum, actualMaximum);
    }


    /// <inheritdoc/>
    protected override double CoerceValue(double value)
    {
        if (double.IsNaN(value))
            return value;
        value = RoundToDecimalPlaces(value, this.GetValue(DecimalPlacesProperty));
        if (value < this.actualMinimum)
            return this.actualMinimum;
        if (value > this.actualMaximum)
            return this.actualMaximum;
        return value;
    }


    /// <inheritdoc/>
    protected override string? ConvertToText(double value) =>
        PadDecimalPlaces(value, this.GetValue(MinDecimalPlacesProperty), NumberFormatInfo.CurrentInfo);


    /// <summary>
    /// Get or set number of decimal places of value. Value will be rounded to given number of decimal places, and the range of value will be narrowed to <see cref="ActualMinimum"/> and <see cref="ActualMaximum"/>.
    /// Negative number means value won't be rounded.
    /// </summary>
    public int DecimalPlaces
    {
        get => this.GetValue(DecimalPlacesProperty);
        set => this.SetValue(DecimalPlacesProperty, value);
    }


    /// <summary>
    /// Get or set the amount to increase or decrease the value when pressing Up or Down key.
    /// </summary>
    public double Increment
    {
        get => this.GetValue(IncrementProperty);
        set => this.SetValue(IncrementProperty, value);
    }


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
    /// Get or set minimum number of decimal places to be displayed. Trailing zeros are appended to the text if needed, and the value is not affected.
    /// </summary>
    public int MinDecimalPlaces
    {
        get => this.GetValue(MinDecimalPlacesProperty);
        set => this.SetValue(MinDecimalPlacesProperty, value);
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
        if (!e.Handled && e.KeyModifiers == 0)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (this.Validate() && this.Value.HasValue)
                    {
                        var value = this.Value.GetValueOrDefault();
                        var min = this.actualMinimum;
                        if (value > min)
                        {
                            var increment = this.Increment;
                            this.Value = value > (min + increment) ? (value - increment) : min;
                            this.SelectAll();
                        }
                    }
                    e.Handled = true;
                    return;
                case Key.Up:
                    if (this.Validate() && this.Value.HasValue)
                    {
                        var value = this.Value.GetValueOrDefault();
                        var max = this.actualMaximum;
                        if (value < max)
                        {
                            var increment = this.Increment;
                            this.Value = value < (max - increment) ? (value + increment) : max;
                            this.SelectAll();
                        }
                    }
                    e.Handled = true;
                    return;
            }
        }
        base.OnKeyDown(e);
    }


    /// <inheritdoc/>
    protected override void OnLosingFocus(FocusChangingEventArgs e)
    {
        base.OnLosingFocus(e);
        if (this.GetValue(MinDecimalPlacesProperty) > 0 || this.GetValue(DecimalPlacesProperty) >= 0)
            this.UpdateTextFromValue();
    }


    /// <inheritdoc/>
    protected override void OnPastingFromClipboard(string? text)
    {
        if (text is null)
            return;
        var length = text.Length;
        if (length == 0)
        {
            base.OnPastingFromClipboard(text);
            return;
        }
        var newText = new StringBuilder();
        fixed (char* p = text)
        {
            if (p is not null)
            {
                var cPtr = p;
                for (var i = 0; i < length; ++i, ++cPtr)
                {
                    var c = unsafe(*cPtr);
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
                    if (this.MaxLength > 0 && newText.Length >= this.MaxLength)
                        break;
                }
            }
        }
        base.OnPastingFromClipboard(newText.ToString());
    }


    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        var property = change.Property;
        if (property == DecimalPlacesProperty
            || property == MaximumProperty
            || property == MinimumProperty)
        {
            this.UpdateActualRange();
            this.Validate();
            this.DefaultValue = this.CoerceValue(this.DefaultValue);
            var value = this.Value;
            if (value.HasValue || !this.IsNullValueAllowed)
                this.Value = this.CoerceValue(value.GetValueOrDefault());
            if (property == DecimalPlacesProperty)
                this.UpdateTextFromValue();
        }
        else if (property == DefaultValueProperty)
        {
            if (this.DefaultValue < this.actualMinimum)
                this.DefaultValue = this.actualMinimum;
            else if (this.DefaultValue > this.actualMaximum)
                this.DefaultValue = this.actualMaximum;
        }
        else if (property == IsNaNAllowedProperty)
        {
            if (!(bool)change.NewValue!)
                this.Validate();
        }
        else if (property == MinDecimalPlacesProperty)
            this.UpdateTextFromValue();
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


    /// <summary>
    /// Convert value to text and append trailing zeros to make the text contain at least given number of decimal places.
    /// The original text is returned if the value is not finite, the text is in exponent form, or the padded text cannot be converted back to the identical value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="minDecimalPlaces">Minimum number of decimal places.</param>
    /// <param name="format">Number format.</param>
    /// <returns>Converted text.</returns>
    internal static string PadDecimalPlaces(double value, int minDecimalPlaces, NumberFormatInfo format)
    {
        var text = value.ToString(format);
        if (minDecimalPlaces <= 0 || !double.IsFinite(value) || text.Contains('E', StringComparison.OrdinalIgnoreCase))
            return text;
        var separator = format.NumberDecimalSeparator;
        var separatorIndex = text.IndexOf(separator, StringComparison.Ordinal);
        var decimalPlaces = separatorIndex >= 0 ? text.Length - separatorIndex - separator.Length : 0;
        if (decimalPlaces >= minDecimalPlaces)
            return text;
        var paddedTextBuffer = new StringBuilder(text);
        if (separatorIndex < 0)
            paddedTextBuffer.Append(separator);
        paddedTextBuffer.Append('0', minDecimalPlaces - decimalPlaces);
        var paddedText = paddedTextBuffer.ToString();
        if (!double.TryParse(paddedText, NumberStyles.Float | NumberStyles.AllowThousands, format, out var paddedValue)
            || BitConverter.DoubleToInt64Bits(paddedValue) != BitConverter.DoubleToInt64Bits(value))
        {
            return text;
        }
        return paddedText;
    }


    /// <inheritdoc/>.
    protected override void RaiseValueChanged(double? oldValue, double? newValue) =>
        this.RaisePropertyChanged(ValueProperty, oldValue, newValue);


    /// <summary>
    /// Round value to given number of decimal places with <see cref="MidpointRounding.AwayFromZero"/>.
    /// The value is returned without rounding if it is not finite, or it is too large to be scaled exactly.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="decimalPlaces">Number of decimal places, or negative number to keep the value.</param>
    /// <returns>Rounded value.</returns>
    internal static double RoundToDecimalPlaces(double value, int decimalPlaces)
    {
        if (decimalPlaces < 0 || !double.IsFinite(value))
            return value;
        var scale = Math.Pow(10, decimalPlaces);
        if (Math.Abs(value) >= MaxExactInteger / scale)
            return value;
        return Math.Round(value * scale, MidpointRounding.AwayFromZero) / scale;
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
        
        // round and check bounds
        n = RoundToDecimalPlaces(n, this.GetValue(DecimalPlacesProperty));
        if (n >= this.actualMinimum && n <= this.actualMaximum)
        {
            value = n;
            return true;
        }
        value = null;
        return false;
    }


    // Update actual range of value.
    void UpdateActualRange()
    {
        var (actualMinimum, actualMaximum) = CalculateActualRange(this.Minimum, this.Maximum, this.DecimalPlaces);
        this.SetAndRaise(ActualMaximumProperty, ref this.actualMaximum, actualMaximum);
        this.SetAndRaise(ActualMinimumProperty, ref this.actualMinimum, actualMinimum);
    }


    // Update text to display current value.
    void UpdateTextFromValue()
    {
        if (!this.Validate())
            return;
        var value = this.Value;
        if (!value.HasValue)
            return;
        var text = this.ConvertToText(value.GetValueOrDefault());
        if (text != this.Text)
            this.Text = text;
    }


    /// <inheritdoc/>
    public override double? Value
    {
        get => (double?)((ValueTextBox)this).Value;
        set => ((ValueTextBox)this).Value = value;
    }
}