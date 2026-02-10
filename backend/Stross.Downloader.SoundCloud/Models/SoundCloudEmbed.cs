using System.Text.Json.Serialization;

namespace Stross.Downloader.SoundCloud.Models;

public record SoundCloudEmbed(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("author_name")] string AuthorName,
    [property: JsonPropertyName("author_url")] string AuthorUrl,
    [property: JsonPropertyName("thumbnail_url")] string ThumbnailUrl
);
