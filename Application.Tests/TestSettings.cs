using CarinaStudio.Configuration;
using System;

namespace CarinaStudio
{
	/// <summary>
	/// Test implementation of <see cref="PersistentSettings"/>.
	/// </summary>
	class TestSettings : PersistentSettings
	{
		// Keys.
		public static readonly SettingKey<int> Int32 = new SettingKey<int>("Int32");


		// Constructor.
		public TestSettings() : base(JsonSettingsSerializer.Default)
		{ }

		
		// Implementations.
		protected override int Version => 1;
		protected override void OnUpgrade(int oldVersion)
		{ }
	}
}
