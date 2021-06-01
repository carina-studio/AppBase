using NUnit.Framework;
using System;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Tests of <see cref="XmlSettingsSerializer"/>.
	/// </summary>
	[TestFixture]
	class XmlSettingsSerializerTests : BaseSettingsSerializerTests<XmlSettingsSerializer>
	{
		/// <summary>
		/// Initialize new <see cref="XmlSettingsSerializerTests"/> instance.
		/// </summary>
		public XmlSettingsSerializerTests() : base("test_settings.xml")
		{ }


		// Create serializer.
		protected override XmlSettingsSerializer CreateSettingsSerializer() => XmlSettingsSerializer.Default;
	}
}
