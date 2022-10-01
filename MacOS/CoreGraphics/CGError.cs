using System;
namespace CarinaStudio.MacOS.CoreGraphics;

#pragma warning disable CS1591

/// <summary>
/// CGError.
/// </summary>
public enum CGError : int
{
    Success = 0,
    Failure = 1000,
    IllegalArgument = 1001,
    InvalidConnection = 1002,
    InvalidContext = 1003,
    CannotComplete = 1004,
    NotImplemented = 1006,
    RangeCheck = 1007,
    TypeCheck = 1008,
    InvalidOperation = 1010,
    NoneAvailable = 1011,
}


/// <summary>
/// Extensions for CGError.
/// </summary>
static class CGErrorExtensions
{
    public static Exception ToException(this CGError error) => error switch
    {
        CGError.IllegalArgument => new ArgumentException(),
        CGError.InvalidOperation => new InvalidOperationException(),
        CGError.NotImplemented => new NotImplementedException(),
        CGError.RangeCheck => new ArgumentOutOfRangeException(),
        _ => new Exception($"Error of Core Graphics occurred: {(int)error}"),
    };
}
