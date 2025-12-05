using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Config;
using Stross.Domain.Entities;
using Stross.Infrastructure;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetCoverArtQuery(SubsonicGetCoverArtInput Input) : IRequest<SubsonicGetCoverArtResponse>;

internal sealed class SubsonicGetCoverArtQueryValidator : AbstractValidator<SubsonicGetCoverArtQuery>
{
    public SubsonicGetCoverArtQueryValidator(IValidator<SubsonicGetCoverArtInput> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicGetCoverArtQueryHandler : IRequestHandler<SubsonicGetCoverArtQuery, SubsonicGetCoverArtResponse>
{
    private readonly StrossContext _context;
    private readonly StrossStorageConfig _storageConfig;

    public SubsonicGetCoverArtQueryHandler(StrossContext context, IOptionsSnapshot<StrossStorageConfig> storageConfigSnapshot)
    {
        _context = context;
        _storageConfig = storageConfigSnapshot.Value;
    }

    public async Task<SubsonicGetCoverArtResponse> Handle(SubsonicGetCoverArtQuery request, CancellationToken cancellationToken)
    {
        // Parse the ID
        if (!long.TryParse(request.Input.Id, out long entityId))
        {
            throw new Stross.Exception.Exceptions.ValidationException("Invalid ID format");
        }

        // Try to get cover art from different entity types
        // First try music track (most common case for cover art)
        string? thumbnailLocation = await GetMusicTrackThumbnailAsync(entityId, cancellationToken);

        // If not found in music tracks, try creators (artists)
        thumbnailLocation ??= await GetCreatorThumbnailAsync(entityId, cancellationToken);

        if (string.IsNullOrEmpty(thumbnailLocation))
        {
            throw new Exception.Exceptions.EntityNotFoundException("Cover art not found for the specified ID");
        }

        string fullThumbnailLocation = Path.Combine(_storageConfig.BasePath, thumbnailLocation);
        string contentType = GetContentTypeFromPath(thumbnailLocation);
        string fileName = $"cover-{request.Input.Id}{Path.GetExtension(thumbnailLocation)}";

        return new SubsonicGetCoverArtResponse(fullThumbnailLocation, contentType, fileName);
    }

    private async Task<string?> GetMusicTrackThumbnailAsync(long musicTrackId, CancellationToken cancellationToken)
    {
        Domain.Entities.MusicTrack? musicTrack = await _context.MusicTracks
            .FirstOrDefaultAsync(mt => mt.Id == musicTrackId, cancellationToken);

        return musicTrack?.ThumbnailLocation;
    }

    private async Task<string?> GetCreatorThumbnailAsync(long creatorId, CancellationToken cancellationToken)
    {
        Creator? creator = await _context.Creators
            .Include(c => c.ExternalCreators)
            .FirstOrDefaultAsync(c => c.Id == creatorId, cancellationToken);

        ExternalCreator? externalCreator = creator?.ExternalCreators.FirstOrDefault();

        return externalCreator?.ThumbnailLocation;
    }

    private static string GetContentTypeFromPath(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            _ => "image/jpeg" // Default to JPEG for Subsonic compatibility
        };
    }
}