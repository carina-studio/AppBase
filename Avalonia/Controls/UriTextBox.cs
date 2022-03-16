using Avalonia;
using Avalonia.Controls;
using System;
using System.Text.RegularExpressions;

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
		public static readonly AvaloniaProperty<string?> DefaultUriSchemeProperty = AvaloniaProperty.Register<UriTextBox, string?>(nameof(DefaultUriScheme), null);
		/// <summary>
		/// Property of <see cref="UriKind"/>.
		/// </summary>
		public static readonly AvaloniaProperty<UriKind> UriKindProperty = AvaloniaProperty.Register<UriTextBox, UriKind>(nameof(IsTextValid), UriKind.Absolute);


		// Static fields.
		static readonly Regex UriSchemeRegex = new Regex("^[\\w]+\\://");


		/// <summary>
		/// Initialize new <see cref="UriTextBox"/> instance.
		/// </summary>
		public UriTextBox()
		{
			this.MaxLength = 65536;
			this.PseudoClasses.Set(":uriTextBox", true);
		}


		/// <summary>
		/// Get or set default scheme of URI if user doesn't input the scheme.
		/// </summary>
		public string? DefaultUriScheme
		{
			get => this.GetValue<string?>(DefaultUriSchemeProperty);
			set => this.SetValue<string?>(DefaultUriSchemeProperty, value);
		}


		/// <inheritdoc/>
		protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
		{
			base.OnPropertyChanged(change);
			var property = change.Property;
			if (property == DefaultUriSchemeProperty || property == UriKindProperty)
				this.Validate();
		}


		/// <inheritdoc/>
		protected override bool TryConvertToObject(string text, out Uri? obj)
		{
			if (!UriSchemeRegex.IsMatch(text))
			{
				var defaultScheme = this.GetValue<string?>(DefaultUriSchemeProperty);
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
			get => this.GetValue<UriKind>(UriKindProperty);
			set => this.SetValue<UriKind>(UriKindProperty, value);
		}
	}
}
