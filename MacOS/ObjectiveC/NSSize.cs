using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// NSSize.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NSSize
    {
        /// <summary>
        /// Initialize <see cref="NSSize"/> structure.
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public NSSize(double width, double height)
        {
            this.Width = width;
            this.Height = height;
        }


        /// <summary>
        /// Width.
        /// </summary>
        public double Width;


        /// <summary>
        /// Height.
        /// </summary>
        public double Height;


        /// <inheritdoc/>
        public override string ToString() =>
            $"{Width}x{Height}";
    }
}