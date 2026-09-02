using CarinaStudio.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Net.Http
{
	/// <summary>
	/// Tests of <see cref="HttpResponseStreamProvider"/>.
	/// </summary>
	[TestFixture]
	public class HttpResponseStreamProviderTests : BaseStreamProviderTests
	{
		// Constants.
		const int BasePort = 9520;


		// Static fields.
		// Port is bound to major version of runtime, which is same as major version of target framework, so that
		// test cases of all target frameworks can run at the same time without conflicting on the port.
		static readonly string HostUri = $"http://localhost:{BasePort + Environment.Version.Major}/";


		// Fields.
		readonly HttpListener httpListener = new();
		volatile bool isHttpListenerStopping;
		volatile byte[]? preparedResponseData;
		volatile byte[]? receivedRequestData;


		/// <summary>
		/// Test for opening multiple streams concurrently.
		/// </summary>
		[Test]
		public async Task ConcurrencyTest()
		{
			// prepare
			var requestUris = new[]
			{
				new Uri("https://amazon.com/"),
				new Uri("https://apple.com/"),
				new Uri("https://facebook.com/"),
				new Uri("https://google.com/"),
				//new Uri("https://microsoft.com/"),
			};
			
			// use default HttpClient
			var providers = new List<HttpResponseStreamProvider>();
			var tasks = new List<Task>();
			for (var i = 0; i < 5; ++i)
			{
				foreach (var requestUri in requestUris)
				{
					var provider = new HttpResponseStreamProvider(() => new HttpRequestMessage
					{
						Method = HttpMethod.Get,
						RequestUri = requestUri,
					});
					Assert.That(provider.IsDefaultHttpClient);
					providers.Add(provider);
					tasks.Add(provider.OpenStreamAsync(StreamAccess.Read));
				}
			}
			await Task.WhenAll(tasks);
			foreach (var provider in providers)
				provider.Dispose();
			providers.Clear();
			
			// use dedicated HttpClient
			var random = new Random();
			for (var i = 0; i < 5; ++i)
			{
				foreach (var requestUri in requestUris)
				{
					var provider = new HttpResponseStreamProvider(() => new HttpRequestMessage
					{
						Method = HttpMethod.Get,
						RequestUri = requestUri,
					}, timeout: TimeSpan.FromSeconds(random.Next(10, 31)));
					Assert.That(!provider.IsDefaultHttpClient);
					providers.Add(provider);
					tasks.Add(provider.OpenStreamAsync(StreamAccess.Read));
				}
			}
			await Task.WhenAll(tasks);
			foreach (var provider in providers)
				provider.Dispose();
			providers.Clear();
		}


		// Create instance.
		protected override IStreamProvider CreateInstance(byte[] data)
		{
			this.preparedResponseData = data;
			return new HttpResponseStreamProvider(() => new HttpRequestMessage
			{
				Method = HttpMethod.Post,
				RequestUri = new(HostUri),
			});
		}


		// Dispose HTTP listener.
		[OneTimeTearDown]
		public void DisposeHttpListener()
		{
			this.isHttpListenerStopping = true;
			this.httpListener.Stop();
		}


		/// <summary>
		/// Test for disposing instances.
		/// </summary>
		[Test]
		public async Task DisposingTest()
		{
			var provider = new HttpResponseStreamProvider(() => new HttpRequestMessage
			{
				Method = HttpMethod.Get,
				RequestUri = new("https://google.com/"),
			});
			await using var stream1 = await provider.OpenStreamAsync(StreamAccess.Read);
			await using var stream2 = await provider.OpenStreamAsync(StreamAccess.Read);
			provider.Dispose();
			try
			{
				await using var stream3 = await provider.OpenStreamAsync(StreamAccess.Read);
				throw new AssertionException("ObjectDisposedException exception should be thrown.");
			}
			catch (ObjectDisposedException)
			{ }
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
					HttpListenerContext context;
					try
					{
						context = this.httpListener.GetContext();
					}
					catch (Exception ex)
					{
						// Stop() aborts the pending GetContext() call, and IsListening can still report
						// true at that moment, so the explicit flag is what distinguishes a shutdown from
						// a real error. Never rethrow here: this runs on a thread pool thread, where an
						// unhandled exception kills the test host and aborts the whole run.
						if (!this.isHttpListenerStopping)
							Console.Error.WriteLine($"Unable to accept HTTP request: {ex.Message}");
						break;
					}

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