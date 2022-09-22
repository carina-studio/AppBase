using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.CoreFoundation
{
    /// <summary>
    /// Range.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CFRange
    {
        /// <summary>
        /// Initialize <see cref="CFRange"/> structure.
        /// </summary>
        /// <param name="location">Start position.</param>
        /// <param name="length">Length.</param>
        public CFRange(long location, long length)
        {
            this.Location = location;
            this.Length = length;
        }


        /// <summary>
        /// Start position.
        /// </summary>
        public long Location;


        /// <summary>
        /// Length.
        /// </summary>
        public long Length;
    }
}