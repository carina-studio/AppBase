using System;
using System.Collections;
using System.Collections.Generic;

namespace CarinaStudio.Collections;

/// <summary>
/// Read-only <see cref="ISet{T}"/>.
/// </summary>
/// <typeparam name="T">Type of element.</typeparam>
public class ReadOnlySet<T> : ISet<T>, IReadOnlySet<T>
{
	// Fields.
	readonly ISet<T> set;


	/// <summary>
	/// Initialize new <see cref="ReadOnlySet{T}"/> instance.
	/// </summary>
	/// <param name="set"><see cref="ISet{T}"/> to be wrapped.</param>
	public ReadOnlySet(ISet<T> set)
	{
		this.set = set;
	}


	/// <summary>
	/// Get number of element.
	/// </summary>
	public int Count => this.set.Count;


	/// <summary>
	/// Check whether given element is contained in set or not.
	/// </summary>
	/// <param name="element">Element.</param>
	/// <returns>True if element is contained in set.</returns>
	public bool Contains(T element) => this.set.Contains(element);


	/// <summary>
	/// Copy elements.
	/// </summary>
	/// <param name="array">Array to receive elements.</param>
	/// <param name="arrayIndex">Index of position in <paramref name="array"/> to put first copied element.</param>
	public void CopyTo(T[] array, int arrayIndex) => this.set.CopyTo(array, arrayIndex);


	/// <summary>
	/// Get enumerator to enumerate elements.
	/// </summary>
	/// <returns><see cref="IEnumerator{T}"/>.</returns>
	public IEnumerator<T> GetEnumerator() => this.set.GetEnumerator();
	
	
	/// <summary>
	/// Check whether the set is empty or not.
	/// </summary>
	/// <returns>True if the set is empty.</returns>
	public bool IsEmpty() => this.set.Count <= 0;
	
	
	/// <summary>
	/// Check whether the set is not empty or not.
	/// </summary>
	/// <returns>True if the set is not empty.</returns>
	public bool IsNotEmpty() => this.set.Count > 0;


	/// <summary>
	/// Check whether the set is proper (strict) subset of given collection or not.
	/// </summary>
	/// <param name="other">Given collection.</param>
	/// <returns>True if the set is proper (strict) subset of given collection.</returns>
	public bool IsProperSubsetOf(IEnumerable<T> other) => this.set.IsProperSubsetOf(other);


	/// <summary>
	/// Check whether the set is proper (strict) superset of given collection or not.
	/// </summary>
	/// <param name="other">Given collection.</param>
	/// <returns>True if the set is proper (strict) superset of given collection.</returns>
	public bool IsProperSupersetOf(IEnumerable<T> other) => this.set.IsProperSupersetOf(other);


	/// <summary>
	/// Check whether the set is subset of given collection or not.
	/// </summary>
	/// <param name="other">Given collection.</param>
	/// <returns>True if the set is subset of given collection.</returns>
	public bool IsSubsetOf(IEnumerable<T> other) => this.set.IsSubsetOf(other);


	/// <summary>
	/// Check whether the set is superset of given collection or not.
	/// </summary>
	/// <param name="other">Given collection.</param>
	/// <returns>True if the set is superset of given collection.</returns>
	public bool IsSupersetOf(IEnumerable<T> other) => this.set.IsSupersetOf(other);


	/// <summary>
	/// Check whether the set overlaps with given collection or not.
	/// </summary>
	/// <param name="other">Given collection.</param>
	/// <returns>True if the set overlaps with given collection.</returns>
	public bool Overlaps(IEnumerable<T> other) => this.set.Overlaps(other);


	/// <summary>
	/// Check whether the set contains same elements as given collection or not.
	/// </summary>
	/// <param name="other">Given collection.</param>
	/// <returns>True if the set contains same elements as given collection.</returns>
	public bool SetEquals(IEnumerable<T> other) => this.set.SetEquals(other);


	// Interface implementations.
	void ICollection<T>.Add(T item) => throw new InvalidOperationException();
	void ICollection<T>.Clear() => throw new InvalidOperationException();
	bool ICollection<T>.IsReadOnly => true;
	bool ICollection<T>.Remove(T item) => throw new InvalidOperationException();
	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
	bool ISet<T>.Add(T item) => throw new InvalidOperationException();
	void ISet<T>.ExceptWith(IEnumerable<T> other) => throw new InvalidOperationException();
	void ISet<T>.IntersectWith(IEnumerable<T> other) => throw new InvalidOperationException();
	void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => throw new InvalidOperationException();
	void ISet<T>.UnionWith(IEnumerable<T> other) => throw new InvalidOperationException();
}