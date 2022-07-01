using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Windows.Input;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// <see cref="TextBlock"/> which supports open the link.
    /// </summary>
    public class LinkTextBlock : Avalonia.Controls.TextBlock, IStyleable
    {
        /// <summary>
        /// Property of <see cref="Command"/>.
        /// </summary>
        public static readonly AvaloniaProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<LinkTextBlock, ICommand?>(nameof(Command));
        /// <summary>
        /// Property of <see cref="CommandParameter"/>.
        /// </summary>
        public static readonly AvaloniaProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<LinkTextBlock, object?>(nameof(CommandParameter));
        /// <summary>
        /// Property of <see cref="Uri"/>.
        /// </summary>
        public static readonly AvaloniaProperty<Uri?> UriProperty = AvaloniaProperty.Register<LinkTextBlock, Uri?>(nameof(Uri));


        /// <summary>
        /// Initialize new <see cref="LinkTextBlock"/> instance.
        /// </summary>
        public LinkTextBlock()
        { }


        // Perform action for click.
        void Click()
        {
            ToolTip.SetIsOpen(this, false);
            var command = this.Command;
            if (command != null)
            {
                var parameter = this.GetValue<object?>(CommandParameterProperty);
                if (command.CanExecute(parameter))
                    command.Execute(parameter);
            }
            else
                this.Uri?.Let(it => Platform.OpenLink(it));
        }


        /// <summary>
        /// Get or set command to execute when clicking the link.
        /// </summary>
        public ICommand? Command
        {
            get => this.GetValue<ICommand?>(CommandProperty);
            set => this.SetValue<ICommand?>(CommandProperty, value);
        }


        /// <summary>
        /// Get or set parameter to execute <see cref="Command"/>.
        /// </summary>
        public object? CommandParameter
        {
            get => this.GetValue<object?>(CommandParameterProperty);
            set => this.SetValue<object?>(CommandParameterProperty, value);
        }


        /// <inheritdoc/>
        protected override bool IsEnabledCore
        {
            get
            {
                var command = this.GetValue<ICommand?>(CommandProperty);
                if (command != null)
                {
                    if (!command.CanExecute(this.GetValue<object?>(CommandParameterProperty)))
                        return false;
                }
                else if (this.GetValue<Uri?>(UriProperty) == null)
                    return false;
                return base.IsEnabledCore;
            }
        }
        

        // Called when CanExecute() of command changed.
        void OnCommandCanExecuteChanged(object? sender, EventArgs e) =>
            this.UpdateIsEffectivelyEnabled();


        /// <inheritdoc/>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.Click();
            base.OnKeyUp(e);
        }


        /// <summary>
        /// Called when pointer released.
        /// </summary>
        /// <param name="e">Event data.</param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (e.InitialPressMouseButton == MouseButton.Left && this.IsPointerOver)
                this.Click();
        }


        /// <summary>
        /// Called when property changed.
        /// </summary>
        /// <typeparam name="T">Type of property.</typeparam>
        /// <param name="change">Change data.</param>
        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == CommandProperty)
            {
                (change.OldValue.Value as ICommand)?.Let(it => it.CanExecuteChanged -= this.OnCommandCanExecuteChanged);
                (change.NewValue.Value as ICommand)?.Let(it =>
                {
                    it.CanExecuteChanged += this.OnCommandCanExecuteChanged;
                    this.Uri = null;
                });
                this.UpdateIsEffectivelyEnabled();
            }
            else if (change.Property == ToolTip.IsOpenProperty)
            {
                if (Platform.IsMacOS && this.FindAncestorOfType<Avalonia.Controls.Window>()?.IsActive == false)
                    ToolTip.SetIsOpen(this, false);
            }
            else if (change.Property == UriProperty)
            {
                if (change.NewValue.Value != null)
                {
                    this.Command = null;
                    this.CommandParameter = null;
                }
                this.UpdateIsEffectivelyEnabled();
            }
        }


        /// <summary>
        /// Get or set URI to open.
        /// </summary>
        public Uri? Uri
        {
            get => this.GetValue<Uri?>(UriProperty);
            set => this.SetValue<Uri?>(UriProperty, value);
        }


        // Interface implementation.
        Type IStyleable.StyleKey { get; } = typeof(LinkTextBlock);
    }
}
