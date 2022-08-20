using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using CarinaStudio.Threading;
using System;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// <see cref="TextBox"/> which treat input text as object with type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of object.</typeparam>
    public abstract class ObjectTextBox<T> : TextBox, IStyleable where T : class
	{
		/// <summary>
		/// Property of <see cref="IsTextValid"/>.
		/// </summary>
		public static readonly DirectProperty<ObjectTextBox<T>, bool> IsTextValidProperty = AvaloniaProperty.RegisterDirect<ObjectTextBox<T>, bool>(nameof(IsTextValid), o => o.isTextValid);
		/// <summary>
		/// Property of <see cref="Object"/>.
		/// </summary>
		public static readonly StyledProperty<T?> ObjectProperty = AvaloniaProperty.Register<ObjectTextBox<T>, T?>(nameof(Object), null);
		/// <summary>
		/// Property of <see cref="ValidationDelay"/>.
		/// </summary>
		public static readonly StyledProperty<int> ValidationDelayProperty = AvaloniaProperty.Register<ObjectTextBox<T>, int>(nameof(ValidationDelay), 500, coerce: (_, it) => Math.Max(0, it));


		// Fields.
		bool isTextValid = true;
		readonly ScheduledAction validateAction;


		/// <summary>
		/// Initialize new <see cref="ObjectTextBox{T}"/> instance.
		/// </summary>
		protected ObjectTextBox()
		{
			this.validateAction = new ScheduledAction(() => this.Validate());
		}


		/// <summary>
		/// Check equality of objects.
		/// </summary>
		/// <param name="x">First object.</param>
		/// <param name="y">Second object.</param>
		/// <returns>True if two objects are equalvant.</returns>
		protected virtual bool CheckObjectEquality(T? x, T? y) => x?.Equals(y) ?? y == null;


		/// <summary>
		/// Convert object to text.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <returns>Converted text.</returns>
		protected virtual string? ConvertToText(T obj) => obj.ToString();


		/// <summary>
		/// Get whether input <see cref="TextBox.Text"/> represent a valid object or not.
		/// </summary>
		public bool IsTextValid { get => this.isTextValid; }


		/// <summary>
		/// Check whether text validation is scheduled or not.
		/// </summary>
		protected bool IsValidationScheduled { get => this.validateAction.IsScheduled; }


		/// <summary>
		/// Get or set object.
		/// </summary>
		public T? Object
		{
			get => this.GetValue<T?>(ObjectProperty);
			set
			{
				this.validateAction.ExecuteIfScheduled();
				this.SetValue<T?>(ObjectProperty, value);
			}
		}


		/// <inheritdoc/>
		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			var property = change.Property;
			if (property == TextProperty)
			{
				if (string.IsNullOrEmpty(this.Text))
					this.validateAction.Reschedule();
				else
					this.validateAction.Reschedule(this.ValidationDelay);
			}
			else if (property == ObjectProperty)
			{
				var obj = (change.NewValue as T);
				if (obj != null)
				{
					if (!this.Validate(false, out var currentObj) || !this.CheckObjectEquality(currentObj, obj))
					{
						var fromEmptyString = string.IsNullOrEmpty(this.Text);
						this.Text = this.ConvertToText(obj);
						this.validateAction.ExecuteIfScheduled();
						if (this.IsFocused && fromEmptyString && !string.IsNullOrEmpty(this.Text))
							this.SelectAll();
					}
				}
				else if (this.Text != null)
					this.Text = "";
			}
			else if (property == ValidationDelayProperty)
			{
				if (this.validateAction.IsScheduled)
					this.validateAction.Reschedule(this.ValidationDelay);
			}
		}


		/// <inheritdoc/>
		protected override void OnTextInput(TextInputEventArgs e)
		{
			if (string.IsNullOrEmpty(this.Text))
			{
				var text = e.Text;
				if (text != null && text.Length > 0 && char.IsWhiteSpace(text[0]))
					e.Handled = true;
			}
			base.OnTextInput(e);
		}


		/// <summary>
		/// Try converting text to object.
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="obj">Converted object.</param>
		/// <returns>True if conversion succeeded.</returns>
		protected abstract bool TryConvertToObject(string text, out T? obj);


		/// <summary>
		/// Validate input <see cref="TextBox.Text"/> and generate corresponding object.
		/// </summary>
		/// <returns>True if input <see cref="TextBox.Text"/> generates a valid object.</returns>
		public bool Validate() =>
			this.Validate(true, out var value);


		// Validate text.
		bool Validate(bool updateObjectAndText, out T? obj)
		{
			// check state
			this.VerifyAccess();

			// cancel scheduled validation
			if (updateObjectAndText)
				this.validateAction.Cancel();

			// trim spaces
			var text = this.Text ?? "";
			var trimmedText = text.Trim();
			if (text != trimmedText)
			{
				text = trimmedText;
				if (updateObjectAndText)
				{
					this.Text = trimmedText;
					this.validateAction.Cancel();
				}
			}

			// clear object
			if (text.Length == 0)
			{
				if (updateObjectAndText)
				{
					this.SetValue<T?>(ObjectProperty, null);
					this.SetAndRaise<bool>(IsTextValidProperty, ref this.isTextValid, true);
					this.PseudoClasses.Set(":invalidObjectTextBoxText", false);
				}
				obj = null;
				return true;
			}

			// try convert to object
			if (!this.TryConvertToObject(text, out obj) || obj == null)
			{
				if (updateObjectAndText)
				{
					this.SetAndRaise<bool>(IsTextValidProperty, ref this.isTextValid, false);
					this.PseudoClasses.Set(":invalidObjectTextBoxText", true);
				}
				return false;
			}

			// complete
			if (updateObjectAndText)
			{
				if (!this.CheckObjectEquality(obj, this.Object))
					this.SetValue<T?>(ObjectProperty, obj);
				this.SetAndRaise<bool>(IsTextValidProperty, ref this.isTextValid, true);
				this.PseudoClasses.Set(":invalidObjectTextBoxText", false);
			}
			return true;
		}


		/// <summary>
		/// Get or set the delay of validating text after user typing in milliseconds.
		/// </summary>
		public int ValidationDelay
		{
			get => this.GetValue<int>(ValidationDelayProperty);
			set => this.SetValue<int>(ValidationDelayProperty, value);
		}


		// Interface implementations.
		Type IStyleable.StyleKey => typeof(TextBox);
	}
}
