using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CarinaStudio.Threading;
using System;

namespace CarinaStudio.Controls;

/// <summary>
/// <see cref="TextBox"/> which treat input text as object with specific type.
/// </summary>
public abstract class ObjectTextBox : TextBox
{
	/// <summary>
	/// Property of <see cref="IsTextValid"/>.
	/// </summary>
	public static readonly DirectProperty<ObjectTextBox, bool> IsTextValidProperty = AvaloniaProperty.RegisterDirect<ObjectTextBox, bool>(nameof(IsTextValid), o => o.isTextValid);
	/// <summary>
	/// Property of <see cref="Object"/>.
	/// </summary>
	public static readonly StyledProperty<object?> ObjectProperty = AvaloniaProperty.Register<ObjectTextBox, object?>(nameof(Object), null);
	/// <summary>
	/// Property of <see cref="ValidationDelay"/>.
	/// </summary>
	public static readonly StyledProperty<int> ValidationDelayProperty = AvaloniaProperty.Register<ObjectTextBox, int>(nameof(ValidationDelay), 500, coerce: (_, it) => Math.Max(0, it));
	
	
	// Fields.
	bool isTextValid = true;
	readonly ScheduledAction validateAction;
	
	
	/// <summary>
	/// Initialize new <see cref="ObjectTextBox"/> instance.
	/// </summary>
	internal ObjectTextBox()
	{
		this.validateAction = new ScheduledAction(() => this.Validate());
	}
	
	
	/// <summary>
	/// Check equality of objects.
	/// </summary>
	/// <param name="x">First object.</param>
	/// <param name="y">Second object.</param>
	/// <returns>True if two objects are equivalent.</returns>
	protected virtual bool CheckObjectEquality(object? x, object? y) => x?.Equals(y) ?? y == null;


	/// <summary>
	/// Convert object to text.
	/// </summary>
	/// <param name="obj">Object.</param>
	/// <returns>Converted text.</returns>
	protected virtual string? ConvertToText(object obj) => obj.ToString();


	/// <summary>
	/// Get whether input <see cref="TextBox.Text"/> represent a valid object or not.
	/// </summary>
	public bool IsTextValid => this.isTextValid;


	/// <summary>
	/// Check whether text validation is scheduled or not.
	/// </summary>
	protected bool IsValidationScheduled => this.validateAction.IsScheduled;


	/// <summary>
	/// Get or set object.
	/// </summary>
	public object? Object
	{
		get => this.GetValue(ObjectProperty);
		set
		{
			this.validateAction.ExecuteIfScheduled();
			this.SetValue(ObjectProperty, value);
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
			var obj = change.NewValue;
			if (obj is not null)
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
			if (!string.IsNullOrEmpty(text) && char.IsWhiteSpace(text[0]))
				e.Handled = true;
		}
		base.OnTextInput(e);
	}
	
	
	/// <inheritdoc/>
	protected override Type StyleKeyOverride => typeof(TextBox);


	/// <summary>
	/// Try converting text to object.
	/// </summary>
	/// <param name="text">Text.</param>
	/// <param name="obj">Converted object.</param>
	/// <returns>True if conversion succeeded.</returns>
	protected abstract bool TryConvertToObject(string text, out object? obj);


	/// <summary>
	/// Validate input <see cref="TextBox.Text"/> and generate corresponding object.
	/// </summary>
	/// <returns>True if input <see cref="TextBox.Text"/> generates a valid object.</returns>
	public bool Validate() =>
		this.Validate(true, out var _);


	// Validate text.
	bool Validate(bool updateObjectAndText, out object? obj)
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
				this.SetValue(ObjectProperty, null);
				this.SetAndRaise(IsTextValidProperty, ref this.isTextValid, true);
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
				this.SetAndRaise(IsTextValidProperty, ref this.isTextValid, false);
				this.PseudoClasses.Set(":invalidObjectTextBoxText", true);
			}
			return false;
		}

		// complete
		if (updateObjectAndText)
		{
			if (!this.CheckObjectEquality(obj, this.Object))
				this.SetValue(ObjectProperty, obj);
			this.SetAndRaise(IsTextValidProperty, ref this.isTextValid, true);
			this.PseudoClasses.Set(":invalidObjectTextBoxText", false);
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
}


/// <summary>
/// <see cref="TextBox"/> which treat input text as object with type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Type of object.</typeparam>
public abstract class ObjectTextBox<T> : ObjectTextBox where T : class
{
	/// <summary>
	/// Initialize new <see cref="ObjectTextBox{T}"/> instance.
	/// </summary>
	protected ObjectTextBox()
	{ }


	/// <inheritdoc/>
	protected override sealed bool CheckObjectEquality(object? x, object? y) =>
		this.CheckObjectEquality(x as T, y as T);


	/// <summary>
	/// Check equality of objects.
	/// </summary>
	/// <param name="x">First object.</param>
	/// <param name="y">Second object.</param>
	/// <returns>True if two objects are equivalent.</returns>
	protected virtual bool CheckObjectEquality(T? x, T? y) => x?.Equals(y) ?? y == null;


	/// <inheritdoc/>
	protected override sealed string? ConvertToText(object obj) =>
		obj is T t ? this.ConvertToText(t) : null;
	

	/// <summary>
	/// Convert object to text.
	/// </summary>
	/// <param name="obj">Object.</param>
	/// <returns>Converted text.</returns>
	protected virtual string? ConvertToText(T obj) => obj.ToString();


	/// <summary>
	/// Get or set object.
	/// </summary>
	public new abstract T? Object { get; set; }


	/// <inheritdoc/>
	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		if (change.Property == ObjectProperty)
			this.RaiseObjectChanged((T?)change.OldValue, (T?)change.NewValue);
		base.OnPropertyChanged(change);
	}


	/// <summary>
	/// Raise property changed event of <see cref="Object"/>.
	/// </summary>
	/// <param name="oldValue">Old value.</param>
	/// <param name="newValue">New value.</param>
	protected abstract void RaiseObjectChanged(T? oldValue, T? newValue);


	/// <inheritdoc/>
	protected override sealed bool TryConvertToObject(string text, out object? obj)
	{
		if (this.TryConvertToObject(text, out var t))
		{
			obj = t;
			return true;
		}
		obj = null;
		return false;
	}


	/// <summary>
	/// Try converting text to object.
	/// </summary>
	/// <param name="text">Text.</param>
	/// <param name="obj">Converted object.</param>
	/// <returns>True if conversion succeeded.</returns>
	protected abstract bool TryConvertToObject(string text, out T? obj);
}
