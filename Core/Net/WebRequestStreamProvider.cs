using CarinaStudio.IO;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Net
{
	/// <summary>
	/// Implementation of <see cref="IStreamProvider"/> based-on <see cref="WebRequest"/> and <see cref="WebResponse"/>.
	/// </summary>
	public class WebRequestStreamProvider : IStreamProvider
	{
		// Stream for request.
		class RequestStream : StreamWrapper
		{
			// Fields.
			readonly WebRequest request;

			// Constructor.
			public RequestStream(WebRequest request) : base(request.GetRequestStream())
			{
				this.request = request;
			}

			// Dispose.
			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				using var _ = this.request.GetResponse();
			}
		}


		// Stream for response.
		class ResponseStream : StreamWrapper
		{
			// Fields.
			readonly WebResponse response;

			// Constructor.
			public ResponseStream(WebResponse response) : base(response.GetResponseStream())
			{
				this.response = response;
			}

			// Dispose.
			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				this.response?.Dispose(); // In case of error occurred in constructor
			}

			// Get length;
			public override long Length => response.ContentLength;
		}


		// Fields.
		readonly ICredentials? credentials;
		readonly string? method;


		/// <summary>
		/// Initialize new <see cref="WebRequestStreamProvider"/> instance.
		/// </summary>
		/// <param name="requestUri">Request URI.</param>
		/// <param name="method">Protocol method.</param>
		/// <param name="credentials">Crdentials.</param>
		public WebRequestStreamProvider(Uri requestUri, string? method = null, ICredentials? credentials = null)
		{
			this.credentials = credentials;
			this.method = method ?? requestUri.Scheme switch
			{
				"file" => WebRequestMethods.File.DownloadFile,
				"ftp" => WebRequestMethods.Ftp.DownloadFile,
				"http" => WebRequestMethods.Http.Get,
				"https" => WebRequestMethods.Http.Get,
				_ => null,
			};
			this.RequestUri = requestUri;
		}


		/// <summary>
		/// Check whether given access to <see cref="Stream"/> is supported by this provider or not.
		/// </summary>
		/// <param name="access">Access to stream.</param>
		/// <returns>True if given combination of access is supported.</returns>
		public bool CheckStreamAccess(StreamAccess access)
		{
			// check reading
			if ((access & StreamAccess.Read) != 0)
			{
				var isReadable = this.RequestUri.Scheme switch
				{
					"file" => this.method == WebRequestMethods.File.DownloadFile,
					"ftp" => this.method switch
					{
						WebRequestMethods.Ftp.DownloadFile => true,
						WebRequestMethods.Ftp.GetDateTimestamp => true,
						WebRequestMethods.Ftp.GetFileSize => true,
						WebRequestMethods.Ftp.ListDirectory => true,
						WebRequestMethods.Ftp.ListDirectoryDetails => true,
						_ => false,
					},
					"http" => true,
					"https" => true,
					_ => true,
				};
				if (!isReadable)
					return false;
			}

			// check writing
			if ((access & StreamAccess.Write) != 0)
			{
				var isWritable = this.RequestUri.Scheme switch
				{
					"file" => this.method == WebRequestMethods.File.UploadFile,
					"ftp" => this.method switch
					{
						WebRequestMethods.Ftp.AppendFile => true,
						WebRequestMethods.Ftp.UploadFile => true,
						WebRequestMethods.Ftp.UploadFileWithUniqueName => true,
						_ => false,
					},
					"http" => this.method == WebRequestMethods.Http.Post,
					"https" => this.method == WebRequestMethods.Http.Post,
					_ => true,
				};
				if (!isWritable)
					return false;
			}

			// access is supported
			return true;
		}


		/// <summary>
		/// Open stream asynchronously.
		/// </summary>
		/// <param name="access">Desired access to stream.</param>
		/// <param name="token">Cancellation token.</param>
		/// <returns>Task of opening stream.</returns>
		public Task<Stream> OpenStreamAsync(StreamAccess access, CancellationToken token) => Task.Run(() =>
		{
			// check parameter
			var isReadNeeded = (access & StreamAccess.Read) != 0;
			var isWriteNeeded = (access & StreamAccess.Write) != 0;
			if (!this.CheckStreamAccess(access))
				throw new ArgumentException("Invalid access to stream.");
			if (!isReadNeeded && !isWriteNeeded)
				throw new ArgumentException("Invalid access to stream.");
			if (isReadNeeded && isWriteNeeded)
				throw new ArgumentException("Invalid access to stream.");

			// create request
			var request = WebRequest.Create(this.RequestUri).Also(it =>
			{
				if (this.credentials != null)
					it.Credentials = this.credentials;
				if (this.method != null)
					it.Method = this.method;
			});
			if (isWriteNeeded)
				return (Stream)new RequestStream(request);

			// cancellation check
			if (token.IsCancellationRequested)
				throw new TaskCanceledException();

			// get response
			var response = (WebResponse?)null;
			try
			{
				response = request.GetResponse();
			}
			catch
			{
				if (token.IsCancellationRequested)
					throw new TaskCanceledException();
				throw;
			}

			// cancellation check
			if (token.IsCancellationRequested)
				throw new TaskCanceledException();

			// create response stream
			try
			{
				return new ResponseStream(response);
			}
			catch
			{
				response.Dispose();
				throw;
			}
		});


		/// <summary>
		/// Get URI of request.
		/// </summary>
		public Uri RequestUri { get; }
	}
}
