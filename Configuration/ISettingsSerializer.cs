using System;
using System.Collections.Generic;
using System.IO;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Interface to serialize and deserialize <see cref="BaseSettings"/>.
	/// </summary>
	public interface ISettingsSerializer
	{
		/// <summary>
		/// Deserialize keys and values of settings.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/> to read serialized settings.</param>
		/// <param name="values">Deserialized keys and values.</param>
		/// <param name="metadata">Deserialized metadata.</param>
		void Deserialize(Stream stream, out IDictionary<SettingKey, object> values, out SettingsMetadata metadata);


		/// <summary>
		/// Serialize settings.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/> to write serialized settings.</param>
		/// <param name="values">All keys and values.</param>
		/// <param name="metadata">Metadata.</param>
		void Serialize(Stream stream, IDictionary<SettingKey, object> values, SettingsMetadata metadata);
	}
}
