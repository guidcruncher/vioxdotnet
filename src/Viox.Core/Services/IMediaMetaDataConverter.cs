using Viox.Core.Models;

namespace Viox.Core.Services;

public interface IMediaMetaDataConverter<in TSource> : IMediaMetaDataConverterBase
{
    MediaMetaData Convert(TSource input);
}
