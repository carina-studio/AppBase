using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Tests of <see cref="JsonSettingsSerializer"/>.
	/// </summary>
	[TestFixture]
	class JsonSettingsSerializerTests : BaseSettingsSerializerTests<JsonSettingsSerializer>
	{
		/// <summary>
		/// Initialize new <see cref="JsonSettingsSerializerTests"/> instance.
		/// </summary>
		public JsonSettingsSerializerTests() : base("test_settings.json")
		{ }


		// Create serializer.
		protected override JsonSettingsSerializer CreateSettingsSerializer() => JsonSettingsSerializer.Default;
	}
}
