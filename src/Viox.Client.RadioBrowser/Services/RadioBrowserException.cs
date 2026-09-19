using System.Net;

namespace Viox.Client.RadioBrowser.Services;

/// <summary>
/// Thrown when a Radio Browser API call fails.
/// </summary>
public sealed class RadioBrowserException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RadioBrowserException"/> class.
    /// </summary>
    /// <param name="message">Human-readable error description.</param>
    /// <param name="statusCode">HTTP status returned by the server, when available.</param>
    /// <param name="requestUri">Request URI that failed, when available.</param>
    /// <param name="responseBody">Raw response body, when available.</param>
    /// <param name="innerException">Underlying exception, when available.</param>
    public RadioBrowserException(
        string message,
        HttpStatusCode? statusCode = null,
        Uri? requestUri = null,
        string? responseBody = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        RequestUri = requestUri;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Gets the HTTP status code returned by the API, if the failure was an HTTP error.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets the request URI that produced the failure.
    /// </summary>
    public Uri? RequestUri { get; }

    /// <summary>
    /// Gets the raw response body, if one was received.
    /// </summary>
    public string? ResponseBody { get; }
}
