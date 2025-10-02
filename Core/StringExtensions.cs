using CarinaStudio.Threading;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace CarinaStudio;

/// <summary>
/// Extensions for <see cref="string"/>.
/// </summary>
public static unsafe class StringExtensions
{
    /// <summary>
    /// Get the first line of the string.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <param name="includeCarriageReturns">True to include Carriage Returns (CR) in the line.</param>
    /// <returns>The first line of the string.</returns>
    [ThreadSafe]
    public static string GetFirstLine(this string s, bool includeCarriageReturns = false)
    {
        var length = s.Length;
        if (length <= 0)
            return s;
        fixed (char* p = s)
        {
            if (p is null)
                return s;
            var lineBuffer = default(StringBuilder);
            var cPtr = p;
            for (int start = 0, end = 0; end <= length; ++end, ++cPtr)
            {
                var c = *cPtr;
                if (c == '\n')
                {
                    if (lineBuffer is null)
                    {
                        if (start == end)
                            return "";
                        return s[start..end];
                    }
                    if (start < end)
                        lineBuffer.Append(s[start..end]);
                    return lineBuffer.ToString();
                }
                if (c == '\r' && !includeCarriageReturns)
                {
                    if (start < end)
                    {
                        lineBuffer ??= new();
                        lineBuffer.Append(s[start..end]);
                    }
                    start = end + 1;
                }
            }
        }
        return s;
    }
    
    
    /// <summary>
    /// Get the last line of the string.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <param name="includeCarriageReturns">True to include Carriage Returns (CR) in the line.</param>
    /// <returns>The last line of the string.</returns>
    [ThreadSafe]
    public static string GetLastLine(this string s, bool includeCarriageReturns = false)
    {
        var length = s.Length;
        if (length <= 0)
            return s;
        fixed (char* p = s)
        {
            if (p is null)
                return s;
            var cPtr = p + length - 1;
            var hasCR = false;
            var start = length;
            do
            {
                var c = *cPtr--;
                if (c == '\n')
                    break;
                if (c == '\r' && !includeCarriageReturns)
                    hasCR = true;
                --start;
            } while (start > 0);
            if (hasCR)
            {
                cPtr += 2;
                var lineBuffer = new StringBuilder(length - start);
                do
                {
                    var c = *cPtr++;
                    if (c != '\r')
                        lineBuffer.Append(c);
                    ++start;
                } while (start < length);
                return lineBuffer.ToString();
            }
            return s[start..];
        }
    }
    
    
    /// <summary>
    /// Check whether the string consist of more than one lines or not.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <returns>True if the string consist of more than one lines.</returns>
    [ThreadSafe]
    public static bool HasMultipleLines(this string s)
    {
        var length = s.Length;
        if (length <= 0)
            return false;
        fixed (char* p = s)
        {
            if (p is null)
                return false;
            var cPtr = p + length - 1;
            do
            {
                var c = *cPtr--;
                if (c == '\n')
                    return true;
                --length;
            } while (length > 0);
        }
        return false;
    }


    /// <summary>
    /// Count number of lines of the string.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <returns>Number of lines of the string.</returns>
    [ThreadSafe]
    public static int LineCount(this string s)
    {
        var length = s.Length;
        if (length <= 0)
            return 1;
        var count = 1;
        fixed (char* p = s)
        {
            if (p is null)
                return 1;
            var cPtr = p + length - 1;
            do
            {
                var c = *cPtr--;
                if (c == '\n')
                    ++count;
                --length;
            } while (length > 0);
        }
        return count;
    }
    
    
    /// <summary>
    /// Get address of characters in given <see cref="String"/> and perform action.
    /// </summary>
    /// <param name="s"><see cref="String"/>.</param>
    /// <param name="action">Action to perform.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ThreadSafe]
    public static void Pin(this string s, Action<IntPtr> action)
    {
        fixed (char* p = s)
            action(new(p));
    }


    /// <summary>
    /// Get address of characters in given <see cref="String"/> and generate value.
    /// </summary>
    /// <param name="s"><see cref="String"/>.</param>
    /// <param name="func">Function to generate value.</param>
    /// <typeparam name="R">Type of generated value.</typeparam>
    /// <returns>Generated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ThreadSafe]
    public static R Pin<R>(this string s, Func<IntPtr, R> func)
    {
        fixed (char* p = s)
            return func(new(p));
    }
    
    
    /// <summary>
    /// Get address of characters in given <see cref="String"/> and perform action.
    /// </summary>
    /// <param name="s"><see cref="String"/>.</param>
    /// <param name="action">Action to perform.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ThreadSafe]
    public static void Pin(this string s, PointerAction<char> action)
    {
        fixed (char* p = s)
            action(p);
    }


    /// <summary>
    /// Get address of characters in given <see cref="String"/> and generate value.
    /// </summary>
    /// <param name="s"><see cref="String"/>.</param>
    /// <param name="func">Function to generate value.</param>
    /// <typeparam name="R">Type of generated value.</typeparam>
    /// <returns>Generated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [ThreadSafe]
    public static R Pin<R>(this string s, PointerInFunc<char, R> func)
    {
        fixed (char* p = s)
            return func(p);
    }


    /// <summary>
    /// Remove all line breaks from the string.
    /// </summary>
    /// <param name="s">The string.</param>
    /// <param name="removeCarriageReturns">True to remove Carriage Returns (CR) also.</param>
    /// <returns>The string without line breaks.</returns>
    [ThreadSafe]
    public static string RemoveLineBreaks(this string s, bool removeCarriageReturns = true)
    {
        var length = s.Length;
        if (length <= 0)
            return s;
        var newStringBuffer = default(StringBuilder);
        fixed (char* p = s)
        {
            if (p is null)
                return s;
            var segmentStart = 0;
            var segmentEnd = 0;
            var cPtr = p;
            do
            {
                var c = *cPtr++;
                if (c == '\n' || (c == '\r' && removeCarriageReturns))
                {
                    if (segmentStart < segmentEnd)
                    {
                        newStringBuffer ??= new(length - 1);
                        newStringBuffer.Append(s[segmentStart..segmentEnd]);
                    }
                    ++segmentEnd;
                    segmentStart = segmentEnd;
                }
                else
                    ++segmentEnd;
            } while (segmentEnd < length);
            if (segmentStart > 0 && segmentStart < segmentEnd)
            {
                newStringBuffer ??= new(segmentEnd - segmentStart);
                newStringBuffer.Append(s[segmentStart..segmentEnd]);
            }
            else if (newStringBuffer is null && segmentStart == segmentEnd)
                return "";
        }
        return newStringBuffer?.ToString() ?? s;
    }
}