namespace Stross.Application.Slices.Thumbnail.ResponseModels;

public sealed record GetThumbnailResponse(string ThumbnailLocation, string ContentType);
