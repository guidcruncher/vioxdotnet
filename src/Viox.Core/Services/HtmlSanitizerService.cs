using System.Net;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;

namespace Viox.Core.Services;

/// <summary>
/// Implementation of <see cref="IHtmlSanitizerService"/> utilizing HtmlAgilityPack for safe DOM parsing.
/// </summary>
public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizerOptions _options;
    private readonly ILogger<HtmlSanitizerService> _logger;

    public HtmlSanitizerService(
        IOptions<HtmlSanitizerOptions> options,
        ILogger<HtmlSanitizerService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
    }

    public string StripHtml(string? htmlInput)
    {
        if (string.IsNullOrWhiteSpace(htmlInput))
        {
            return string.Empty;
        }

        try
        {
            var document = new HtmlDocument();
            document.LoadHtml(htmlInput);

            var plainText = document.DocumentNode.InnerText;

            if (_options.DecodeHtmlEntities)
            {
                plainText = WebUtility.HtmlDecode(plainText);
            }

            if (_options.TrimWhitespace)
            {
                plainText = plainText.Trim();
            }

            return plainText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse and remove HTML tags from the input string.");
            throw;
        }
    }
}
