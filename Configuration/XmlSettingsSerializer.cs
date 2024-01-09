using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Implementation of <see cref="ISettingsSerializer"/> which serialize settings to XML format data.
	/// </summary>
	public class XmlSettingsSerializer : ISettingsSerializer
	{
		/// <summary>
		/// Default instance.
		/// </summary>
		public static readonly XmlSettingsSerializer Default = new XmlSettingsSerializer();


		/// <summary>
		/// Type name of <see cref="bool"/>.
		/// </summary>
		protected const string BooleanType = "Boolean";
		/// <summary>
		/// Type name of <see cref="byte"/>.
		/// </summary>
		protected const string ByteType = "Byte";
		/// <summary>
		/// Type name of <see cref="short"/>.
		/// </summary>
		protected const string Int16Type = "Int16";
		/// <summary>
		/// Type name of <see cref="int"/>.
		/// </summary>
		protected const string Int32Type = "Int32";
		/// <summary>
		/// Type name of <see cref="long"/>.
		/// </summary>
		protected const string Int64Type = "Int64";
		/// <summary>
		/// Type name of <see cref="float"/>.
		/// </summary>
		protected const string SingleType = "Single";
		/// <summary>
		/// Type name of <see cref="double"/>.
		/// </summary>
		protected const string DoubleType = "Double";
		/// <summary>
		/// Type name of <see cref="DateTime"/>.
		/// </summary>
		protected const string DateTimeType = "DateTime";
		/// <summary>
		/// Type name of <see cref="string"/>.
		/// </summary>
		protected const string StringType = "String";


#pragma warning disable CS1591
		// Deserialize.
		public void Deserialize(Stream stream, out IDictionary<SettingKey, object> values, out SettingsMetadata metadata)
		{
			// parse document
			var xmlDocument = new XmlDocument();
			xmlDocument.Load(stream);

			// find root element
			var settingsElement = xmlDocument.FirstChild.Let((node) =>
			{
				while (node != null)
				{
					if (node.NodeType == XmlNodeType.Element && node.Name == "Settings")
						return node;
					node = node.NextSibling;
				}
				return null;
			}) ?? throw new XmlException("No 'Settings' element.");

			// get metadata and values
			var version = (int?)null;
			var lastModifiedTime = (DateTime?)null;
			var keyValues = new Dictionary<SettingKey, object>();
			var node = settingsElement.FirstChild;
			while (node != null)
			{
				try
				{
					if (node.NodeType != XmlNodeType.Element)
						continue;
					switch (node.Name)
					{
						case "Version":
							if (version != null)
								throw new XmlException("Duplicate 'Version' specified.");
							version = node.Let((_) =>
							{
								if (!node.HasChildNodes)
									return null;
								return node.FirstChild?.Let((it) =>
								{
									if (it.NodeType == XmlNodeType.Text && int.TryParse(it.Value, out var intValue))
										return (int?)intValue;
									return null;
								});
							});
							break;
						case "LastModifiedTime":
							if (lastModifiedTime != null)
								throw new XmlException("Duplicate 'LastModifiedTime' specified.");
							lastModifiedTime = node.Let((_) =>
							{
								if (!node.HasChildNodes)
									return null;
								return node.FirstChild?.Let((it) =>
								{
									if (it.NodeType == XmlNodeType.Text && long.TryParse(it.Value, out var longValue))
										return (DateTime?)DateTime.FromBinary(longValue);
									return null;
								});
							});
							break;
						case "Values":
							if (node.HasChildNodes)
							{
								var keyValueNode = node.FirstChild;
								while (keyValueNode != null)
								{
									try
									{
										if (keyValueNode.NodeType != XmlNodeType.Element || keyValueNode.Name != "Value" || !keyValueNode.HasChildNodes)
											continue;
										var name = (string?)null;
										var typeName = (string?)null;
										var defaultValue = (object?)null;
										var value = (object?)null;
										var childNode = keyValueNode.FirstChild;
										while (childNode != null)
										{
											try
											{
												if (childNode.NodeType != XmlNodeType.Element || !childNode.HasChildNodes)
													continue;
												switch (childNode.Name)
												{
													case "Name":
														name = childNode.FirstChild?.Value;
														break;
													case "Type":
														typeName = childNode.FirstChild?.Value;
														break;
													case "Default":
														if (typeName == null)
															throw new XmlException($"No type of setting value specified for '{name}'.");
														defaultValue = this.ReadXmlValue(childNode.FirstChild.AsNonNull(), typeName);
														break;
													case "Value":
														if (typeName == null)
															throw new XmlException($"No type of setting value specified for '{name}'.");
														value = this.ReadXmlValue(childNode.FirstChild.AsNonNull(), typeName);
														break;
												}
											}
											finally
											{
												childNode = childNode.NextSibling;
											}
										}
										if (name == null)
											throw new XmlException("No name of setting key specified.");
										if (defaultValue == null)
											throw new XmlException($"No default setting value specified for '{name}'.");
										if (value == null)
											throw new XmlException($"No value specified for '{name}'.");
										keyValues[new SettingKey(name, defaultValue.GetType(), defaultValue)] = value;
									}
									finally
									{
										keyValueNode = keyValueNode.NextSibling;
									}
								}
							}
							break;
					}
				}
				finally
				{
					node = node.NextSibling;
				}
			}

			// complete
			if (version == null)
				throw new XmlException("No 'Version' specified.");
			if (lastModifiedTime == null)
				throw new XmlException("No 'LastModifiedTime' specified.");
			metadata = new SettingsMetadata(version.GetValueOrDefault(), lastModifiedTime.GetValueOrDefault());
			values = keyValues;
		}
#pragma warning restore CS1591


		/// <summary>
		/// Get type name of given type of value.
		/// </summary>
		/// <param name="valueType">Type of value.</param>
		/// <returns>Type name.</returns>
		protected virtual string GetTypeName(Type valueType)
		{
			if (valueType.IsArray)
			{
				var elementType = valueType.GetElementType();
				if (valueType.GetArrayRank() == 1 && elementType != null)
					return this.GetTypeName(elementType);
			}
			else if (valueType.IsEnum)
				return $"{valueType.FullName}, {valueType.Assembly.GetName().Name}";
			else if (valueType == typeof(bool))
				return BooleanType;
			else if (valueType == typeof(byte))
				return ByteType;
			else if (valueType == typeof(short))
				return Int16Type;
			else if (valueType == typeof(int))
				return Int32Type;
			else if (valueType == typeof(long))
				return Int64Type;
			else if (valueType == typeof(float))
				return SingleType;
			else if (valueType == typeof(double))
				return DoubleType;
			else if (valueType == typeof(DateTime))
				return DateTimeType;
			else if (valueType == typeof(string))
				return StringType;
			throw new ArgumentException($"Unsupported type of value: {valueType.FullName}.");
		}


		/// <summary>
		/// Check whether given XML node represents an array of values or not.
		/// </summary>
		/// <param name="xmlNode">XML node.</param>
		/// <returns>True if XML node represents an array of values.</returns>
		protected bool IsXmlValueArray(XmlNode xmlNode) => xmlNode.NodeType == XmlNodeType.Element && xmlNode.Name == "Array";


		/// <summary>
		/// Read setting value from XML value according to type name. XML value will be treated as string or string array if type name cannot be recognized.
		/// </summary>
		/// <param name="xmlValue">XML value.</param>
		/// <param name="typeName">Type name.</param>
		/// <returns>Read setting value.</returns>
		protected virtual object ReadXmlValue(XmlNode xmlValue, string typeName)
		{
			switch(typeName)
			{
				case BooleanType:
					if(this.IsXmlValueArray(xmlValue))
						return new List<bool>().Also((list) => this.ReadXmlValueArray(xmlValue, typeName, list)).ToArray();
					return bool.Parse(xmlValue.Value ?? "");
				case ByteType:
					if (byte.TryParse(xmlValue.Value, out var byteValue))
						return byteValue;
					return Convert.FromBase64String(xmlValue.Value ?? "");
				case Int16Type:
					if (this.IsXmlValueArray(xmlValue))
						return new List<short>().Also((list) => this.ReadXmlValueArray(xmlValue, typeName, list)).ToArray();
					return short.Parse(xmlValue.Value ?? "");
				case Int32Type:
					if (this.IsXmlValueArray(xmlValue))
						return new List<int>().Also((list) => this.ReadXmlValueArray(xmlValue, typeName, list)).ToArray();
					return int.Parse(xmlValue.Value ?? "");
				case Int64Type:
					if (this.IsXmlValueArray(xmlValue))
						return new List<long>().Also((list) => this.ReadXmlValueArray(xmlValue, typeName, list)).ToArray();
					return long.Parse(xmlValue.Value ?? "");
				case SingleType:
					if (this.IsXmlValueArray(xmlValue))
						return new List<float>().Also((list) => this.ReadXmlValueArray(xmlValue, typeName, list)).ToArray();
					return xmlValue.Value.Let((str) =>
					{
						return str switch
						{
							"NaN" => float.NaN,
							"+Infinity" => float.PositiveInfinity,
							"-Infinity" => float.NegativeInfinity,
							_ => float.Parse(str ?? ""),
						};
					});
				case DoubleType:
					if (this.IsXmlValueArray(xmlValue))
						return new List<double>().Also((list) => this.ReadXmlValueArray(xmlValue, typeName, list)).ToArray();
					return xmlValue.Value.Let((str) =>
					{
						return str switch
						{
							"NaN" => double.NaN,
							"+Infinity" => double.PositiveInfinity,
							"-Infinity" => double.NegativeInfinity,
							_ => double.Parse(str ?? ""),
						};
					});
				case DateTimeType:
					if (this.IsXmlValueArray(xmlValue))
						return new List<DateTime>().Also((list) => this.ReadXmlValueArray(xmlValue, typeName, list)).ToArray();
					return DateTime.FromBinary(long.Parse(xmlValue.Value ?? ""));
				default:
#pragma warning disable IL2057
					return Type.GetType(typeName)?.Let(type =>
					{
#pragma warning restore IL2057
						if (!type.IsEnum)
							return null;
						try
						{
							if (this.IsXmlValueArray(xmlValue))
							{
								return new ArrayList().Let(list =>
								{
									this.ReadXmlValueArray(xmlValue, typeName, list);
#pragma warning disable IL3050
									return Array.CreateInstance(type, list.Count).Also(array =>
									{
										for (var i = array.Length - 1; i >= 0; --i)
											array.SetValue(list[i], i);
									});
#pragma warning restore IL3050
								});
							}
							return Enum.Parse(type, xmlValue.Value ?? "");
						}
						catch
						{
							return null;
						}
					}) ?? this.ReadXmlValueAsString(xmlValue);
			}
		}


		/// <summary>
		/// Read setting value as array from XML value.
		/// </summary>
		/// <param name="arrayNode">XML node represents an array of values.</param>
		/// <param name="typeName">Type name.</param>
		/// <param name="list"><see cref="IList"/> to receive read values.</param>
		protected void ReadXmlValueArray(XmlNode arrayNode, string typeName, IList list)
		{
			if (!this.IsXmlValueArray(arrayNode))
				throw new XmlException("Not an array node.");
			if (!arrayNode.HasChildNodes)
				return;
			var node = arrayNode.FirstChild;
			while (node != null)
			{
				try
				{
					if (node.NodeType != XmlNodeType.Element || node.Name != "Value" || !node.HasChildNodes)
						continue;
					list.Add(this.ReadXmlValue(node.FirstChild.AsNonNull(), typeName));
				}
				finally
				{
					node = node.NextSibling;
				}
			}
		}


		/// <summary>
		/// Read setting value from XML value as string or string array.
		/// </summary>
		/// <param name="xmlValue">XML value.</param>
		/// <returns>Setting value as string or string array.</returns>
		protected object ReadXmlValueAsString(XmlNode xmlValue)
		{
			if(this.IsXmlValueArray(xmlValue))
				return new List<string>().Also((list) => this.ReadXmlValueArray(xmlValue, StringType, list)).ToArray();
			return xmlValue.Value ?? "";
		}


#pragma warning disable CS1591
		// Serialize.
		public void Serialize(Stream stream, IDictionary<SettingKey, object> values, SettingsMetadata metadata)
		{
			// prepare
			using var writer = XmlWriter.Create(stream, new XmlWriterSettings()
			{
				Encoding = Encoding.UTF8,
				Indent = true,
			});

			// start document
			writer.WriteStartDocument();
			writer.WriteStartElement("Settings");

			// version
			writer.WriteStartElement("Version");
			writer.WriteValue(metadata.Version);
			writer.WriteEndElement();

			// last modified time
			writer.WriteStartElement("LastModifiedTime");
			writer.WriteValue(metadata.LastModifiedTime.ToBinary());
			writer.WriteEndElement();

			// key-values
			writer.WriteStartElement("Values");
			foreach (var keyValue in values)
			{
				var key = keyValue.Key;
				var value = keyValue.Value;
				var typeName = this.GetTypeName(key.DefaultValue.GetType());
				writer.WriteStartElement("Value");
				writer.WriteStartElement("Name");
				writer.WriteValue(key.Name);
				writer.WriteEndElement();
				writer.WriteStartElement("Type");
				writer.WriteValue(typeName);
				writer.WriteEndElement();
				writer.WriteStartElement("Default");
				this.WriteXmlValue(writer, key.DefaultValue);
				writer.WriteEndElement();
				writer.WriteStartElement("Value");
				this.WriteXmlValue(writer, value);
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			// complete
			writer.WriteEndElement();
			writer.WriteEndDocument();
		}
#pragma warning restore CS1591


		/// <summary>
		/// Write setting value as XML value.
		/// </summary>
		/// <param name="writer"><see cref="XmlWriter"/>.</param>
		/// <param name="value">Setting value.</param>
		protected virtual void WriteXmlValue(XmlWriter writer, object value)
		{
			if (value is byte[] byteArray)
				writer.WriteString(Convert.ToBase64String(byteArray));
			else if (value is Array array)
			{
				if (array.Rank != 1)
					throw new ArgumentException("Only 1-dimensional array is supported.");
				writer.WriteStartElement("Array");
				for (int i = 0, count = array.Length; i < count; ++i)
				{
					writer.WriteStartElement("Value");
					this.WriteXmlValue(writer, array.GetValue(i).AsNonNull());
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
			}
			else if (value is float floatValue)
			{
				if (float.IsNaN(floatValue))
					writer.WriteValue("NaN");
				else if (float.IsPositiveInfinity(floatValue))
					writer.WriteValue("+Infinity");
				else if (float.IsNegativeInfinity(floatValue))
					writer.WriteValue("-Infinity");
				else
					writer.WriteValue(floatValue);
			}
			else if (value is double doubleValue)
			{
				if (double.IsNaN(doubleValue))
					writer.WriteValue("NaN");
				else if (double.IsPositiveInfinity(doubleValue))
					writer.WriteValue("+Infinity");
				else if (double.IsNegativeInfinity(doubleValue))
					writer.WriteValue("-Infinity");
				else
					writer.WriteValue(doubleValue);
			}
			else if (value is DateTime dateTime)
				writer.WriteValue(dateTime.ToBinary());
			else
				writer.WriteValue(value.ToString());
		}
	}
}
