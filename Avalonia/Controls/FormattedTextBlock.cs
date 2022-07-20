using System;
using Avalonia;
using Avalonia.Styling;
using CarinaStudio.Threading;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// <see cref="TextBlock"/> which shows formatted text on it.
    /// </summary>
    public class FormattedTextBlock : TextBlock, IStyleable
    {
        /// <summary>
        /// Property of <see cref="Arg1"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg1Property = AvaloniaProperty.Register<FormattedTextBlock, object?>(nameof(Arg1));
        /// <summary>
        /// Property of <see cref="Arg2"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg2Property = AvaloniaProperty.Register<FormattedTextBlock, object?>(nameof(Arg2));
        /// <summary>
        /// Property of <see cref="Arg3"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg3Property = AvaloniaProperty.Register<FormattedTextBlock, object?>(nameof(Arg3));
        /// <summary>
        /// Property of <see cref="Arg4"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg4Property = AvaloniaProperty.Register<FormattedTextBlock, object?>(nameof(Arg4));
        /// <summary>
        /// Property of <see cref="Arg5"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg5Property = AvaloniaProperty.Register<FormattedTextBlock, object?>(nameof(Arg5));
        /// <summary>
        /// Property of <see cref="Arg6"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg6Property = AvaloniaProperty.Register<FormattedTextBlock, object?>(nameof(Arg6));
        /// <summary>
        /// Property of <see cref="Arg7"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg7Property = AvaloniaProperty.Register<FormattedTextBlock, object?>(nameof(Arg7));
        /// <summary>
        /// Property of <see cref="Arg8"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg8Property = AvaloniaProperty.Register<FormattedTextBlock, object?>(nameof(Arg8));
        /// <summary>
        /// Property of <see cref="Arg9"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> Arg9Property = AvaloniaProperty.Register<FormattedTextBlock, object?>(nameof(Arg9));
        /// <summary>
        /// Property of <see cref="Format"/>.
        /// </summary>
        public static readonly AvaloniaProperty<string?> FormatProperty = AvaloniaProperty.Register<FormattedTextBlock, string?>(nameof(Format));


        // Fields.
        readonly IObserver<object?> argObserver;
        bool isUpdatingText;
        readonly ScheduledAction updateTextAction;


        /// <summary>
        /// Initialize new <see cref="FormattedTextBlock"/> instance.
        /// </summary>
        public FormattedTextBlock()
        {
            this.argObserver = new Observer<object?>(_ => this.updateTextAction?.Schedule());
            this.updateTextAction = new ScheduledAction(() =>
            {
                var format = this.GetValue<string?>(FormatProperty);
                var result = string.IsNullOrEmpty(format)
                    ? ""
                    : string.Format(format, new object?[] {
                        this.GetValue<object?>(Arg1Property),
                        this.GetValue<object?>(Arg2Property),
                        this.GetValue<object?>(Arg3Property),
                        this.GetValue<object?>(Arg4Property),
                        this.GetValue<object?>(Arg5Property),
                        this.GetValue<object?>(Arg6Property),
                        this.GetValue<object?>(Arg7Property),
                        this.GetValue<object?>(Arg8Property),
                        this.GetValue<object?>(Arg9Property),
                    });
                this.isUpdatingText = true;
                this.SetValue<string?>(TextProperty, result);
                this.isUpdatingText = false;
            });
            this.GetObservable(Arg1Property).Subscribe(this.argObserver);
            this.GetObservable(Arg2Property).Subscribe(this.argObserver);
            this.GetObservable(Arg3Property).Subscribe(this.argObserver);
            this.GetObservable(Arg4Property).Subscribe(this.argObserver);
            this.GetObservable(Arg5Property).Subscribe(this.argObserver);
            this.GetObservable(Arg6Property).Subscribe(this.argObserver);
            this.GetObservable(Arg7Property).Subscribe(this.argObserver);
            this.GetObservable(Arg8Property).Subscribe(this.argObserver);
            this.GetObservable(Arg9Property).Subscribe(this.argObserver);
            this.GetObservable(FormatProperty).Subscribe(this.argObserver);
            this.GetObservable(TextProperty).Subscribe(_ =>
            {
                if (this.isUpdatingText)
                    return;
                this.ClearValue(Arg1Property);
                this.ClearValue(Arg2Property);
                this.ClearValue(Arg3Property);
                this.ClearValue(Arg4Property);
                this.ClearValue(Arg5Property);
                this.ClearValue(Arg6Property);
                this.ClearValue(Arg7Property);
                this.ClearValue(Arg8Property);
                this.ClearValue(Arg9Property);
                this.ClearValue(FormatProperty);
                this.updateTextAction.Cancel();
            });
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
        Type IStyleable.StyleKey { get; } = typeof(Avalonia.Controls.TextBlock);
    }
}