using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stross.Application.Slices.Thumbnail.InputModels;
using Stross.Application.Slices.Thumbnail.ResponseModels;
using Stross.Config;
using Stross.Domain.Entities;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;
using ValidationException = Stross.Exception.Exceptions.ValidationException;

namespace Stross.Application.Slices.Thumbnail.Queries;

public sealed record GetThumbnailQuery(GetThumbnailInput Input) : IRequest<GetThumbnailResponse>;

internal sealed class GetThumbnailQueryValidator : AbstractValidator<GetThumbnailQuery>
{
    public GetThumbnailQueryValidator(IValidator<GetThumbnailInput> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class GetThumbnailQueryHandler : IRequestHandler<GetThumbnailQuery, GetThumbnailResponse>
{
    private readonly StrossContext _context;
    private readonly StrossStorageConfig _storageConfig;

    public GetThumbnailQueryHandler(StrossContext context, IOptionsSnapshot<StrossStorageConfig> storageConfigSnapshot)
    {
        _context = context;
        _storageConfig = storageConfigSnapshot.Value;
    }

    public async Task<GetThumbnailResponse> Handle(GetThumbnailQuery request, CancellationToken cancellationToken)
    {
        string thumbnailLocation = request.Input.Type switch
        {
            ThumbnailType.Creator => await GetCreatorThumbnailAsync(request.Input.Id, cancellationToken),
            ThumbnailType.MusicTrack => await GetMusicTrackThumbnailAsync(request.Input.Id, cancellationToken),
            _ => throw new ValidationException("Invalid thumbnail type specified")
        };

        string contentType = GetContentTypeFromPath(thumbnailLocation);

        string fullThumbnailLocation = Path.Combine(_storageConfig.BasePath, thumbnailLocation);

        return new GetThumbnailResponse(fullThumbnailLocation, contentType);
    }

    private async Task<string> GetCreatorThumbnailAsync(long creatorId, CancellationToken cancellationToken)
    {
        Creator? creator = await _context.Creators
            .Include(c => c.ExternalCreators)
            .FirstOrDefaultAsync(c => c.Id == creatorId, cancellationToken);

        if (creator is null)
            throw new EntityNotFoundException(nameof(Creator));

        ExternalCreator? externalCreator = creator.ExternalCreators.FirstOrDefault();
        if (externalCreator is null || string.IsNullOrEmpty(externalCreator.ThumbnailLocation))
            throw new EntityNotFoundException(nameof(Creator));

        return externalCreator.ThumbnailLocation;
    }

    private async Task<string> GetMusicTrackThumbnailAsync(long musicTrackId, CancellationToken cancellationToken)
    {
        Domain.Entities.MusicTrack? musicTrack = await _context.MusicTracks
            .FirstOrDefaultAsync(mt => mt.Id == musicTrackId, cancellationToken);

        if (musicTrack is null)
            throw new EntityNotFoundException(nameof(MusicTrack));

        if (string.IsNullOrEmpty(musicTrack.ThumbnailLocation))
            throw new EntityNotFoundException(nameof(MusicTrack));

        return musicTrack.ThumbnailLocation;
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
            _ => "application/octet-stream"
        };
    }
}