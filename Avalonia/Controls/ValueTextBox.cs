using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using CarinaStudio.Threading;
using System;
using Avalonia.Interactivity;

namespace CarinaStudio.Controls
{
    /// <summary>
    /// <see cref="TextBox"/> which treat input text as given value with type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of object.</typeparam>
    public abstract class ValueTextBox<T> : TextBox, IStyleable where T : struct
	{
		/// <summary>
		/// Property of <see cref="CoerceValueWhenLostFocus"/>.
		/// </summary>
		public static readonly StyledProperty<bool> CoerceValueWhenLostFocusProperty = AvaloniaProperty.Register<ValueTextBox<T>, bool>(nameof(CoerceValueWhenLostFocus), true);
		/// <summary>
		/// Property of <see cref="DefaultValue"/>.
		/// </summary>
		public static readonly StyledProperty<T> DefaultValueProperty = AvaloniaProperty.Register<ValueTextBox<T>, T>(nameof(DefaultValue));
		/// <summary>
		/// Property of <see cref="IsNullValueAllowed"/>.
		/// </summary>
		public static readonly StyledProperty<bool> IsNullValueAllowedProperty = AvaloniaProperty.Register<ValueTextBox<T>, bool>(nameof(IsNullValueAllowed), true);
		/// <summary>
		/// Property of <see cref="IsTextValid"/>.
		/// </summary>
		public static readonly DirectProperty<ValueTextBox<T>, bool> IsTextValidProperty = AvaloniaProperty.RegisterDirect<ValueTextBox<T>, bool>(nameof(IsTextValid), o => o.isTextValid);
		/// <summary>
		/// Property of <see cref="ValidationDelay"/>.
		/// </summary>
		public static readonly StyledProperty<int> ValidationDelayProperty = AvaloniaProperty.Register<ValueTextBox<T>, int>(nameof(ValidationDelay), 500, coerce: (_, it) => Math.Max(0, it));
		/// <summary>
		/// Property of <see cref="Value"/>.
		/// </summary>
		public static readonly StyledProperty<T?> ValueProperty = AvaloniaProperty.Register<ValueTextBox<T>, T?>(nameof(Value), null);


		// Fields.
		bool isTextValid = true;
		T? lastValidValue;
		readonly ScheduledAction validateAction;


		/// <summary>
		/// Initialize new <see cref="ValueTextBox{T}"/> instance.
		/// </summary>
		protected ValueTextBox()
		{
			if (!IsNullValueAllowedProperty.GetDefaultValue(this.GetType()))
				this.lastValidValue = this.GetValue(DefaultValueProperty);
			this.validateAction = new ScheduledAction(() => this.Validate());
		}


		/// <summary>
		/// Check equality of values.
		/// </summary>
		/// <param name="x">First value.</param>
		/// <param name="y">Second value.</param>
		/// <returns>True if two values are equivalent.</returns>
		protected virtual bool CheckValueEquality(T? x, T? y) => x?.Equals(y) ?? y == null;


		/// <summary>
		/// Coerce the set value.
		/// </summary>
		/// <param name="value">Set value.</param>
		/// <returns>Coerced value.</returns>
		protected virtual T CoerceValue(T value) => value;


		/// <summary>
		/// Get or set whether value should be coerced when the control lost its focus or not.
		/// </summary>
		public bool CoerceValueWhenLostFocus
		{
			get => this.GetValue(CoerceValueWhenLostFocusProperty);
			set => this.SetValue(CoerceValueWhenLostFocusProperty, value);
		}


		/// <summary>
		/// Convert value to text.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <returns>Converted text.</returns>
		protected virtual string? ConvertToText(T value) => value.ToString();


		/// <summary>
		/// Get of set default value for <see cref="IsNullValueAllowed"/> is False and <see cref="Avalonia.Controls.TextBox.Text"/> is empty.
		/// </summary>
		public T DefaultValue 
		{
			 get => this.GetValue(DefaultValueProperty);
			 set => this.SetValue(DefaultValueProperty, value);
		}


		/// <summary>
		/// Get or set whether <see cref="Value"/> can be Null or not.
		/// </summary>
		public bool IsNullValueAllowed
		{
			get => this.GetValue(IsNullValueAllowedProperty);
			set => this.SetValue(IsNullValueAllowedProperty, value);
		}


		/// <summary>
		/// Get whether input <see cref="TextBox.Text"/> represent a valid value or not.
		/// </summary>
		public bool IsTextValid => this.isTextValid;


		/// <inheritdoc/>
		protected override void OnLostFocus(RoutedEventArgs e)
		{
			if (this.GetValue(CoerceValueWhenLostFocusProperty))
			{
				this.validateAction.ExecuteIfScheduled();
				if (!this.isTextValid)
				{
					if (this.CheckValueEquality(this.GetValue(ValueProperty), this.lastValidValue))
						this.OnValueChanged(this.lastValidValue);
					else
						this.SetValue(ValueProperty, this.lastValidValue);
				}
			}
			base.OnLostFocus(e);
		}


		/// <inheritdoc/>
		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			var property = change.Property;
			if (property == CoerceValueWhenLostFocusProperty)
			{
				if ((bool)change.NewValue! && !this.IsFocused)
				{
					this.validateAction.ExecuteIfScheduled();
					if (!this.isTextValid)
					{
						if (this.CheckValueEquality(this.GetValue(ValueProperty), this.lastValidValue))
							this.OnValueChanged(this.lastValidValue);
						else
							this.SetValue(ValueProperty, this.lastValidValue);
					}
				}
			}
			else if (property == DefaultValueProperty)
				this.Validate();
			else if (property == IsNullValueAllowedProperty)
			{
				if (!(bool)change.NewValue.AsNonNull())
				{
					if (!this.validateAction.ExecuteIfScheduled() && this.Value == null)
						this.ResetToDefaultValue();
				}
			}
			else if (property == TextProperty)
			{
				if (string.IsNullOrEmpty(this.Text))
					this.validateAction.Reschedule();
				else
					this.validateAction.Reschedule(this.ValidationDelay);
			}
			else if (property == ValidationDelayProperty)
			{
				if (this.validateAction.IsScheduled)
					this.validateAction.Reschedule(this.ValidationDelay);
			}
			else if (property == ValueProperty)
			{
				var value = (T?)change.NewValue;
				if (value is null && !this.IsNullValueAllowed)
					value = this.GetValue(DefaultValueProperty);
				this.OnValueChanged(value);
			}
		}


		/// <inheritdoc/>
		protected override void OnTextInput(TextInputEventArgs e)
		{
			if (string.IsNullOrEmpty(this.Text))
			{
				var text = e.Text;
				if (!string.IsNullOrEmpty(text) && char.IsWhiteSpace(text[0]))
					e.Handled = true;
			}
			base.OnTextInput(e);
		}


		/// <summary>
		/// Called when value changed.
		/// </summary>
		/// <param name="value">New value.</param>
		protected virtual void OnValueChanged(T? value)
		{
			if (value is not null)
			{
				if (!this.Validate(false, out var currentValue) || !this.CheckValueEquality(currentValue, value))
				{
					var fromEmptyString = string.IsNullOrEmpty(this.Text);
					this.Text = this.ConvertToText(value.Value);
					this.validateAction.ExecuteIfScheduled();
					if (this.IsFocused && fromEmptyString && !string.IsNullOrEmpty(this.Text))
						this.SelectAll();
				}
			}
			else if (!string.IsNullOrWhiteSpace(this.Text))
				this.Text = "";
		}


		// Reset to default value.
		void ResetToDefaultValue()
		{
			var value = this.GetValue(DefaultValueProperty);
			this.lastValidValue = value;
			this.SetValue(ValueProperty, value);
			this.Text = this.ConvertToText(value);
			this.SetAndRaise(IsTextValidProperty, ref this.isTextValid, true);
			this.PseudoClasses.Set(":invalidValueTextBoxText", false);
			this.validateAction.Cancel();
			if (this.IsFocused)
				this.SelectAll();
		}

		/// <summary>
		/// Try converting text to value.
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="value">Converted value.</param>
		/// <returns>True if conversion succeeded.</returns>
		protected abstract bool TryConvertToValue(string text, out T? value);


		/// <summary>
		/// Validate input <see cref="TextBox.Text"/> and generate corresponding value.
		/// </summary>
		/// <returns>True if input <see cref="TextBox.Text"/> generates a valid value.</returns>
		public bool Validate() =>
			this.Validate(true, out _);


		// Validate text.
		bool Validate(bool updateValueAndText, out T? value)
		{
			// check state
			this.VerifyAccess();

			// cancel scheduled validation
			if (updateValueAndText)
				this.validateAction.Cancel();

			// trim spaces
			var text = this.Text ?? "";
			var trimmedText = text.Trim();
			if (text != trimmedText)
			{
				text = trimmedText;
				if (updateValueAndText)
				{
					this.Text = trimmedText;
					this.validateAction.Cancel();
				}
			}

			// clear value
			if (text.Length == 0)
			{
				if (this.IsNullValueAllowed)
				{
					value = null;
					if (updateValueAndText)
					{
						this.lastValidValue = null;
						this.SetValue(ValueProperty, null);
						this.SetAndRaise(IsTextValidProperty, ref this.isTextValid, true);
						this.PseudoClasses.Set(":invalidValueTextBoxText", false);
					}
				}
				else
				{
					value = this.GetValue(DefaultValueProperty);
					if (updateValueAndText)
						this.ResetToDefaultValue();
				}
				return true;
			}

			// try convert to object
			if (!this.TryConvertToValue(text, out value) || value is null)
			{
				if (updateValueAndText)
				{
					this.SetAndRaise(IsTextValidProperty, ref this.isTextValid, false);
					this.PseudoClasses.Set(":invalidValueTextBoxText", true);
				}
				return false;
			}

			// complete
			value = this.CoerceValue(value.Value);
			if (updateValueAndText)
			{
				this.lastValidValue = value;
				if (!this.CheckValueEquality(value, this.Value))
					this.SetValue(ValueProperty, value);
				this.SetAndRaise(IsTextValidProperty, ref this.isTextValid, true);
				this.PseudoClasses.Set(":invalidValueTextBoxText", false);
			}
			return true;
		}


		/// <summary>
		/// Get or set the delay of validating text after user typing in milliseconds.
		/// </summary>
		public int ValidationDelay
		{
			get => this.GetValue(ValidationDelayProperty);
			set => this.SetValue(ValidationDelayProperty, value);
		}


		/// <summary>
		/// Get or set value.
		/// </summary>
		public T? Value
		{
			get => this.GetValue(ValueProperty);
			set
			{
				this.validateAction.ExecuteIfScheduled();
				if (value is not null)
				{
					this.lastValidValue = this.CoerceValue(value.Value);
					value = this.lastValidValue;
				}
				else if (!this.IsNullValueAllowed)
				{
					this.lastValidValue = this.GetValue(DefaultValueProperty);
					value = this.lastValidValue;
				}
				else
					this.lastValidValue = null;
				this.SetValue(ValueProperty, value);
			}
		}


		// Interface implementations.
		Type IStyleable.StyleKey => typeof(TextBox);
	}
}
