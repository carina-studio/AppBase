using System;
using System.Collections.Generic;
using System.Linq;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Implementation of <see cref="BaseSettings"/> for tests.
	/// </summary>
	class TestSettings : BaseSettings
	{
		// Keys.
		public static readonly SettingKey<bool> BooleanKey = new SettingKey<bool>(nameof(BooleanKey), true);
		public static readonly SettingKey<bool[]> BooleanArrayKey = new SettingKey<bool[]>(nameof(BooleanArrayKey), new bool[] { true, false });
		public static readonly SettingKey<byte> ByteKey = new SettingKey<byte>(nameof(ByteKey), 127);
		public static readonly SettingKey<byte[]> ByteArrayKey = new SettingKey<byte[]>(nameof(ByteArrayKey), new byte[] { 1, 2, 3 });
		public static readonly SettingKey<short> Int16Key = new SettingKey<short>(nameof(Int16Key), 16384);
		public static readonly SettingKey<short[]> Int16ArrayKey = new SettingKey<short[]>(nameof(Int16ArrayKey), new short[] { 4, 5, 6 });
		public static readonly SettingKey<int> Int32Key = new SettingKey<int>(nameof(Int32Key), int.MaxValue / 2);
		public static readonly SettingKey<int[]> Int32ArrayKey = new SettingKey<int[]>(nameof(Int32ArrayKey), new int[] { 7, 8, 9 });
		public static readonly SettingKey<long> Int64Key = new SettingKey<long>(nameof(Int64Key), long.MaxValue / 2);
		public static readonly SettingKey<long[]> Int64ArrayKey = new SettingKey<long[]>(nameof(Int64ArrayKey), new long[] { 10, 11, 12 });
		public static readonly SettingKey<float> SingleKey = new SettingKey<float>(nameof(SingleKey), long.MaxValue / 2);
		public static readonly SettingKey<float[]> SingleArrayKey = new SettingKey<float[]>(nameof(SingleArrayKey), new float[] { 1, 0, -1 });
		public static readonly SettingKey<double> DoubleKey = new SettingKey<double>(nameof(DoubleKey), long.MaxValue / 2);
		public static readonly SettingKey<double[]> DoubleArrayKey = new SettingKey<double[]>(nameof(DoubleArrayKey), new double[] { double.NaN, 0, double.PositiveInfinity });
		public static readonly SettingKey<DateTime> DateTimeKey = new SettingKey<DateTime>(nameof(DateTimeKey), new DateTime(1911, 10, 10));
		public static readonly SettingKey<DateTime[]> DateTimeArrayKey = new SettingKey<DateTime[]>(nameof(DateTimeArrayKey), new DateTime[] { new DateTime(1911, 10, 10) });
		public static readonly SettingKey<string> StringKey = new SettingKey<string>(nameof(StringKey), "Carina Studio");
		public static readonly SettingKey<string[]> StringArrayKey = new SettingKey<string[]>(nameof(StringArrayKey), new string[] { "Hello", "World" });
		public static readonly IList<SettingKey> AllKeys = typeof(TestSettings).GetFields()
			.TakeWhile((it) => it.IsStatic && typeof(SettingKey).IsAssignableFrom(it.FieldType))
			.Select((field, _) => (SettingKey)field.GetValue(null).AsNonNull())
			.ToList()
			.AsReadOnly();


		// Values for tests.
		public static readonly IDictionary<SettingKey, object> TestValues = new Dictionary<SettingKey, object>().Also((it) =>
		{
			it[BooleanKey] = false;
			it[BooleanArrayKey] = new bool[] { true, true };
			it[ByteKey] = (byte)254;
			it[ByteArrayKey] = new byte[] { 0, 127, 255 };
			it[Int16Key] = short.MaxValue;
			it[Int16ArrayKey] = new short[] { short.MinValue, short.MaxValue };
			it[Int32Key] = int.MaxValue;
			it[Int32ArrayKey] = new int[] { int.MaxValue, 0, int.MinValue };
			it[Int64Key] = long.MinValue;
			it[Int64ArrayKey] = new long[] { long.MinValue, long.MaxValue };
			it[SingleKey] = 1.234f;
			it[SingleArrayKey] = new float[] { 3.14f, 0.99f };
			it[DoubleKey] = Math.PI;
			it[DoubleArrayKey] = new double[] { 1, 1.41421, 1.732, 2 };
			it[DateTimeKey] = new DateTime(2021, 5, 27, 15, 1, 0);
			it[DateTimeArrayKey] = new DateTime[0];
			it[StringKey] = "Hello World";
			it[StringArrayKey] = new string[] { "Carina", "Studio" };
		});


		// Public fields.
		public bool IsOnUpgradeCalled;
		public int OldVersion;


		// Constructor.
		public TestSettings(int version, ISettingsSerializer serializer) : base(serializer)
		{
			this.Version = version;
		}
		public TestSettings(TestSettings template, ISettingsSerializer serializer) : base(template, serializer)
		{
			this.Version = template.Version;
		}


		// Upgrade.
		protected override void OnUpgrade(int oldVersion)
		{
			this.IsOnUpgradeCalled = true;
			this.OldVersion = oldVersion;
		}


		// Version.
		protected override int Version { get; }
	}
}
