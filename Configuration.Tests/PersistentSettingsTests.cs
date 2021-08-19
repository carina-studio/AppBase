using NUnit.Framework;
using System;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Tests of <see cref="PersistentSettings"/>.
	/// </summary>
	[TestFixture]
	class PersistentSettingsTests : BaseSettingsTests
	{
		// Create settings.
		protected override ISettings CreateSettings(ISettings? template = null)
		{
			if (template == null)
				return new TestSettings(1, JsonSettingsSerializer.Default);
			return new TestSettings(template, JsonSettingsSerializer.Default);
		}
	}
}
