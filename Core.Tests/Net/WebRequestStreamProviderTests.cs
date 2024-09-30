using NUnit.Framework;
using CarinaStudio.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Net
{
	/// <summary>
	/// Tests of <see cref="WebRequestStreamProvider"/>.
	/// </summary>
	[TestFixture]
	class WebRequestStreamProviderTests : BaseStreamProviderTests
	{
		// Constants.
		const string HostUri = "http://localhost:9521/";
		const string CustomHeaderKey = "appbase-custom-header";
		
		// Fields.
		readonly HttpListener httpListener = new();
		volatile byte[]? preparedResponseData;
		volatile string? receivedCustomHeaderValue;
		volatile byte[]? receivedRequestData;


		/// <summary>
		/// Test for sending custom header.
		/// </summary>
		[Test]
		public async Task CustomHeaderTest()
		{
			var headers = new Dictionary<string, string>
			{
				{ CustomHeaderKey, "test" }
			};
			var provider = new WebRequestStreamProvider(new Uri(HostUri), headers: headers, method: WebRequestMethods.Http.Post);
			await using var stream = await provider.OpenStreamAsync(StreamAccess.Read);
			Assert.That(this.receivedCustomHeaderValue == "test");
		}


		// Create instance.
		protected override IStreamProvider CreateInstance(byte[] data)
		{
			this.preparedResponseData = data;
			return new WebRequestStreamProvider(new Uri(HostUri), WebRequestMethods.Http.Post);
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
			this.httpListener.Prefixes.Add(HostUri);
			this.httpListener.Start();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				while (true)
				{
					// wait for connection
					HttpListenerContext? context;
					try
					{
						context = this.httpListener.GetContext();
					}
					catch
					{
						if (this.httpListener.IsListening != true)
							break;
						throw;
					}
					
					// get custom header
					this.receivedCustomHeaderValue = context.Request.Headers[CustomHeaderKey];

					// get request data
					this.receivedRequestData = context.Request.InputStream.Use(it => it.ReadAllBytes());

					// response
					var responseBuffer = this.preparedResponseData ?? Array.Empty<byte>();
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
