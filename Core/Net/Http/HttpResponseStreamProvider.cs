using CarinaStudio.IO;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Net.Http;

/// <summary>
/// Implementation of <see cref="IStreamProvider"/> which send HTTP request and get <see cref="Stream"/> of response.
/// </summary>
public class HttpResponseStreamProvider : BaseDisposable, IStreamProvider
{
    // Static fields.
    static readonly HttpClient DefaultClient = new();
    
    
    // Fields.
    readonly HttpClient client;
    readonly Func<HttpRequestMessage> requestProvider;


    /// <summary>
    /// Initialize new <see cref="HttpResponseStreamProvider"/> instance.
    /// <param name="request"><see cref="HttpRequestMessage"/>.</param>
    /// <param name="timeout">Timeout of getting HTTP response.</param>
    /// <param name="messageHandler">The HTTP handler stack to use for sending requests.</param>
    /// <param name="maxResponseContentBufferSize">Maximum buffer size for response content.</param>
    /// </summary>
    public HttpResponseStreamProvider(HttpRequestMessage request, TimeSpan? timeout = null, HttpMessageHandler? messageHandler = null, long? maxResponseContentBufferSize = null) :
        this(() => request, timeout, messageHandler, maxResponseContentBufferSize) { }
    
    
    /// <summary>
    /// Initialize new <see cref="HttpResponseStreamProvider"/> instance.
    /// <param name="requestProvider">Function to provide <see cref="HttpRequestMessage"/>.</param>
    /// <param name="timeout">Timeout of getting HTTP response.</param>
    /// <param name="messageHandler">The HTTP handler stack to use for sending requests.</param>
    /// <param name="maxResponseContentBufferSize">Maximum buffer size for response content.</param>
    /// </summary>
    public HttpResponseStreamProvider(Func<HttpRequestMessage> requestProvider, TimeSpan? timeout = null, HttpMessageHandler? messageHandler = null, long? maxResponseContentBufferSize = null)
    {
        if (messageHandler is not null)
        {
            this.client = new(messageHandler);
            this.client.Let(it =>
            {
                if (maxResponseContentBufferSize.HasValue)
                    it.MaxResponseContentBufferSize = maxResponseContentBufferSize.Value;
                if (timeout.HasValue)
                    it.Timeout = timeout.Value;
            });
        }
        else if (maxResponseContentBufferSize.HasValue)
        {
            this.client = new();
            this.client.Let(it =>
            {
                it.MaxResponseContentBufferSize = maxResponseContentBufferSize.Value;
                if (timeout.HasValue)
                    it.Timeout = timeout.Value;
            });
        }
        else if (timeout.HasValue)
        {
            this.client = new()
            {
                Timeout = timeout.Value
            };
        }
        else
            this.client = DefaultClient;
        this.requestProvider = requestProvider;
    }


    /// <inheritdoc/>
    public bool CheckStreamAccess(StreamAccess access) =>
        access == StreamAccess.Read;
    
    
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !ReferenceEquals(this.client, DefaultClient))
            this.client.Dispose();
    }


    /// <summary>
    /// Check whether the internal <see cref="HttpClient"/> is default instance or not.
    /// </summary>
    internal bool IsDefaultHttpClient => ReferenceEquals(this.client, DefaultClient);

    
    /// <inheritdoc/>
    public async Task<Stream> OpenStreamAsync(StreamAccess access, CancellationToken token)
    {
        // check state
        this.VerifyDisposed();
        
        // get request
        using var request = this.requestProvider();
        
        // send response and get stream of content
        var response = await this.client.SendAsync(request, token);
        return response.Content.ReadAsStream(token);
    }
}