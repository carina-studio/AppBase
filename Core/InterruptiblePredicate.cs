namespace CarinaStudio;

/// <summary>
/// Function to check whether the value meets the condition or not.
/// </summary>
/// <typeparam name="T">Type of value.</typeparam>
/// <param name="value">Value to check.</param>
/// <param name="interrupt">Set to true if the related process should be interrupted.</param>
/// <returns>True if the value meets the condition.</returns>
public delegate bool InterruptiblePredicate<in T>(T value, ref bool interrupt);