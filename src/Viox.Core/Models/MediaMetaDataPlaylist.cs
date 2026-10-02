// File: MediaMetaData.cs
namespace Viox.Core.Models;

public class MediaMetaDataPlaylist
{
     public string Title { get; set ; } = string.Empty;

     public string ImageUrl { get; set; } = string.Empty;

     public List<MediaMetaData> Items { get; set; } = new ();

}
