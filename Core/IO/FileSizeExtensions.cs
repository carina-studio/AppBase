using System;
using System.Text;

namespace CarinaStudio.IO
{
	/// <summary>
	/// Extensions for file size.
	/// </summary>
	public static class FileSizeExtensions
	{
		// Constants.
		const long KILO_BYTES = 1L << 10;
		const long MEGA_BYTES = 1L << 20;
		const long GIGA_BYTES = 1L << 30;
		const long TERA_BYTES = 1L << 40;
		const long PETA_BYTES = 1L << 50;


		/// <summary>
		/// Convert byte size to readable file size string.
		/// </summary>
		/// <param name="bytes">File size in bytes.</param>
		/// <param name="decimalPlaces">Decimal places.</param>
		/// <returns>Readable file size string.</returns>
		public static string ToFileSizeString(this int bytes, int decimalPlaces = 1) =>
			ToFileSizeString((long)bytes, decimalPlaces);


		/// <summary>
		/// Convert byte size to readable file size string.
		/// </summary>
		/// <param name="bytes">File size in bytes.</param>
		/// <param name="decimalPlaces">Decimal places.</param>
		/// <returns>Readable file size string.</returns>
		public static string ToFileSizeString(this long bytes, int decimalPlaces = 1)
		{
			if (decimalPlaces < 0)
				throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
			var format = decimalPlaces switch
			{
				0 => "0",
				1 => "0.0",
				2 => "0.00",
				3 => "0.000",
				4 => "0.0000",
				_ => new StringBuilder("0.").Also(it=>
				{
					for (var i = decimalPlaces; i > 0; --i)
						it.Append('0');
				}).ToString(),
			};
			var absBytes = Math.Abs(bytes);
			if (absBytes < KILO_BYTES)
				return bytes + " B";
			if (absBytes < MEGA_BYTES)
				return ((double)bytes / KILO_BYTES).ToString(format) + " KB";
			if (absBytes < GIGA_BYTES)
				return ((double)bytes / MEGA_BYTES).ToString(format) + " MB";
			if (absBytes < TERA_BYTES)
				return ((double)bytes / GIGA_BYTES).ToString(format) + " GB";
			if (absBytes < PETA_BYTES)
				return ((double)bytes / TERA_BYTES).ToString(format) + " TB";
			return ((double)bytes / PETA_BYTES).ToString(format) + " PB";
		}
	}
}
