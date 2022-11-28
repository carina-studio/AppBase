using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using CarinaStudio.Collections;
using CarinaStudio.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extended <see cref="Avalonia.Controls.SelectableTextBlock"/>.
    /// </summary>
    public class SelectableTextBlock : Avalonia.Controls.SelectableTextBlock, IStyleable
    {
        /// <summary>
        /// Property of <see cref="IsMultiLineText"/>.
        /// </summary>
        public static readonly DirectProperty<SelectableTextBlock, bool> IsMultiLineTextProperty = AvaloniaProperty.RegisterDirect<SelectableTextBlock, bool>(nameof(IsMultiLineText), v => v.isMultiLineText);
        /// <summary>
        /// Property of <see cref="IsTextTrimmed"/>.
        /// </summary>
        public static readonly DirectProperty<SelectableTextBlock, bool> IsTextTrimmedProperty = AvaloniaProperty.RegisterDirect<SelectableTextBlock, bool>(nameof(IsTextTrimmed), v => v.isTextTrimmed);
        /// <summary>
        /// Property of <see cref="ShowToolTipWhenTextTrimmed"/>.
        /// </summary>
        public static readonly StyledProperty<bool> ShowToolTipWhenTextTrimmedProperty = AvaloniaProperty.Register<SelectableTextBlock, bool>(nameof(ShowToolTipWhenTextTrimmed), true);


        // Constants.
        const int MaxToolTipLength = 1024;


        // Fields.
        InlineCollection? attachedInlines;
        IDisposable? isWindowActiveObserverToken;
        bool isMultiLineText;
        bool isTextTrimmed;
        readonly List<(int, int)> textLineRanges = new();
        readonly ScheduledAction updateToolTipAction;
        Window? window;


        /// <summary>
        /// Initialize new <see cref="SelectableTextBlock"/> instance.
        /// </summary>
        public SelectableTextBlock()
        {
            var isCtor = true;
            this.GetObservable(InlinesProperty).Subscribe(inlines => 
            {
                if (this.attachedInlines != null)
                    this.attachedInlines.CollectionChanged -= this.OnInlinesChanged;
                this.attachedInlines = inlines;
                if (inlines != null)
                    inlines.CollectionChanged += this.OnInlinesChanged;
                if (!isCtor)
                    this.CheckMultiLine();
            });
            this.GetObservable(IsTextTrimmedProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
            this.GetObservable(ShowToolTipWhenTextTrimmedProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
            this.GetObservable(TextProperty).Subscribe(text => 
            {
                if (!isCtor)
                    this.CheckMultiLine();
            });
            this.GetObservable(TextTrimmingProperty).Subscribe(textTrimming =>
            {
                if (textTrimming == TextTrimming.None)
                    this.SetAndRaise<bool>(IsTextTrimmedProperty, ref this.isTextTrimmed, false);
            });
            this.TextTrimming = TextTrimming.CharacterEllipsis;
            this.updateToolTipAction = new ScheduledAction(() =>
            {
                if (!this.isTextTrimmed
                    || !this.GetValue<bool>(ShowToolTipWhenTextTrimmedProperty)
                    || (Platform.IsMacOS && this.window?.IsActive == false))
                {
                    this.ClearValue(ToolTip.TipProperty);
                }
                else
                {
                    var inlines = this.Inlines;
                    var text = inlines.IsNotEmpty() ? inlines.Text : this.Text;
                    if (string.IsNullOrEmpty(text))
                        this.ClearValue(ToolTip.TipProperty);
                    else if (text.Length <= MaxToolTipLength)
                        this.SetValue<object?>(ToolTip.TipProperty, text);
                    else
                        this.SetValue<object?>(ToolTip.TipProperty, $"{text[0..MaxToolTipLength]}…");
                }
            });
            isCtor = false;
        }


        // Check whether text inside text block has multiple lines or not.
        unsafe void CheckMultiLine()
        {
            var inlines = this.Inlines;
            var text = inlines.IsNotEmpty() ? inlines.Text : this.Text;
            this.textLineRanges.Clear();
            if (!string.IsNullOrEmpty(text))
            {
                fixed (char* textPtr = text)
                {
                    var start = 0;
                    var end = 0;
                    var textLength = text.Length;
                    var cPtr = textPtr;
                    while (end < textLength)
                    {
                        ++end;
                        if (*(cPtr++) == '\n')
                        {
                            this.textLineRanges.Add((start, end));
                            start = end;
                        }
                    }
                    if (start < textLength)
                        this.textLineRanges.Add((start, textLength));
                }
            }
            this.updateToolTipAction?.Schedule();
        }


        /// <summary>
        /// Check whether text inside the <see cref="TextBlock"/> has multiple lines or not.
        /// </summary>
        public bool IsMultiLineText { get => this.isMultiLineText; }


        /// <summary>
        /// Check whether text inside the <see cref="TextBlock"/> has been trimmed or not.
        /// </summary>
        public bool IsTextTrimmed { get => this.isTextTrimmed; }


        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            var measuredSize = base.MeasureOverride(availableSize);
            bool isRemeasureNeeded = false;
            if (double.IsFinite(availableSize.Width))
            {
                // check multi line
                if (this.textLineRanges.IsEmpty())
                    this.SetAndRaise<bool>(IsMultiLineTextProperty, ref this.isMultiLineText, false);
                else if (this.textLineRanges.Count > 1)
                    this.SetAndRaise<bool>(IsMultiLineTextProperty, ref this.isMultiLineText, true);
                else if (this.TextWrapping == TextWrapping.NoWrap)
                    this.SetAndRaise<bool>(IsMultiLineTextProperty, ref this.isMultiLineText, false);
                else
                {
                    var minSize = base.MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    isRemeasureNeeded = true;
                    this.SetAndRaise<bool>(IsMultiLineTextProperty, ref this.isMultiLineText, measuredSize.Height > minSize.Height + 0.1);
                }

                // check trimming
                if (this.TextTrimming != TextTrimming.None)
                {
                    if (this.TextWrapping != TextWrapping.NoWrap)
                    {
                        if (double.IsFinite(availableSize.Height) && this.textLineRanges.Count > availableSize.Height)
                            this.SetAndRaise<bool>(IsTextTrimmedProperty, ref this.isTextTrimmed, true);
                        else
                        {
                            var minSize = base.MeasureOverride(new Size(availableSize.Width, double.PositiveInfinity));
                            isRemeasureNeeded = true;
                            this.SetAndRaise<bool>(IsTextTrimmedProperty, ref this.isTextTrimmed, minSize.Height > measuredSize.Height);
                        }
                    }
                    else if (this.textLineRanges.IsEmpty())
                        this.SetAndRaise<bool>(IsTextTrimmedProperty, ref this.isTextTrimmed, false);
                    else if (this.textLineRanges.Count == 1)
                    {
                        if (this.textLineRanges[0].Item2 > availableSize.Width)
                            this.SetAndRaise<bool>(IsTextTrimmedProperty, ref this.isTextTrimmed, true);
                        else
                        {
                            var minSize = base.MeasureOverride(new Size(availableSize.Width + this.FontSize * 2, this.FontSize));
                            isRemeasureNeeded = true;
                            this.SetAndRaise<bool>(IsTextTrimmedProperty, ref this.isTextTrimmed, minSize.Width > measuredSize.Width);
                        }
                    }
                    else
                    {
                        if (double.IsFinite(availableSize.Height) && this.textLineRanges.Count > availableSize.Height)
                            this.SetAndRaise<bool>(IsTextTrimmedProperty, ref this.isTextTrimmed, true);
                        else
                        {
                            var isTextTrimmedChecked = false;
                            for (int i = 0, count = this.textLineRanges.Count; i < count; ++i)
                            {
                                var range = this.textLineRanges[i];
                                if ((range.Item2 - range.Item1) > availableSize.Width)
                                {
                                    this.SetAndRaise<bool>(IsTextTrimmedProperty, ref this.isTextTrimmed, true);
                                    isTextTrimmedChecked = true;
                                    break;
                                }
                            }
                            if (!isTextTrimmedChecked)
                            {
                                var minSize = base.MeasureOverride(new Size(double.PositiveInfinity, availableSize.Height));
                                isRemeasureNeeded = true;
                                this.SetAndRaise<bool>(IsTextTrimmedProperty, ref this.isTextTrimmed, minSize.Width > measuredSize.Width);
                            }
                        }
                    }
                }
            }
            return isRemeasureNeeded ? base.MeasureOverride(availableSize) : measuredSize;
        }


        /// <inheritdoc/>
        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            this.window = this.FindLogicalAncestorOfType<Window>();
            if (Platform.IsMacOS)
            {
                this.isWindowActiveObserverToken = this.window?.GetObservable(Window.IsActiveProperty)?.Subscribe(_ => 
                    this.updateToolTipAction.Schedule());
            }
        }


        /// <inheritdoc/>
        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            this.isWindowActiveObserverToken = this.isWindowActiveObserverToken.DisposeAndReturnNull();
            this.window = null;
            base.OnDetachedFromLogicalTree(e);
        }


        // Called when inlines changed.
        void OnInlinesChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
            this.CheckMultiLine();


        /// <summary>
        /// Get or set whether tooltip is needed to be shown if text inside the control has been trimmed or not.
        /// </summary>
        public bool ShowToolTipWhenTextTrimmed
        {
            get => this.GetValue<bool>(ShowToolTipWhenTextTrimmedProperty);
            set => this.SetValue<bool>(ShowToolTipWhenTextTrimmedProperty, value);
        }


        // Interface implementation.
        Type IStyleable.StyleKey { get; } = typeof(Avalonia.Controls.SelectableTextBlock);
    }
}
