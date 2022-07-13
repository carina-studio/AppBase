using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using CarinaStudio.Collections;
using CarinaStudio.Threading;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extended <see cref="Avalonia.Controls.TextBlock"/>.
    /// </summary>
    public class TextBlock : Avalonia.Controls.TextBlock, IStyleable
    {
        /// <summary>
        /// Property of <see cref="IsMultiLineText"/>.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsMultiLineTextProperty = AvaloniaProperty.RegisterDirect<TextBlock, bool>(nameof(IsMultiLineText), v => v.isMultiLineText);
        /// <summary>
        /// Property of <see cref="IsTextTrimmed"/>.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsTextTrimmedProperty = AvaloniaProperty.RegisterDirect<TextBlock, bool>(nameof(IsTextTrimmed), v => v.isTextTrimmed);
        /// <summary>
        /// Property of <see cref="ShowToolTipWhenTextTrimmed"/>.
        /// </summary>
        public static readonly AvaloniaProperty<bool> ShowToolTipWhenTextTrimmedProperty = AvaloniaProperty.Register<TextBlock, bool>(nameof(ShowToolTipWhenTextTrimmed), true);


        // Constants.
        const int MaxToolTipLength = 1024;


        // Static fields.
        static readonly Regex NewLineRegex = new Regex("\n");


        // Fields.
        bool isMultiLineText;
        bool isTextTrimmed;
        readonly List<(int, int)> textLineRanges = new List<(int, int)>();
        readonly ScheduledAction updateToolTipAction;
        Window? window;


        /// <summary>
        /// Initialize new <see cref="TextBlock"/> instance.
        /// </summary>
        public TextBlock()
        {
            this.GetObservable(IsTextTrimmedProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
            this.GetObservable(ShowToolTipWhenTextTrimmedProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
            this.GetObservable(TextProperty).Subscribe(text => 
            {
                this.textLineRanges.Clear();
                if (!string.IsNullOrEmpty(text))
                {
                    var start = 0;
                    var textLength = text.Length;
                    var match = NewLineRegex.Match(text);
                    while (match.Success)
                    {
                        this.textLineRanges.Add((start, match.Index));
                        start = match.Index + match.Length;
                        match = match.NextMatch();
                    }
                    if (start < textLength)
                        this.textLineRanges.Add((start, textLength));
                }
                this.updateToolTipAction?.Schedule();
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
                    || !this.GetValue<bool>(ShowToolTipWhenTextTrimmedProperty))
                {
                    this.ClearValue(ToolTip.TipProperty);
                }
                else
                {
                    var text = this.Text;
                    if (string.IsNullOrEmpty(text))
                        this.ClearValue(ToolTip.TipProperty);
                    else if (text.Length <= MaxToolTipLength)
                        this.SetValue<object?>(ToolTip.TipProperty, text);
                    else
                        this.SetValue<object?>(ToolTip.TipProperty, $"{text.Substring(0, MaxToolTipLength - 1)}…");
                }
            });
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
                var text = this.GetValue<string?>(TextProperty);
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
                                var minSize = base.MeasureOverride(new Size(availableSize.Width + this.FontSize * 2, availableSize.Height));
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
        }


        /// <inheritdoc/>
        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            this.window = null;
            base.OnDetachedFromLogicalTree(e);
        }


        /// <summary>
        /// Get or set whether tooltip is needed to be shown if text inside the control has been trimmed or not.
        /// </summary>
        public bool ShowToolTipWhenTextTrimmed
        {
            get => this.GetValue<bool>(ShowToolTipWhenTextTrimmedProperty);
            set => this.SetValue<bool>(ShowToolTipWhenTextTrimmedProperty, value);
        }


        // Interface implementation.
        Type IStyleable.StyleKey { get; } = typeof(Avalonia.Controls.TextBlock);
    }
}
