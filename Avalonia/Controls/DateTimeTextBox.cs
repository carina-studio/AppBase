using Avalonia;
using Avalonia.Controls;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// <see cref="TextBox"/> to let user input a <see cref="DateTime"/>.
    /// </summary>
    public class DateTimeTextBox : ValueTextBox<DateTime>
    {
        /// <summary>
        /// Property of <see cref="ValueTextBox{DateTime}.Value"/>.
        /// </summary>
        public static new readonly DirectProperty<DateTimeTextBox, DateTime?> ValueProperty = AvaloniaProperty.RegisterDirect<DateTimeTextBox, DateTime?>(nameof(Value), t => t.Value, (t, v) => t.Value = v);
        
        
        // Constants.
        const long DecadeInSeconds = 3600 * 24 * 365 * 10;
        const long DecadeInMillis = DecadeInSeconds * 1000;
        const long DecadeInMicros = DecadeInMillis * 1000;


        // Static fields.
        static readonly Regex CompactFormatRegex = new("^((?<Year>[\\d]{4})[\\.\\-_]?)?((?<Month>[\\d]{2})|(?<Month>[\\d]{1,2})[\\.\\-_])(?<Day>[\\d]{1,2})[\\s\\-_]+((?<Hours>[\\d]{2})|(?<Hours>[\\d]{1,2})[:\\-_])((?<Minutes>[\\d]{2})|(?<Minutes>[\\d]{1,2})[:\\-_])(?<Seconds>[\\d]{1,2}(\\.[\\d]+)?)?[\\s]*$");
        static readonly CultureInfo DefaultCultureInfo = CultureInfo.GetCultureInfo("en-US");
        static readonly DateTime UnixTimestampBase = new(1970, 1, 1);


        /// <summary>
        /// Initialize new <see cref="DateTimeTextBox"/> instance.
        /// </summary>
        public DateTimeTextBox()
        {
            this.AcceptsWhiteSpaces = true;
            this.MaxLength = 128;
            this.PseudoClasses.Set(":dateTimeTextBox", true);
        }


        /// <inheritdoc/>.
        protected override void RaiseValueChanged(DateTime? oldValue, DateTime? newValue) =>
            this.RaisePropertyChanged(ValueProperty, oldValue, newValue);


        /// <inheritdoc/>.
        protected override bool TryConvertToValue(string text, out DateTime? value)
        {
            // try parsing by default culture
            if (DateTime.TryParse(text, DefaultCultureInfo, DateTimeStyles.None, out var dateTime))
            {
                value = dateTime;
                return true;
            }

            // try parse by current culture
            var currentCultureInfo = CultureInfo.CurrentUICulture;
            if (currentCultureInfo.ToString() != DefaultCultureInfo.ToString()
                && DateTime.TryParse(text, currentCultureInfo, DateTimeStyles.None, out dateTime))
            {
                value = dateTime;
                return true;
            }

            // try parsing by Unix timestamp
            if (double.TryParse(text, out var timestamp) && double.IsFinite(timestamp) && timestamp > 0)
            {
                var currentDateTime = DateTime.Now;
                var currentTimestamp = (currentDateTime - UnixTimestampBase).TotalSeconds;
                if (currentTimestamp + DecadeInSeconds >= timestamp)
                {
                    value = UnixTimestampBase.AddSeconds(timestamp);
                    return true;
                }
                currentTimestamp *= 1000;
                if (currentTimestamp + DecadeInMillis >= timestamp)
                {
                    value = UnixTimestampBase.AddMilliseconds(timestamp);
                    return true;
                }
                currentTimestamp *= 1000;
                if (currentTimestamp + DecadeInMicros >= timestamp)
                {
                    value = UnixTimestampBase.AddMilliseconds(timestamp / 1000);
                    return true;
                }
            }

            // try parse by compact format
            var match = CompactFormatRegex.Match(text);
            if (match.Success)
            {
                // get date
                var currentDate = DateTime.Now;
                var year = match.Groups["Year"].Let(group =>
                {
                    if (group.Success)
                        return int.Parse(group.Value);
                    return currentDate.Year;
                });
                var month = int.Parse(match.Groups["Month"].Value);
                var day = int.Parse(match.Groups["Day"].Value);

                // get time
                var hours = int.Parse(match.Groups["Hours"].Value);
                var minutes = int.Parse(match.Groups["Minutes"].Value);
                var seconds = match.Groups["Seconds"].Let(group =>
                {
                    if (group.Success)
                        return double.Parse(group.Value);
                    return 0.0;
                });

                // create date time
                try
                {
                    var milliseconds = ((seconds - (int)seconds) * 1000);
                    value = new DateTime(year, month, day, hours, minutes, (int)seconds).AddMilliseconds(milliseconds);
                    return true;
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                { }
                // ReSharper restore EmptyGeneralCatchClause
            }

            // unable to parse
            value = null;
            return false;
        }


        /// <inheritdoc/>
        public override DateTime? Value
        {
            get => (DateTime?)((ValueTextBox)this).Value;
            set => ((ValueTextBox)this).Value = value;
        }
    }
}
