using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
		/// Type name of <see cref="bool"/> array.
		/// </summary>
		protected const string BooleanArrayType = "BooleanArray";
		/// <summary>
		/// Type name of <see cref="bool"/>.
		/// </summary>
		protected const string BooleanType = "Boolean";
		/// <summary>
		/// Type name of <see cref="byte"/> array.
		/// </summary>
		protected const string ByteArrayType = "ByteArray";
		/// <summary>
		/// Type name of <see cref="byte"/>.
		/// </summary>
		protected const string ByteType = "Byte";
		/// <summary>
		/// Type name of <see cref="short"/> array.
		/// </summary>
		protected const string Int16ArrayType = "Int16Array";
		/// <summary>
		/// Type name of <see cref="short"/>.
		/// </summary>
		protected const string Int16Type = "Int16";
		/// <summary>
		/// Type name of <see cref="int"/> array.
		/// </summary>
		protected const string Int32ArrayType = "Int32Array";
		/// <summary>
		/// Type name of <see cref="int"/>.
		/// </summary>
		protected const string Int32Type = "Int32";
		/// <summary>
		/// Type name of <see cref="long"/> array.
		/// </summary>
		protected const string Int64ArrayType = "Int64Array";
		/// <summary>
		/// Type name of <see cref="long"/>.
		/// </summary>
		protected const string Int64Type = "Int64";
		/// <summary>
		/// Type name of <see cref="float"/> array.
		/// </summary>
		protected const string SingleArrayType = "SingleArray";
		/// <summary>
		/// Type name of <see cref="float"/>.
		/// </summary>
		protected const string SingleType = "Single";
		/// <summary>
		/// Type name of <see cref="double"/> array.
		/// </summary>
		protected const string DoubleArrayType = "DoubleArray";
		/// <summary>
		/// Type name of <see cref="double"/>.
		/// </summary>
		protected const string DoubleType = "Double";
		/// <summary>
		/// Type name of <see cref="DateTime"/> array.
		/// </summary>
		protected const string DateTimeArrayType = "DateTimeArray";
		/// <summary>
		/// Type name of <see cref="DateTime"/>.
		/// </summary>
		protected const string DateTimeType = "DateTime";
		/// <summary>
		/// Type name of <see cref="string"/> array.
		/// </summary>
		protected const string StringArrayType = "StringArray";
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
					return this.GetTypeName(elementType) + "Array";
			}
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
		/// Read setting value from JSON value according to type name.
		/// </summary>
		/// <param name="jsonValue">JSON value.</param>
		/// <param name="typeName">Type name.</param>
		/// <returns>Read setting value.</returns>
		protected object ReadJsonValue(JsonElement jsonValue, string typeName)
		{
			return typeName switch
			{
				BooleanType => jsonValue.GetBoolean(),
				BooleanArrayType => new bool[jsonValue.GetArrayLength()].Also((array) =>
				{
					var index = 0;
					foreach (var e in jsonValue.EnumerateArray())
						array[index++] = (bool)this.ReadJsonValue(e, BooleanType);
				}),
				ByteType => jsonValue.GetByte(),
				ByteArrayType => jsonValue.GetBytesFromBase64(),
				Int16Type => jsonValue.GetInt16(),
				Int16ArrayType => new short[jsonValue.GetArrayLength()].Also((array) =>
				{
					var index = 0;
					foreach (var e in jsonValue.EnumerateArray())
						array[index++] = (short)this.ReadJsonValue(e, Int16Type);
				}),
				Int32Type => jsonValue.GetInt32(),
				Int32ArrayType => new int[jsonValue.GetArrayLength()].Also((array) =>
				{
					var index = 0;
					foreach (var e in jsonValue.EnumerateArray())
						array[index++] = (int)this.ReadJsonValue(e, Int32Type);
				}),
				Int64Type => jsonValue.GetInt64(),
				Int64ArrayType => new long[jsonValue.GetArrayLength()].Also((array) =>
				{
					var index = 0;
					foreach (var e in jsonValue.EnumerateArray())
						array[index++] = (long)this.ReadJsonValue(e, Int64Type);
				}),
				SingleType => jsonValue.GetSingle(),
				SingleArrayType => new float[jsonValue.GetArrayLength()].Also((array) =>
				{
					var index = 0;
					foreach (var e in jsonValue.EnumerateArray())
						array[index++] = (float)this.ReadJsonValue(e, SingleType);
				}),
				DoubleType => jsonValue.GetDouble(),
				DoubleArrayType => new double[jsonValue.GetArrayLength()].Also((array) =>
				{
					var index = 0;
					foreach (var e in jsonValue.EnumerateArray())
						array[index++] = (double)this.ReadJsonValue(e, DoubleType);
				}),
				DateTimeType => DateTime.FromBinary(jsonValue.GetInt64()),
				DateTimeArrayType => new DateTime[jsonValue.GetArrayLength()].Also((array) =>
				{
					var index = 0;
					foreach (var e in jsonValue.EnumerateArray())
						array[index++] = (DateTime)this.ReadJsonValue(e, DateTimeType);
				}),
				StringType => jsonValue.GetString(),
				StringArrayType => new string[jsonValue.GetArrayLength()].Also((array) =>
				{
					var index = 0;
					foreach (var e in jsonValue.EnumerateArray())
						array[index++] = (string)this.ReadJsonValue(e, StringType);
				}),
				_ => throw new ArgumentException($"Unsupported type name: {typeName}."),
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
			switch(typeName)
			{
				case BooleanType:
					writer.WriteBooleanValue((bool)value);
					break;
				case BooleanArrayType:
					writer.WriteStartArray();
					foreach (var b in (bool[])value)
						this.WriteJsonValue(writer, BooleanType, b);
					writer.WriteEndArray();
					break;
				case ByteType:
					writer.WriteNumberValue((byte)value);
					break;
				case ByteArrayType:
					writer.WriteBase64StringValue(((byte[])value).AsSpan());
					break;
				case Int16Type:
					writer.WriteNumberValue((short)value);
					break;
				case Int16ArrayType:
					writer.WriteStartArray();
					foreach (var i in (short[])value)
						this.WriteJsonValue(writer, Int16Type, i);
					writer.WriteEndArray();
					break;
				case Int32Type:
					writer.WriteNumberValue((int)value);
					break;
				case Int32ArrayType:
					writer.WriteStartArray();
					foreach (var i in (int[])value)
						this.WriteJsonValue(writer, Int32Type, i);
					writer.WriteEndArray();
					break;
				case Int64Type:
					writer.WriteNumberValue((long)value);
					break;
				case Int64ArrayType:
					writer.WriteStartArray();
					foreach (var i in (long[])value)
						this.WriteJsonValue(writer, Int64Type, i);
					writer.WriteEndArray();
					break;
				case SingleType:
					writer.WriteNumberValue((float)value);
					break;
				case SingleArrayType:
					writer.WriteStartArray();
					foreach (var f in (float[])value)
						this.WriteJsonValue(writer, SingleType, f);
					writer.WriteEndArray();
					break;
				case DoubleType:
					writer.WriteNumberValue((double)value);
					break;
				case DoubleArrayType:
					writer.WriteStartArray();
					foreach (var d in (double[])value)
						this.WriteJsonValue(writer, DoubleType, d);
					writer.WriteEndArray();
					break;
				case DateTimeType:
					writer.WriteNumberValue(((DateTime)value).ToBinary());
					break;
				case DateTimeArrayType:
					writer.WriteStartArray();
					foreach (var d in (DateTime[])value)
						this.WriteJsonValue(writer, DateTimeType, d);
					writer.WriteEndArray();
					break;
				case StringType:
					writer.WriteStringValue((string)value);
					break;
				case StringArrayType:
					writer.WriteStartArray();
					foreach (var s in (string[])value)
						this.WriteJsonValue(writer, StringType, s);
					writer.WriteEndArray();
					break;
				default:
					throw new ArgumentException($"Unsupported type name: {typeName}.");
			}
		}
	}
}
