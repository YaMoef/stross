using System.Text.Json.Serialization;

namespace Stross.Downloader.YT.Models;

public record YoutubeVideoMetadata(
    [property:JsonPropertyName("title")]string Title,
    [property:JsonPropertyName("author_name")]string AuthorName,
    [property:JsonPropertyName("author_url")]string AuthorUrl,
    [property:JsonPropertyName("type")]string Type,
    [property:JsonPropertyName("height")]int Height,
    [property:JsonPropertyName("width")]int Width,
    [property:JsonPropertyName("version")]string Version,
    [property:JsonPropertyName("provider_name")]string ProviderName,
    [property:JsonPropertyName("provider_url")]string ProviderUrl,
    [property:JsonPropertyName("thumbnail_height")]int ThumbnailHeight,
    [property:JsonPropertyName("thumbnail_width")]int ThumbnailWidth,
    [property:JsonPropertyName("thumbnail_url")]string ThumbnailUrl,
    [property:JsonPropertyName("html")]string Html
);