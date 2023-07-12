using Android.Content;

namespace CarinaStudio.Android;

/// <summary>
/// Interface for object which is related to a <see cref="Context"/>.
/// </summary>
public interface IContextObject
{
    /// <summary>
    /// Get related <see cref="Context"/>.
    /// </summary>
    Context Context { get; }
}