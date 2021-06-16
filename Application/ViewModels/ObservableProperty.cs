using System;
using System.Collections.Generic;
using System.Threading;

namespace CarinaStudio.ViewModels
{
	/// <summary>
	/// Represent an observable property.
	/// </summary>
	public abstract class ObservableProperty
	{
		// Implementation of IComparer.
		class ComparerImpl : IComparer<ObservableProperty>
		{
			public int Compare(ObservableProperty? x, ObservableProperty? y)
			{
				if (x == null)
				{
					if (y == null)
						return 0;
					return -1;
				}
				if (y == null)
					return 1;
				return (x.Id - y.Id);
			}
		}


		/// <summary>
		/// Default <see cref="IComparer{T}"/> for <see cref="ObservableProperty"/>.
		/// </summary>
		public static readonly IComparer<ObservableProperty> Comparer = new ComparerImpl();


		// Static fields.
		volatile int nextId;


		// Constructor.
		internal ObservableProperty(Type ownerType, string name, Type valueType)
		{
			this.Id = Interlocked.Increment(ref nextId);
			this.Name = name;
			this.OwnerType = ownerType;
			this.ValueType = valueType;
		}


		/// <summary>
		/// Get hash-code.
		/// </summary>
		/// <returns>Hash-code.</returns>
		public override int GetHashCode() => this.Id;


		/// <summary>
		/// Unique ID of property.
		/// </summary>
		public int Id { get; }


		/// <summary>
		/// Name of property.
		/// </summary>
		public string Name { get; }


		/// <summary>
		/// Type of property owner.
		/// </summary>
		public Type OwnerType { get; }


#pragma warning disable CS8601
		/// <summary>
		/// Register a typed property.
		/// </summary>
		/// <typeparam name="TOwner">Type of owner.</typeparam>
		/// <typeparam name="TValue">Type of value.</typeparam>
		/// <param name="name">Name of property.</param>
		/// <param name="defaultValue">Default value.</param>
		/// <param name="coerce">Coercion function.</param>
		/// <param name="validate">Validation function.</param>
		/// <returns></returns>
		public static ObservableProperty<TValue> Register<TOwner, TValue>(string name, TValue defaultValue = default, Func<TValue, TValue>? coerce = null, Predicate<TValue>? validate = null)
		{
			return new ObservableProperty<TValue>(typeof(TOwner), name, defaultValue, coerce, validate);
		}
#pragma warning restore CS8601


		/// <summary>
		/// Type of value.
		/// </summary>
		public Type ValueType { get; }
	}


	/// <summary>
	/// Represent a typed observable property.
	/// </summary>
	/// <typeparam name="T">Type of value.</typeparam>
	public class ObservableProperty<T> : ObservableProperty
	{
		// Constructor.
		internal ObservableProperty(Type ownerType, string name, T defaultValue, Func<T, T>? coerce, Predicate<T>? validate) : base(ownerType, name, typeof(T))
		{
			this.DefaultValue = defaultValue;
			this.CoercionFunction = coerce;
			this.ValidationFunction = validate;
		}


		/// <summary>
		/// Coercion function.
		/// </summary>
		public Func<T, T>? CoercionFunction { get; }


		/// <summary>
		/// Default value.
		/// </summary>
		public T DefaultValue { get; }


		/// <summary>
		/// Validation function.
		/// </summary>
		public Predicate<T>? ValidationFunction { get; }
	}
}
