using System;
using System.Collections.Generic;
using System.IO;

namespace CarinaStudio.IO
{
    /// <summary>
    /// Extensions for file name and path.
    /// </summary>
    public static unsafe class PathExtensions
    {
        // Fields.
        private static ISet<char>? InvalidFileNameChars;
        private static ISet<char>? InvalidPathChars;


        /// <summary>
        /// Check whether the given string can represent a valid file name or not.
        /// </summary>
        /// <param name="s">String.</param>
        /// <returns>True if the given string can represent a valid file name.</returns>
        public static bool IsValidFileName(this string? s) =>
            IsValidFileName(s.AsSpan());
        
        
        /// <summary>
        /// Check whether the given string can represent a valid file name or not.
        /// </summary>
        /// <param name="s">String.</param>
        /// <returns>True if the given string can represent a valid file name.</returns>
        public static bool IsValidFileName(this ReadOnlySpan<char> s)
        {
            InvalidFileNameChars ??= new HashSet<char>(Path.GetInvalidFileNameChars());
            fixed (char* p = s)
            {
                var count = s.Length;
                if (p == null || count == 0)
                    return false;
                if (char.IsWhiteSpace(*p) || char.IsWhiteSpace(p[count - 1]))
                    return false;
                var cPtr = p;
                do
                {
                    if (InvalidFileNameChars.Contains(*cPtr))
                        return false;
                    --count;
                    ++cPtr;
                }
                while (count > 0);
            }
            return true;
        }


        /// <summary>
        /// Check whether the given string can represent a valid file path or not.
        /// </summary>
        /// <param name="s">String.</param>
        /// <returns>True if the given string can represent a valid file path.</returns>
        public static bool IsValidFilePath(this string? s) =>
            IsValidFilePath(s.AsSpan());


        /// <summary>
        /// Check whether the given string can represent a valid file path or not.
        /// </summary>
        /// <param name="s">String.</param>
        /// <returns>True if the given string can represent a valid file path.</returns>
        public static bool IsValidFilePath(this ReadOnlySpan<char> s)
        {
            InvalidPathChars ??= new HashSet<char>(Path.GetInvalidPathChars());
            fixed (char* p = s)
            {
                var count = s.Length;
                if (p == null || count == 0)
                    return false;
                if (char.IsWhiteSpace(*p) || char.IsWhiteSpace(p[count - 1]))
                    return false;
                var cPtr = p;
                do
                {
                    if (InvalidPathChars.Contains(*cPtr))
                        return false;
                    --count;
                    ++cPtr;
                }
                while (count > 0);
            }
            return true;
        }
    }
}