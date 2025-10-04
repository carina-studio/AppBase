using System;
using System.Collections.Generic;
using System.IO;

namespace CarinaStudio.Configuration;

/// <summary>
/// Interface to serialize and deserialize <see cref="ISettings"/>.
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


/// <summary>
/// Metadata of <see cref="PersistentSettings"/>.
/// </summary>
public class SettingsMetadata
{
	/// <summary>
	/// Initialize new <see cref="SettingsMetadata"/> instance.
	/// </summary>
	/// <param name="version">Version of settings.</param>
	/// <param name="lastModifiedTime">Timestamp of last change to settings.</param>
	public SettingsMetadata(int version, DateTime lastModifiedTime)
	{
		this.LastModifiedTime = lastModifiedTime;
		this.Version = version;
	}


	/// <summary>
	/// Timestamp of last change to settings.
	/// </summary>
	public DateTime LastModifiedTime { get; }


	/// <summary>
	/// Version of settings.
	/// </summary>
	public int Version { get; }
}