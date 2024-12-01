using Avalonia;
using Avalonia.Controls;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// <see cref="TextBox"/> to let user input a <see cref="TimeSpan"/>.
    /// </summary>
    public class TimeSpanTextBox : ValueTextBox<TimeSpan>
    {
        /// <summary>
        /// Property of <see cref="ValueTextBox{TimeSpan}.Value"/>.
        /// </summary>
        public static new readonly DirectProperty<TimeSpanTextBox, TimeSpan?> ValueProperty = AvaloniaProperty.RegisterDirect<TimeSpanTextBox, TimeSpan?>(nameof(Value), t => t.Value, (t, v) => t.Value = v);
        
        
        // Static fields.
        static readonly Regex CustomFormatRegex = new("^[\\+\\-]?((?<Days>[\\d]+)(\\s+|\\.+|\\-+)(?<Hours>[\\d]{1,2})|(?<Hours>[\\d]+))(\\s+|\\:+|\\.+|\\-+)(?<Minutes>[\\d]{1,2})(\\s+|\\:+|\\.+|\\-+)(?<Seconds>[\\d]{1,2}(\\.[\\d]+)?)[\\s]*$");
        static readonly CultureInfo DefaultCultureInfo = CultureInfo.GetCultureInfo("en-US");
        static readonly Regex MicrosecondsFormatRegex = new("^(?<Number>[\\d]+)[\\s]*us[\\s]*$");
        static readonly Regex MillisecondsFormatRegex = new("^(?<Number>[\\d]+)[\\s]*ms[\\s]*$");
        static readonly Regex NanosecondsFormatRegex = new("^(?<Number>[\\d]+)[\\s]*ns[\\s]*$");


        /// <summary>
        /// Initialize new <see cref="TimeSpanTextBox"/> instance.
        /// </summary>
        public TimeSpanTextBox()
        {
            this.MaxLength = 128;
            this.PseudoClasses.Set(":timeSpanTextBox", true);
        }
        
        
        /// <inheritdoc/>.
        protected override void RaiseValueChanged(TimeSpan? oldValue, TimeSpan? newValue) =>
            this.RaisePropertyChanged(ValueProperty, oldValue, newValue);


        /// <inheritdoc/>.
        protected override bool TryConvertToValue(string text, out TimeSpan? value)
        {
            // try parsing by default culture
            value = null;
            if (TimeSpan.TryParse(text, DefaultCultureInfo, out var timeSpan))
            {
                value = timeSpan;
                return true;
            }

            // try parse by current culture
            var currentCultureInfo = CultureInfo.CurrentUICulture;
            if (currentCultureInfo.ToString() != DefaultCultureInfo.ToString()
                && TimeSpan.TryParse(text, currentCultureInfo, out timeSpan))
            {
                value = timeSpan;
                return true;
            }

            // try parse in nanoseconds
            var match = NanosecondsFormatRegex.Match(text);
            if (match.Success)
            {
                try
                {
                    var ns = double.Parse(match.Groups["Number"].Value);
                    value = TimeSpan.FromMilliseconds(ns / 1000000);
                    return true;
                }
                catch
                { 
                    return false;
                }
            }

            // try parse in microseconds
            match = MicrosecondsFormatRegex.Match(text);
            if (match.Success)
            {
                try
                {
                    var us = double.Parse(match.Groups["Number"].Value);
                    value = TimeSpan.FromMilliseconds(us / 1000);
                    return true;
                }
                catch
                { 
                    return false;
                }
            }

            // try parse in milliseconds
            match = MillisecondsFormatRegex.Match(text);
            if (match.Success)
            {
                try
                {
                    var ms = double.Parse(match.Groups["Number"].Value);
                    value = TimeSpan.FromMilliseconds(ms);
                    return true;
                }
                catch
                { 
                    return false;
                }
            }

            // try parse in custom format
            match = CustomFormatRegex.Match(text);
            if (match.Success)
            {
                try
                {
                    var isPositive = text[0] != '-';
                    var hasDays = match.Groups["Days"].Success;
                    var days = hasDays ? int.Parse(match.Groups["Days"].Value) : 0;
                    var hours = int.Parse(match.Groups["Hours"].Value);
                    if (hours >= 24 && hasDays)
                        return false;
                    var mins = int.Parse(match.Groups["Minutes"].Value);
                    if (mins >= 60)
                        return false;
                    var secs = double.Parse(match.Groups["Seconds"].Value);
                    value = TimeSpan.FromDays(isPositive ? days : -days)
                        + TimeSpan.FromHours(isPositive ? hours : -hours)
                        + TimeSpan.FromMinutes(isPositive ? mins : -mins)
                        + TimeSpan.FromSeconds(isPositive ? secs : -secs);
                    return true;
                }
                catch
                { 
                    return false;
                }
            }

            // unable to parse
            return false;
        }
        
        /// <inheritdoc/>
        public override TimeSpan? Value
        {
            get => (TimeSpan?)((ValueTextBox)this).Value;
            set => ((ValueTextBox)this).Value = value;
        }
    }
}