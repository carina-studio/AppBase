using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Implementation of <see cref="ISettingsSerializer"/> which serialize settings to JSON format data.
	/// </summary>
	public class JsonSettingsSerializer : ISettingsSerializer
	{
		/// <summary>
		/// Default instance.
		/// </summary>
		public static readonly JsonSettingsSerializer Default = new JsonSettingsSerializer();


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
			// build JSON document
			using var jsonDocument = JsonDocument.Parse(stream);

			// read metadata
			var rootJsonElement = jsonDocument.RootElement;
			if (rootJsonElement.ValueKind != JsonValueKind.Object)
				throw new ArgumentException("Invalid root JSON element.");
			var version = rootJsonElement.GetProperty("Version").Let((it) =>
			{
				if (it.ValueKind == JsonValueKind.Undefined)
					return 0;
				return it.GetInt32();
			});
			var lastModifiedTime = rootJsonElement.GetProperty("LastModifiedTime").Let((it) =>
			{
				if (it.ValueKind == JsonValueKind.Undefined)
					return DateTime.Now;
				return DateTime.FromBinary(it.GetInt64());
			});

			// read keys and values
			var keyValues = new Dictionary<SettingKey, object>();
			rootJsonElement.GetProperty("Values").Let((jsonValuesArray) =>
			{
				if (jsonValuesArray.ValueKind == JsonValueKind.Undefined)
					return;
				foreach (var jsonValueEntry in jsonValuesArray.EnumerateArray())
				{
					var name = jsonValueEntry.GetProperty("Name").GetString();
					var typeName = jsonValueEntry.GetProperty("Type").GetString();
					var defaultValue = this.ReadJsonValue(jsonValueEntry.GetProperty("Default"), typeName);
					var value = this.ReadJsonValue(jsonValueEntry.GetProperty("Value"), typeName);
					keyValues[new SettingKey(name, defaultValue.GetType(), defaultValue)] = value;
				}
			});

			// complete
			values = keyValues;
			metadata = new SettingsMetadata(version, lastModifiedTime);
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
		/// Read setting value from JSON value according to type name. JSON value will be treated as string or string array if type name cannot be recognized.
		/// </summary>
		/// <param name="jsonValue">JSON value.</param>
		/// <param name="typeName">Type name.</param>
		/// <returns>Read setting value.</returns>
		protected object ReadJsonValue(JsonElement jsonValue, string typeName)
		{
			return typeName switch
			{
				BooleanType => jsonValue.ValueKind switch
				{
					JsonValueKind.Array => new bool[jsonValue.GetArrayLength()].Also((array) =>
					{
						var index = 0;
						foreach (var e in jsonValue.EnumerateArray())
							array[index++] = (bool)this.ReadJsonValue(e, typeName);
					}),
					_ => jsonValue.GetBoolean(),
				},
				ByteType => jsonValue.ValueKind switch
				{
					JsonValueKind.String => jsonValue.GetBytesFromBase64(),
					_ => jsonValue.GetByte(),
				},
				Int16Type => jsonValue.ValueKind switch
				{
					JsonValueKind.Array => new short[jsonValue.GetArrayLength()].Also((array) =>
					{
						var index = 0;
						foreach (var e in jsonValue.EnumerateArray())
							array[index++] = (short)this.ReadJsonValue(e, typeName);
					}),
					_ => jsonValue.GetInt16(),
				},
				Int32Type => jsonValue.ValueKind switch
				{
					JsonValueKind.Array => new int[jsonValue.GetArrayLength()].Also((array) =>
					{
						var index = 0;
						foreach (var e in jsonValue.EnumerateArray())
							array[index++] = (int)this.ReadJsonValue(e, typeName);
					}),
					_ => jsonValue.GetInt32(),
				},
				Int64Type => jsonValue.ValueKind switch
				{
					JsonValueKind.Array => new long[jsonValue.GetArrayLength()].Also((array) =>
					{
						var index = 0;
						foreach (var e in jsonValue.EnumerateArray())
							array[index++] = (long)this.ReadJsonValue(e, typeName);
					}),
					_ => jsonValue.GetInt64(),
				},
				SingleType => jsonValue.ValueKind switch
				{
					JsonValueKind.Array => new float[jsonValue.GetArrayLength()].Also((array) =>
					{
						var index = 0;
						foreach (var e in jsonValue.EnumerateArray())
							array[index++] = (float)this.ReadJsonValue(e, typeName);
					}),
					JsonValueKind.String => jsonValue.GetString() switch
					{
						"NaN" => float.NaN,
						"PositiveInfinity" => float.PositiveInfinity,
						"NegativeInfinity" => float.NegativeInfinity,
						_ => throw new ArgumentException($"Invalid single value: {jsonValue}."),
					},
					_ => jsonValue.GetSingle(),
				},
				DoubleType => jsonValue.ValueKind switch
				{
					JsonValueKind.Array => new double[jsonValue.GetArrayLength()].Also((array) =>
					{
						var index = 0;
						foreach (var e in jsonValue.EnumerateArray())
							array[index++] = (double)this.ReadJsonValue(e, typeName);
					}),
					JsonValueKind.String => jsonValue.GetString() switch
					{
						"NaN" => double.NaN,
						"PositiveInfinity" => double.PositiveInfinity,
						"NegativeInfinity" => double.NegativeInfinity,
						_ => throw new ArgumentException($"Invalid double value: {jsonValue}."),
					},
					_ => jsonValue.GetDouble(),
				},
				DateTimeType => jsonValue.ValueKind switch
				{
					JsonValueKind.Array => new DateTime[jsonValue.GetArrayLength()].Also((array) =>
					{
						var index = 0;
						foreach (var e in jsonValue.EnumerateArray())
							array[index++] = (DateTime)this.ReadJsonValue(e, typeName);
					}),
					_ => DateTime.FromBinary(jsonValue.GetInt64()),
				},
				StringType => this.ReadJsonValueAsString(jsonValue),
				_ => Type.GetType(typeName)?.Let((type) =>
				{
					if (type.IsEnum)
					{
						if (jsonValue.ValueKind == JsonValueKind.Array)
						{
							return Array.CreateInstance(type, jsonValue.GetArrayLength()).Also((array) =>
							{
								var index = 0;
								foreach (var e in jsonValue.EnumerateArray())
									array.SetValue(this.ReadJsonValue(e, typeName), index++);
							});
						}
						else if (Enum.TryParse(type, jsonValue.GetString(), out var enumValue))
							return enumValue;
					}
					return null;
				}) ?? this.ReadJsonValueAsString(jsonValue),
			};
		}


		/// <summary>
		/// Read setting value from JSON value as string or string array.
		/// </summary>
		/// <param name="jsonValue">JSON value.</param>
		/// <returns>Setting value as string or string array.</returns>
		protected object ReadJsonValueAsString(JsonElement jsonValue)
		{
			return jsonValue.ValueKind switch
			{
				JsonValueKind.Array => new string[jsonValue.GetArrayLength()].Also((array) =>
				{
					var index = 0;
					foreach (var e in jsonValue.EnumerateArray())
						array[index++] = (string)ReadJsonValueAsString(e);
				}),
				_ => jsonValue.GetString(),
			};
		}


#pragma warning disable CS1591
		// Serialize settings.
		public void Serialize(Stream stream, IDictionary<SettingKey, object> values, SettingsMetadata metadata)
		{
			// prepare
			using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true });
			writer.WriteStartObject();

			// version
			writer.WriteNumber("Version", metadata.Version);

			// last modified time
			writer.WriteNumber("LastModifiedTime", metadata.LastModifiedTime.ToBinary());

			// keys and values
			writer.WritePropertyName("Values");
			writer.WriteStartArray();
			foreach (var keyValue in values)
			{
				var key = keyValue.Key;
				var typeName = this.GetTypeName(key.ValueType);
				writer.WriteStartObject();
				writer.WriteString("Name", key.Name);
				writer.WriteString("Type", typeName);
				writer.WritePropertyName("Default");
				this.WriteJsonValue(writer, typeName, key.DefaultValue);
				writer.WritePropertyName("Value");
				this.WriteJsonValue(writer, typeName, keyValue.Value);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();

			// complete
			writer.WriteEndObject();
			writer.Flush();
		}
#pragma warning restore CS1591


		/// <summary>
		/// Write JSON value according to type name.
		/// </summary>
		/// <param name="writer"><see cref="Utf8JsonWriter"/>.</param>
		/// <param name="typeName">Type name.</param>
		/// <param name="value">Value to write.</param>
		protected virtual void WriteJsonValue(Utf8JsonWriter writer, string typeName, object value)
		{
			switch (typeName)
			{
				case BooleanType:
					if (value is bool[] boolArray)
					{
						writer.WriteStartArray();
						foreach (var b in boolArray)
							this.WriteJsonValue(writer, typeName, b);
						writer.WriteEndArray();
					}
					else
						writer.WriteBooleanValue((bool)value);
					break;
				case ByteType:
					if (value is byte[] byteArray)
						writer.WriteBase64StringValue(byteArray.AsSpan());
					else
						writer.WriteNumberValue((byte)value);
					break;
				case Int16Type:
					if (value is short[] shortArray)
					{
						writer.WriteStartArray();
						foreach (var i in shortArray)
							this.WriteJsonValue(writer, typeName, i);
						writer.WriteEndArray();
					}
					else
						writer.WriteNumberValue((short)value);
					break;
				case Int32Type:
					if (value is int[] intArrar)
					{
						writer.WriteStartArray();
						foreach (var i in intArrar)
							this.WriteJsonValue(writer, typeName, i);
						writer.WriteEndArray();
					}
					else
						writer.WriteNumberValue((int)value);
					break;
				case Int64Type:
					if (value is long[] longArray)
					{
						writer.WriteStartArray();
						foreach (var i in longArray)
							this.WriteJsonValue(writer, typeName, i);
						writer.WriteEndArray();
					}
					else
						writer.WriteNumberValue((long)value);
					break;
				case SingleType:
					if (value is float[] floatArray)
					{
						writer.WriteStartArray();
						foreach (var f in floatArray)
							this.WriteJsonValue(writer, typeName, f);
						writer.WriteEndArray();
					}
					else
					{
						((float)value).Let((floatValue) =>
						{
							if (float.IsNaN(floatValue))
								writer.WriteStringValue("NaN");
							else if (float.IsPositiveInfinity(floatValue))
								writer.WriteStringValue("PositiveInfinity");
							else if (float.IsNegativeInfinity(floatValue))
								writer.WriteStringValue("NegativeInfinity");
							else
								writer.WriteNumberValue(floatValue);
						});
					}
					break;
				case DoubleType:
					if (value is double[] doubleArray)
					{
						writer.WriteStartArray();
						foreach (var d in doubleArray)
							this.WriteJsonValue(writer, typeName, d);
						writer.WriteEndArray();
					}
					else
					{
						((double)value).Let((doubleValue) =>
						{
							if (double.IsNaN(doubleValue))
								writer.WriteStringValue("NaN");
							else if (double.IsPositiveInfinity(doubleValue))
								writer.WriteStringValue("PositiveInfinity");
							else if (double.IsNegativeInfinity(doubleValue))
								writer.WriteStringValue("NegativeInfinity");
							else
								writer.WriteNumberValue(doubleValue);
						});
					}
					break;
				case DateTimeType:
					if (value is DateTime[] dtArray)
					{
						writer.WriteStartArray();
						foreach (var d in dtArray)
							this.WriteJsonValue(writer, typeName, d);
						writer.WriteEndArray();
					}
					else
						writer.WriteNumberValue(((DateTime)value).ToBinary());
					break;
				case StringType:
					if (value is string[] stringArray)
					{
						writer.WriteStartArray();
						foreach (var s in stringArray)
							this.WriteJsonValue(writer, typeName, s);
						writer.WriteEndArray();
					}
					else
						writer.WriteStringValue((string)value);
					break;
				default:
					if (value is Array unknownArray && unknownArray.Rank == 1)
					{
						writer.WriteStartArray();
						foreach (var e in unknownArray)
							this.WriteJsonValue(writer, typeName, e.AsNonNull());
						writer.WriteEndArray();
					}
					else if (value.GetType().IsEnum)
						writer.WriteStringValue(value.ToString());
					else
						throw new ArgumentException($"Unsupported type name: {typeName}.");
					break;
			}
		}
	}
}
