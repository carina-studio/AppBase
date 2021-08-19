using NUnit.Framework;
using System;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Tests of <see cref="MemorySettings"/>.
	/// </summary>
	[TestFixture]
	class MemorySettingsTests : BaseSettingsTests
	{
		// Create settings.
		protected override ISettings CreateSettings(ISettings? template = null)
		{
			if (template == null)
				return new MemorySettings();
			return new MemorySettings(template);
		}
	}
}
