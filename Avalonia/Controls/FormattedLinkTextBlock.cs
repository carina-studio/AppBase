using System;
using Avalonia;
using Avalonia.Styling;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// <see cref="FormattedLinkTextBlock"/> which shows formatted text on it.
    /// </summary>
    public class FormattedLinkTextBlock : LinkTextBlock, IStyleable
    {
        /// <summary>
        /// Property of <see cref="Arg1"/>.
        /// </summary>
        public static readonly StyledProperty<object?> Arg1Property = AvaloniaProperty.Register<FormattedLinkTextBlock, object?>(nameof(Arg1));
        /// <summary>
        /// Property of <see cref="Arg2"/>.
        /// </summary>
        public static readonly StyledProperty<object?> Arg2Property = AvaloniaProperty.Register<FormattedLinkTextBlock, object?>(nameof(Arg2));
        /// <summary>
        /// Property of <see cref="Arg3"/>.
        /// </summary>
        public static readonly StyledProperty<object?> Arg3Property = AvaloniaProperty.Register<FormattedLinkTextBlock, object?>(nameof(Arg3));
        /// <summary>
        /// Property of <see cref="Arg4"/>.
        /// </summary>
        public static readonly StyledProperty<object?> Arg4Property = AvaloniaProperty.Register<FormattedLinkTextBlock, object?>(nameof(Arg4));
        /// <summary>
        /// Property of <see cref="Arg5"/>.
        /// </summary>
        public static readonly StyledProperty<object?> Arg5Property = AvaloniaProperty.Register<FormattedLinkTextBlock, object?>(nameof(Arg5));
        /// <summary>
        /// Property of <see cref="Arg6"/>.
        /// </summary>
        public static readonly StyledProperty<object?> Arg6Property = AvaloniaProperty.Register<FormattedLinkTextBlock, object?>(nameof(Arg6));
        /// <summary>
        /// Property of <see cref="Arg7"/>.
        /// </summary>
        public static readonly StyledProperty<object?> Arg7Property = AvaloniaProperty.Register<FormattedLinkTextBlock, object?>(nameof(Arg7));
        /// <summary>
        /// Property of <see cref="Arg8"/>.
        /// </summary>
        public static readonly StyledProperty<object?> Arg8Property = AvaloniaProperty.Register<FormattedLinkTextBlock, object?>(nameof(Arg8));
        /// <summary>
        /// Property of <see cref="Arg9"/>.
        /// </summary>
        public static readonly StyledProperty<object?> Arg9Property = AvaloniaProperty.Register<FormattedLinkTextBlock, object?>(nameof(Arg9));
        /// <summary>
        /// Property of <see cref="Format"/>.
        /// </summary>
        public static readonly StyledProperty<string?> FormatProperty = AvaloniaProperty.Register<FormattedLinkTextBlock, string?>(nameof(Format));


        // Fields.
        readonly FormattedString formattedString = new FormattedString();
        bool isUpdatingText;


        /// <summary>
        /// Initialize new <see cref="FormattedLinkTextBlock"/> instance.
        /// </summary>
        public FormattedLinkTextBlock()
        {
            var isCtor = true;
            this.formattedString.Subscribe(text =>
            {
                this.isUpdatingText = true;
                this.SetValue<string?>(TextProperty, text);
                this.isUpdatingText = false;
            });
            this.GetObservable(Arg1Property).Subscribe(arg => this.formattedString.Arg1 = arg);
            this.GetObservable(Arg2Property).Subscribe(arg => this.formattedString.Arg2 = arg);
            this.GetObservable(Arg3Property).Subscribe(arg => this.formattedString.Arg3 = arg);
            this.GetObservable(Arg4Property).Subscribe(arg => this.formattedString.Arg4 = arg);
            this.GetObservable(Arg5Property).Subscribe(arg => this.formattedString.Arg5 = arg);
            this.GetObservable(Arg6Property).Subscribe(arg => this.formattedString.Arg6 = arg);
            this.GetObservable(Arg7Property).Subscribe(arg => this.formattedString.Arg7 = arg);
            this.GetObservable(Arg8Property).Subscribe(arg => this.formattedString.Arg8 = arg);
            this.GetObservable(Arg9Property).Subscribe(arg => this.formattedString.Arg9 = arg);
            this.GetObservable(FormatProperty).Subscribe(f => this.formattedString.Format = f);
            this.GetObservable(TextProperty).Subscribe(_ =>
            {
                if (!isCtor && !this.isUpdatingText)
                    throw new InvalidOperationException();
            });
            isCtor = false;
        }


        /// <summary>
        /// Get or set 1st argument to generate formatted string.
        /// </summary>
        public object? Arg1
        {
            get => this.GetValue<object?>(Arg1Property);
            set => this.SetValue<object?>(Arg1Property, value);
        }


        /// <summary>
        /// Get or set 2nd argument to generate formatted string.
        /// </summary>
        public object? Arg2
        {
            get => this.GetValue<object?>(Arg2Property);
            set => this.SetValue<object?>(Arg2Property, value);
        }


        /// <summary>
        /// Get or set 3rd argument to generate formatted string.
        /// </summary>
        public object? Arg3
        {
            get => this.GetValue<object?>(Arg3Property);
            set => this.SetValue<object?>(Arg3Property, value);
        }


        /// <summary>
        /// Get or set 4th argument to generate formatted string.
        /// </summary>
        public object? Arg4
        {
            get => this.GetValue<object?>(Arg4Property);
            set => this.SetValue<object?>(Arg4Property, value);
        }


        /// <summary>
        /// Get or set 5th argument to generate formatted string.
        /// </summary>
        public object? Arg5
        {
            get => this.GetValue<object?>(Arg5Property);
            set => this.SetValue<object?>(Arg5Property, value);
        }


        /// <summary>
        /// Get or set 6th argument to generate formatted string.
        /// </summary>
        public object? Arg6
        {
            get => this.GetValue<object?>(Arg6Property);
            set => this.SetValue<object?>(Arg6Property, value);
        }


        /// <summary>
        /// Get or set 7th argument to generate formatted string.
        /// </summary>
        public object? Arg7
        {
            get => this.GetValue<object?>(Arg7Property);
            set => this.SetValue<object?>(Arg7Property, value);
        }


        /// <summary>
        /// Get or set 8th argument to generate formatted string.
        /// </summary>
        public object? Arg8
        {
            get => this.GetValue<object?>(Arg8Property);
            set => this.SetValue<object?>(Arg8Property, value);
        }


        /// <summary>
        /// Get or set 9th argument to generate formatted string.
        /// </summary>
        public object? Arg9
        {
            get => this.GetValue<object?>(Arg9Property);
            set => this.SetValue<object?>(Arg9Property, value);
        }


        /// <summary>
        /// Get or set string format.
        /// </summary>
        public string? Format
        {
            get => this.GetValue<string?>(FormatProperty);
            set => this.SetValue<string?>(FormatProperty, value);
        }


        /// <summary>
        /// Get formatted text.
        /// </summary>
        public new string? Text { get => base.Text; }


        // Interface implementation.
        Type IStyleable.StyleKey { get; } = typeof(LinkTextBlock);
    }
}