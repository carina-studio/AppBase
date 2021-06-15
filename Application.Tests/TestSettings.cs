using CarinaStudio.Configuration;
using System;

namespace CarinaStudio
{
	/// <summary>
	/// Test implementation of <see cref="BaseSettings"/>.
	/// </summary>
	class TestSettings : BaseSettings
	{
		public TestSettings() : base(JsonSettingsSerializer.Default)
		{ }
		protected override int Version => 1;
		protected override void OnUpgrade(int oldVersion)
		{ }
	}
}
