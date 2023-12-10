using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media;
using CarinaStudio.Collections;
using CarinaStudio.Media.TextFormatting;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// Extended <see cref="Avalonia.Controls.SelectableTextBlock"/>.
    /// </summary>
    public class SelectableTextBlock : Avalonia.Controls.SelectableTextBlock
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
        /// <summary>
        /// Property of <see cref="ToolTipTemplate"/>.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> ToolTipTemplateProperty = AvaloniaProperty.Register<SelectableTextBlock, IDataTemplate?>(nameof(ToolTipTemplate));


        // Constants.
        const int MaxToolTipLength = 1024;


        // Fields.
        IDisposable? isWindowActiveObserverToken;
        bool isMultiLineText;
        bool isTextTrimmed;
        IDisposable? toolTipBindingToken;
        readonly ScheduledAction updateToolTipAction;
        int updateToolTipFailureCount;
        Avalonia.Controls.Window? window;


        /// <summary>
        /// Initialize new <see cref="SelectableTextBlock"/> instance.
        /// </summary>
        public SelectableTextBlock()
        {
            this.GetObservable(IsTextTrimmedProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
            this.GetObservable(ShowToolTipWhenTextTrimmedProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
            this.GetObservable(TextProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
            this.GetObservable(ToolTipTemplateProperty).Subscribe(_ => this.updateToolTipAction?.Schedule());
            this.TextTrimming = TextTrimming.CharacterEllipsis;
            this.updateToolTipAction = new ScheduledAction(() =>
            {
                this.toolTipBindingToken = this.toolTipBindingToken.DisposeAndReturnNull();
                try
                {
                    if (this.isTextTrimmed
                        && this.GetValue(ShowToolTipWhenTextTrimmedProperty)
                        && (Platform.IsNotMacOS || this.window?.IsActive == true))
                    {
                        var inlines = this.Inlines;
                        var text = inlines.IsNotEmpty() ? inlines.Text : this.Text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            var toolTipText = text.Length <= MaxToolTipLength
                                ? text
                                : $"{text[..MaxToolTipLength]}…";
                            var toolTip = this.GetValue(ToolTipTemplateProperty)?.Build(toolTipText)?.Also(control =>
                            {
                                control.DataContext = toolTipText;
                            }) ?? (object)toolTipText;
                            this.toolTipBindingToken = this.Bind(ToolTip.TipProperty, new Binding { Source = toolTip, Priority = BindingPriority.Template });
                        }
                    }
                    this.updateToolTipFailureCount = 0;
                }
                catch (Exception ex)
                {
                    ++this.updateToolTipFailureCount;
                    this.DebugLogger?.LogWarning(ex, "Failed to update tool tip, failure count: {count}", this.updateToolTipFailureCount);
                    if (this.updateToolTipFailureCount <= 5)
                        this.updateToolTipAction!.Schedule(500);
                }
            });
        }
        
        
        /// <summary>
        /// Get or set logger for debugging purpose.
        /// </summary>
        public ILogger? DebugLogger { get; set; }


        /// <summary>
        /// Check whether text inside the <see cref="TextBlock"/> has multiple lines or not.
        /// </summary>
        public bool IsMultiLineText => this.isMultiLineText;


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
                this.SetAndRaise(IsMultiLineTextProperty, ref this.isMultiLineText, lineCount > 1);

                // check trimming
                this.IsTextTrimmed = textLayout.IsTextTrimmed();
            }
            return measuredSize;
        }


        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            this.window = TopLevel.GetTopLevel(this) as Avalonia.Controls.Window;
            if (Platform.IsMacOS)
            {
                this.isWindowActiveObserverToken = this.window?.GetObservable(Window.IsActiveProperty).Subscribe(_ => 
                    this.updateToolTipAction.Schedule());
                this.updateToolTipAction.Schedule();
            }
        }


        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            this.isWindowActiveObserverToken = this.isWindowActiveObserverToken.DisposeAndReturnNull();
            this.window = null;
            base.OnDetachedFromVisualTree(e);
        }


        /// <summary>
        /// Get or set whether tooltip is needed to be shown if text inside the control has been trimmed or not.
        /// </summary>
        public bool ShowToolTipWhenTextTrimmed
        {
            get => this.GetValue(ShowToolTipWhenTextTrimmedProperty);
            set => this.SetValue(ShowToolTipWhenTextTrimmedProperty, value);
        }

        
        /// <inheritdoc/>
        protected override Type StyleKeyOverride => typeof(Avalonia.Controls.SelectableTextBlock);


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
