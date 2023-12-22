using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.IO
{
	/// <summary>
	/// Object to provide <see cref="Stream"/>.
	/// </summary>
	public interface IStreamProvider
	{
		/// <summary>
		/// Check whether given access to <see cref="Stream"/> is supported by this provider or not.
		/// </summary>
		/// <param name="access">Access to stream.</param>
		/// <returns>True if given combination of access is supported.</returns>
		bool CheckStreamAccess(StreamAccess access);


		/// <summary>
		/// Open stream asynchronously.
		/// </summary>
		/// <param name="access">Desired access to stream.</param>
		/// <param name="token">Cancellation token.</param>
		/// <returns>Task of opening stream.</returns>
		Task<Stream> OpenStreamAsync(StreamAccess access, CancellationToken token);
	}


	/// <summary>
	/// Extensions for <see cref="IStreamProvider"/>.
	/// </summary>
	public static class StreamProviderExtensions
	{
		/// <summary>
		/// Check whether opening <see cref="Stream"/> to read data is supported by <see cref="IStreamProvider"/> or not.
		/// </summary>
		/// <param name="provider"><see cref="IStreamProvider"/>.</param>
		/// <returns>True if opening <see cref="Stream"/> to read data is supported.</returns>
		public static bool CanOpenRead(this IStreamProvider provider) => provider.CheckStreamAccess(StreamAccess.Read);


		/// <summary>
		/// Check whether opening <see cref="Stream"/> to write data is supported by <see cref="IStreamProvider"/> or not.
		/// </summary>
		/// <param name="provider"><see cref="IStreamProvider"/>.</param>
		/// <returns>True if opening <see cref="Stream"/> to write data is supported.</returns>
		public static bool CanOpenWrite(this IStreamProvider provider) => provider.CheckStreamAccess(StreamAccess.Write);


		/// <summary>
		/// Open stream asynchronously.
		/// </summary>
		/// <param name="provider"><see cref="IStreamProvider"/>.</param>
		/// <param name="access">Desired access to stream.</param>
		/// <returns>Task of opening stream.</returns>
		public static Task<Stream> OpenStreamAsync(this IStreamProvider provider, StreamAccess access) =>
			provider.OpenStreamAsync(access, new CancellationToken());


		/// <summary>
		/// Open stream and create <see cref="StreamReader"/> asynchronously.
		/// </summary>
		/// <param name="provider"><see cref="IStreamProvider"/>.</param>
		/// <param name="access">Desired access to stream.</param>
		/// <param name="encoding">Text encoding.</param>
		/// <returns>Task of opening <see cref="StreamReader"/>.</returns>
		public static Task<StreamReader> OpenStreamReaderAsync(this IStreamProvider provider, StreamAccess access, Encoding encoding) =>
			OpenStreamReaderAsync(provider, access, encoding, new CancellationToken());


		/// <summary>
		/// Open stream and create <see cref="StreamReader"/> asynchronously.
		/// </summary>
		/// <param name="provider"><see cref="IStreamProvider"/>.</param>
		/// <param name="access">Desired access to stream.</param>
		/// <param name="encoding">Text encoding.</param>
		/// <param name="cancellationToken"></param>
		/// <returns>Task of opening <see cref="StreamReader"/>.</returns>
		public static async Task<StreamReader> OpenStreamReaderAsync(this IStreamProvider provider, StreamAccess access, Encoding encoding, CancellationToken cancellationToken)
		{
			var stream = await provider.OpenStreamAsync(access, cancellationToken);
			return new StreamReader(stream, encoding);
		}
	}
}
