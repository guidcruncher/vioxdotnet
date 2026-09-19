using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Viox.Client.Librespot.Models;

namespace Viox.Client.Librespot.Services;

public sealed class LibrespotManager
{
    public async Task UpdateAccessToken(string username, string token, CancellationToken cancellationToken = default)
    {
        string template = "/etc/golibrespot/config-template.yml";
        string configFile = "/data/golibrespot/config.yml";
        string state = "/data/golibrespot/state.json";
        string content = string.Empty;
        Encoding encoding = Encoding.GetEncoding("UTF-8");
        content = await File.ReadAllTextAsync(template, encoding, cancellationToken);
        string result = content
                    .Replace("[USERNAME]", username, StringComparison.Ordinal)
                    .Replace("[ACCESS_TOKEN]", token, StringComparison.Ordinal);
        await File.WriteAllTextAsync(configFile, result, encoding, cancellationToken);

        content = await File.ReadAllTextAsync(state, encoding, cancellationToken);
        DeviceEventPayload? payload = JsonSerializer.Deserialize<DeviceEventPayload>(content);
        if (payload is not null)
        {
            payload.Credentials.Username = username;
            payload.Credentials.Data = token;
            await File.WriteAllTextAsync(state, JsonSerializer.Serialize(payload, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                WriteIndented = true
            }), encoding, cancellationToken);
        }
    }
}
