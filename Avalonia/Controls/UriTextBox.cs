using Avalonia;
using Avalonia.Controls;
using System;
using System.Text.RegularExpressions;
using Avalonia.Input;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// <see cref="TextBox"/> which treat input text as <see cref="Uri"/>.
	/// </summary>
	public class UriTextBox : ObjectTextBox<Uri>
	{
		/// <summary>
		/// Property of <see cref="DefaultUriScheme"/>.
		/// </summary>
		public static readonly StyledProperty<string?> DefaultUriSchemeProperty = AvaloniaProperty.Register<UriTextBox, string?>(nameof(DefaultUriScheme), null);
		/// <summary>
		/// Property of <see cref="ObjectTextBox{Uri}.Object"/>.
		/// </summary>
		public static new readonly DirectProperty<UriTextBox, Uri?> ObjectProperty = AvaloniaProperty.RegisterDirect<UriTextBox, Uri?>(nameof(Object), t => t.Object, (t, o) => t.Object = o);
		/// <summary>
		/// Property of <see cref="UriKind"/>.
		/// </summary>
		public static readonly StyledProperty<UriKind> UriKindProperty = AvaloniaProperty.Register<UriTextBox, UriKind>(nameof(IsTextValid), UriKind.Absolute);


		// Static fields.
		static readonly Regex UriSchemeRegex = new("^[\\w]+\\://");


		/// <summary>
		/// Initialize new <see cref="UriTextBox"/> instance.
		/// </summary>
		public UriTextBox()
		{
			this.AcceptsWhiteSpaces = true;
			this.MaxLength = 65536;
			this.PseudoClasses.Set(":uriTextBox", true);
		}


		/// <summary>
		/// Get or set default scheme of URI if user doesn't input the scheme.
		/// </summary>
		public string? DefaultUriScheme
		{
			get => this.GetValue(DefaultUriSchemeProperty);
			set => this.SetValue(DefaultUriSchemeProperty, value);
		}
		
		
		/// <inheritdoc/>.
		public override Uri? Object
		{
			get => (Uri?)((ObjectTextBox)this).Object;
			set => ((ObjectTextBox)this).Object = value;
		}


		/// <inheritdoc/>
		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			var property = change.Property;
			if (property == DefaultUriSchemeProperty || property == UriKindProperty)
				this.Validate();
		}


		/// <inheritdoc/>
		protected override void OnTextInput(TextInputEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.Text) && Math.Min(this.SelectionStart, this.SelectionEnd) == 0)
				e.Handled = true;
			base.OnTextInput(e);
		}


		/// <inheritdoc/>.
		protected override void RaiseObjectChanged(Uri? oldValue, Uri? newValue) =>
			this.RaisePropertyChanged(ObjectProperty, oldValue, newValue);


		/// <inheritdoc/>
		protected override bool TryConvertToObject(string text, out Uri? obj)
		{
			if (!UriSchemeRegex.IsMatch(text))
			{
				var defaultScheme = this.GetValue(DefaultUriSchemeProperty);
				if (!string.IsNullOrWhiteSpace(defaultScheme))
					text = $"{defaultScheme}://{text}";
			}
			return Uri.TryCreate(text, this.UriKind, out obj);
		}


		/// <summary>
		/// Get or set target <see cref="UriKind"/>.
		/// </summary>
		public UriKind UriKind
		{
			get => this.GetValue(UriKindProperty);
			set => this.SetValue(UriKindProperty, value);
		}
	}
}
