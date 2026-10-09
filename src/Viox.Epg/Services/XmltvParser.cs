namespace Viox.Epg.Services;

using System.Xml;

using Viox.Epg.Models;

public class XmltvParser
{
    public async Task ParseAsync(
        Stream stream,
        Func<ChannelRecord, Task> onChannelParsed,
        Func<ProgrammeRecord, Task> onProgrammeParsed,
        CancellationToken cancellationToken = default)
    {
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true, IgnoreWhitespace = true });

        while (await reader.ReadAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "channel")
                {
                    var channel = await ParseChannelAsync(reader);
                    if (channel != null)
                    {
                        await onChannelParsed(channel);
                    }
                }
                else if (reader.Name == "programme")
                {
                    var programme = await ParseProgrammeAsync(reader);
                    if (programme != null)
                    {
                        await onProgrammeParsed(programme);
                    }
                }
            }
        }
    }

    private static async Task<ChannelRecord?> ParseChannelAsync(XmlReader reader)
    {
        var channel = new ChannelRecord
        {
            Id = reader.GetAttribute("id") ?? string.Empty
        };

        using var subReader = reader.ReadSubtree();
        while (await subReader.ReadAsync())
        {
            if (subReader.NodeType == XmlNodeType.Element)
            {
                if (subReader.Name == "display-name")
                {
                    channel.DisplayName = await subReader.ReadElementContentAsStringAsync();
                }
                else if (subReader.Name == "url")
                {
                    channel.Url = await subReader.ReadElementContentAsStringAsync();
                }
                else if (subReader.Name == "icon")
                {
                    channel.IconUrl ??= subReader.GetAttribute("src");
                }
            }
        }

        return channel;
    }

    private static async Task<ProgrammeRecord?> ParseProgrammeAsync(XmlReader reader)
    {
        var rawStart = reader.GetAttribute("start") ?? string.Empty;
        var rawStop = reader.GetAttribute("stop") ?? string.Empty;

        var programme = new ProgrammeRecord
        {
            ChannelId = reader.GetAttribute("channel") ?? string.Empty,
            Start = EpgTimeHelper.ToEpoch(rawStart),
            Stop = EpgTimeHelper.ToEpoch(rawStop)
        };

        using var subReader = reader.ReadSubtree();
        while (await subReader.ReadAsync())
        {
            if (subReader.NodeType == XmlNodeType.Element)
            {
                switch (subReader.Name)
                {
                    case "title":
                        programme.Title = await subReader.ReadElementContentAsStringAsync();
                        break;
                    case "sub-title":
                        programme.SubTitle = await subReader.ReadElementContentAsStringAsync();
                        break;
                    case "desc":
                        programme.Description = await subReader.ReadElementContentAsStringAsync();
                        break;
                    case "episode-num":
                        programme.EpisodeNum = await subReader.ReadElementContentAsStringAsync();
                        break;
                    case "icon":
                        programme.IconUrl ??= subReader.GetAttribute("src");
                        break;
                    case "category":
                        var category = await subReader.ReadElementContentAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(category))
                        {
                            programme.Categories.Add(category);
                        }
                        break;
                }
            }
        }

        return programme;
    }
}

