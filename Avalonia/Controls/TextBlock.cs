using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using CarinaStudio.Collections;
using CarinaStudio.Threading;
using System;

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
        public static readonly DirectProperty<TextBlock, bool> IsMultiLineTextProperty = AvaloniaProperty.RegisterDirect<TextBlock, bool>(nameof(IsMultiLineText), v => v.isMultiLineText);
        /// <summary>
        /// Property of <see cref="IsTextTrimmed"/>.
        /// </summary>
        public static readonly DirectProperty<TextBlock, bool> IsTextTrimmedProperty = AvaloniaProperty.RegisterDirect<TextBlock, bool>(nameof(IsTextTrimmed), v => v.isTextTrimmed);
        /// <summary>
        /// Property of <see cref="ShowToolTipWhenTextTrimmed"/>.
        /// </summary>
        public static readonly StyledProperty<bool> ShowToolTipWhenTextTrimmedProperty = AvaloniaProperty.Register<TextBlock, bool>(nameof(ShowToolTipWhenTextTrimmed), true);
        /// <summary>
        /// Property of <see cref="ToolTipTemplate"/>.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> ToolTipTemplateProperty = AvaloniaProperty.Register<TextBlock, IDataTemplate?>(nameof(ToolTipTemplate));


        // Constants.
        const int MaxToolTipLength = 1024;


        // Fields.
        IDisposable? isWindowActiveObserverToken;
        bool isMultiLineText;
        bool isTextTrimmed;
        readonly ScheduledAction updateToolTipAction;
        Window? window;


        /// <summary>
        /// Initialize new <see cref="TextBlock"/> instance.
        /// </summary>
        public TextBlock()
        {
            this.GetObservable(IsTextTrimmedProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
            this.GetObservable(ShowToolTipWhenTextTrimmedProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
            this.GetObservable(TextProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
            this.GetObservable(ToolTipTemplateProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
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
                    else
                    {
                        var toolTipText = text.Length <= MaxToolTipLength
                            ? text
                            : $"{text[0..MaxToolTipLength]}…";
                        var toolTip = this.GetValue(ToolTipTemplateProperty)?.Build(toolTipText)?.Also(control =>
                        {
                            control.DataContext = toolTipText;
                        }) ?? (object)toolTipText;
                        this.SetValue(ToolTip.TipProperty, toolTip);
                    }
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
        public bool IsTextTrimmed 
        { 
            get => this.isTextTrimmed;
            protected set => this.SetAndRaise(IsTextTrimmedProperty, ref this.isTextTrimmed, value); 
        }


        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            var measuredSize = base.MeasureOverride(availableSize);
            if (double.IsFinite(availableSize.Width) || double.IsFinite(availableSize.Height))
            {
                // check multi line
                var textLayout = this.TextLayout;
                var lineCount = textLayout.TextLines.Count;
                this.SetAndRaise<bool>(IsMultiLineTextProperty, ref this.isMultiLineText, lineCount > 1);

                // check trimming
                var isTextTrimmed = false;
                for (var i = lineCount - 1; i >= 0; --i)
                {
                    if (textLayout.TextLines[i].HasCollapsed)
                    {
                        isTextTrimmed = true;
                        break;
                    }
                }
                this.IsTextTrimmed = isTextTrimmed;
            }
            return measuredSize;
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


        /// <summary>
        /// Get or set template for building tooltip.
        /// </summary>
        public IDataTemplate? ToolTipTemplate
        {
            get => this.GetValue(ToolTipTemplateProperty);
            set => this.SetValue(ToolTipTemplateProperty, value);
        }
    }
}
