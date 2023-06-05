using System;

namespace CarinaStudio
{
	/// <summary>
	/// <see cref="ObservableValue{T}"/> which allows updating value.
	/// </summary>
	/// <typeparam name="T">Type of value.</typeparam>
	public class MutableObservableValue<T> : ObservableValue<T>
	{
		/// <summary>
		/// Initialize new <see cref="MutableObservableValue{T}"/> instance.
		/// </summary>
		/// <param name="initialValue">Initial value.</param>
#pragma warning disable CS8601
		public MutableObservableValue(T initialValue = default) : base(initialValue)
#pragma warning restore CS8601
		{ }


		/// <summary>
		/// Update value.
		/// </summary>
		/// <param name="value">Value to update.</param>
		public void Update(T value) => this.Value = value;
	}


	/// <summary>
	/// Specific <see cref="MutableObservableValue{T}"/> for <see cref="bool"/> value.
	/// </summary>
	public class MutableObservableBoolean : MutableObservableValue<bool>
	{
		/// <summary>
		/// Initialize new <see cref="MutableObservableBoolean"/> instance.
		/// </summary>
		/// <param name="initialValue">Initial value.</param>
		public MutableObservableBoolean(bool initialValue = default) : base(initialValue)
		{ }


		/// <inheritdoc/>
		protected override bool CheckValuesEquality(bool x, bool y) =>
			x == y;


		/// <summary>
		/// False operator.
		/// </summary>
		/// <param name="value"><see cref="MutableObservableBoolean"/>.</param>
		/// <returns>True if value is False.</returns>
		public static bool operator false(MutableObservableBoolean value) => !value.Value;


		/// <summary>
		/// True operator.
		/// </summary>
		/// <param name="value"><see cref="MutableObservableBoolean"/>.</param>
		/// <returns>True if value is True.</returns>
		public static bool operator true(MutableObservableBoolean value) => value.Value;


		/// <summary>
		/// Toggle the <see cref="bool"/> value.
		/// </summary>
		public void Toggle() => this.Value = !this.Value;
	}
	
	
	/// <summary>
	/// Specific <see cref="MutableObservableValue{T}"/> for <see cref="double"/> value.
	/// </summary>
	public class MutableObservableDouble : MutableObservableValue<double>
	{
		/// <summary>
		/// Initialize new <see cref="MutableObservableDouble"/> instance.
		/// </summary>
		/// <param name="initialValue">Initial value.</param>
		public MutableObservableDouble(double initialValue = default) : base(initialValue)
		{ }


		/// <inheritdoc/>
		protected override bool CheckValuesEquality(double x, double y) =>
			Math.Abs(x - y) <= double.Epsilon;


		/// <summary>
		/// Decrease by given value.
		/// </summary>
		/// <param name="value">Value to decrease.</param>
		public void Decrease(double value) => this.Value -= value;


		/// <summary>
		/// Increase by given value.
		/// </summary>
		/// <param name="value">Value to increase.</param>
		public void Increase(double value) => this.Value += value;
	}


	/// <summary>
	/// Specific <see cref="MutableObservableValue{T}"/> for <see cref="int"/> value.
	/// </summary>
	public class MutableObservableInt32 : MutableObservableValue<int>
	{
		/// <summary>
		/// Initialize new <see cref="MutableObservableInt32"/> instance.
		/// </summary>
		/// <param name="initialValue">Initial value.</param>
		public MutableObservableInt32(int initialValue = default) : base(initialValue)
		{ }
		
		
		/// <inheritdoc/>
		protected override bool CheckValuesEquality(int x, int y) =>
			x == y;


		/// <summary>
		/// Decrease by given value.
		/// </summary>
		/// <param name="value">Value to decrease.</param>
		public void Decrease(int value) => this.Value -= value;


		/// <summary>
		/// Increase by given value.
		/// </summary>
		/// <param name="value">Value to increase.</param>
		public void Increase(int value) => this.Value += value;


		/// <summary>
		/// Increment operator.
		/// </summary>
		/// <param name="value"><see cref="MutableObservableInt32"/>.</param>
		/// <returns><see cref="MutableObservableInt32"/>.</returns>
		public static MutableObservableInt32 operator ++(MutableObservableInt32 value)
		{
			value.Value += 1;
			return value;
		}


		/// <summary>
		/// Decrement operator.
		/// </summary>
		/// <param name="value"><see cref="MutableObservableInt32"/>.</param>
		/// <returns><see cref="MutableObservableInt32"/>.</returns>
		public static MutableObservableInt32 operator --(MutableObservableInt32 value)
		{
			value.Value -= 1;
			return value;
		}
	}


	/// <summary>
	/// Specific <see cref="MutableObservableValue{T}"/> for <see cref="long"/> value.
	/// </summary>
	public class MutableObservableInt64 : MutableObservableValue<long>
	{
		/// <summary>
		/// Initialize new <see cref="MutableObservableInt64"/> instance.
		/// </summary>
		/// <param name="initialValue">Initial value.</param>
		public MutableObservableInt64(long initialValue = default) : base(initialValue)
		{ }
		
		
		/// <inheritdoc/>
		protected override bool CheckValuesEquality(long x, long y) =>
			x == y;


		/// <summary>
		/// Decrease by given value.
		/// </summary>
		/// <param name="value">Value to decrease.</param>
		public void Decrease(long value) => this.Value -= value;


		/// <summary>
		/// Increase by given value.
		/// </summary>
		/// <param name="value">Value to increase.</param>
		public void Increase(long value) => this.Value += value;


		/// <summary>
		/// Increment operator.
		/// </summary>
		/// <param name="value"><see cref="MutableObservableInt64"/>.</param>
		/// <returns><see cref="MutableObservableInt64"/>.</returns>
		public static MutableObservableInt64 operator ++(MutableObservableInt64 value)
		{
			value.Value += 1;
			return value;
		}


		/// <summary>
		/// Decrement operator.
		/// </summary>
		/// <param name="value"><see cref="MutableObservableInt64"/>.</param>
		/// <returns><see cref="MutableObservableInt64"/>.</returns>
		public static MutableObservableInt64 operator --(MutableObservableInt64 value)
		{
			value.Value -= 1;
			return value;
		}
	}
	
	
	/// <summary>
	/// Specific <see cref="MutableObservableValue{T}"/> for <see cref="float"/> value.
	/// </summary>
	public class MutableObservableSingle : MutableObservableValue<float>
	{
		/// <summary>
		/// Initialize new <see cref="MutableObservableSingle"/> instance.
		/// </summary>
		/// <param name="initialValue">Initial value.</param>
		public MutableObservableSingle(float initialValue = default) : base(initialValue)
		{ }


		/// <inheritdoc/>
		protected override bool CheckValuesEquality(float x, float y) =>
			Math.Abs(x - y) <= float.Epsilon;


		/// <summary>
		/// Decrease by given value.
		/// </summary>
		/// <param name="value">Value to decrease.</param>
		public void Decrease(float value) => this.Value -= value;


		/// <summary>
		/// Increase by given value.
		/// </summary>
		/// <param name="value">Value to increase.</param>
		public void Increase(float value) => this.Value += value;
	}


	/// <summary>
	/// Specific <see cref="MutableObservableValue{T}"/> for nullable <see cref="string"/> value.
	/// </summary>
	public class MutableObservableString : MutableObservableValue<string?>
	{
		/// <summary>
		/// Initialize new <see cref="MutableObservableString"/> instance.
		/// </summary>
		/// <param name="initialValue">Initial value.</param>
		public MutableObservableString(string? initialValue = default) : base(initialValue)
		{ }


		/// <summary>
		/// Check whether string is null/empty or not.
		/// </summary>
		public bool IsNullOrEmpty => string.IsNullOrEmpty(this.Value);


		/// <summary>
		/// Check whether string is null/blank or not.
		/// </summary>
		public bool IsNullOrWhiteSpace => string.IsNullOrWhiteSpace(this.Value);


		/// <summary>
		/// Get the length of string. Return 0 if string is null.
		/// </summary>
		public int Length => this.Value?.Length ?? 0;


		/// <summary>
		/// Indexer to get character in string.
		/// </summary>
		/// <param name="index">Index of character in string.</param>
		/// <returns>Character in string.</returns>
		public char this[int index] => this.Value?[index] ?? throw new ArgumentOutOfRangeException();
	}
}
