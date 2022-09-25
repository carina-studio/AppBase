using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// NSRange.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NSRange
    {
        /// <summary>
        /// Initialize <see cref="NSRange"/> structure.
        /// </summary>
        /// <param name="location">Start location.</param>
        /// <param name="length">Length.</param>
        public NSRange(nint location, nint length)
        {
            this.Location = location;
            this.Length = length;
        }


        /// <summary>
        /// Start location.
        /// </summary>
        public nint Location;


        /// <summary>
        /// Length.
        /// </summary>
        public nint Length;
    }
}