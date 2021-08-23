using NUnit.Framework;
using CarinaStudio.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace CarinaStudio.Net
{
	/// <summary>
	/// Tests of <see cref="WebRequestStreamProvider"/>.
	/// </summary>
	[TestFixture]
	class WebRequestStreamProviderTests : BaseStreamProviderTests
	{
		// Fields.
		readonly HttpListener httpListener = new HttpListener();
		volatile byte[]? preparedResponseData;
		volatile byte[]? receivedRequestData;


		// Create instance.
		protected override IStreamProvider CreateInstance(byte[] data)
		{
			this.preparedResponseData = data;
			return new WebRequestStreamProvider(new Uri("http://localhost:9521/"), WebRequestMethods.Http.Post);
		}


		// Dispose HTTP listener.
		[OneTimeTearDown]
		public void DisposeHttpListener()
		{
			this.httpListener.Stop();
		}


		// Get written data.
		protected override byte[] GetWrittenData(IStreamProvider provider) => this.receivedRequestData.AsNonNull();


		/// <summary>
		/// Setup HTTP listener.
		/// </summary>
		[OneTimeSetUp]
		public void SetupHttpListener()
		{
			this.httpListener.Prefixes.Add("http://localhost:9521/");
			this.httpListener.Start();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				while (true)
				{
					// wait for connection
					var context = (HttpListenerContext?)null;
					try
					{
						context = this.httpListener.GetContext();
					}
					catch
					{
						if (this.httpListener?.IsListening != true)
							break;
						throw;
					}

					// get request data
					this.receivedRequestData = context.Request.InputStream.Use(it => it.ReadAllBytes());

					// response
					var responseBuffer = this.preparedResponseData ?? new byte[0];
					context.Response.Let(response =>
					{
						response.ContentLength64 = responseBuffer.Length;
						response.ContentEncoding = Encoding.UTF8;
						using var stream = response.OutputStream;
						stream.Write(responseBuffer, 0, responseBuffer.Length);
						stream.Flush();
					});
				}
			});
		}
	}
}
