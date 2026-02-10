using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stross.Application.Slices.MusicTrack.InputModels;
using Stross.Config;
using Stross.Domain.Entities;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;
using Stross.Infrastructure.Constants;
using Stross.Infrastructure.Services.AudioFileMetadataService;
using Stross.Infrastructure.Services.GrpcService;
using Stross.Infrastructure.Services.GrpcService.Models;
using Stross.Infrastructure.Services.ThumbnailService;

namespace Stross.Application.Slices.MusicTrack.Commands;

public sealed record DownloadMusicTrackCommand(DownloadMusicTrackInput Input) : IRequest<long>;

internal sealed class DownloadMusicTrackCommandValidator : AbstractValidator<DownloadMusicTrackCommand>
{
    public DownloadMusicTrackCommandValidator(IValidator<DownloadMusicTrackInput> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class DownloadMusicTrackCommandHandler : IRequestHandler<DownloadMusicTrackCommand, long>
{
    private readonly StrossContext _context;
    private readonly IGrpcService _grpcService;
    private readonly StrossStorageConfig _storageConfig;
    private readonly IThumbnailService _thumbnailService;
    private readonly IAudioFileMetadataService _audioFileMetadataService;

    public DownloadMusicTrackCommandHandler(StrossContext context, IGrpcService grpcService, IOptionsSnapshot<StrossStorageConfig> storageConfigSnapshot, IThumbnailService thumbnailService,
        IAudioFileMetadataService audioFileMetadataService)
    {
        _context = context;
        _grpcService = grpcService;
        _thumbnailService = thumbnailService;
        _audioFileMetadataService = audioFileMetadataService;
        _storageConfig = storageConfigSnapshot.Value;
    }

    public async Task<long> Handle(DownloadMusicTrackCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Provider? providerToUse =
            await _context.Providers.FirstOrDefaultAsync(p => p.Id == request.Input.ProviderId, cancellationToken);

        if (providerToUse is null)
            throw new EntityNotFoundException(nameof(Provider));

        if (!providerToUse.Enabled)
            throw new ProviderException("This provider is not enabled.");

        string sanitizedUrl = await _grpcService.GetSanitizedSourceUrlAsync(request.Input.SourceUrl, providerToUse, cancellationToken);

        Genre unknownGenre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == Constants.UnknownName, cancellationToken) ??
                             throw new EntityNotFoundException(nameof(Genre));

        Domain.Entities.MusicTrack? musicTrackInDb = await _context.MusicTracks.FirstOrDefaultAsync(m => m.ExternalUrl.ToLower() == sanitizedUrl.ToLower(), cancellationToken);

        if (musicTrackInDb is not null)
            throw new StrossException("This music track has already been downloaded.");

        DownloadedMusicTrack downloadedMusicTrack =
            await _grpcService.DownloadMusicTrackAsync(sanitizedUrl, providerToUse, cancellationToken);

        List<Creator> creatorsInDb = await _context.Creators
            .Include(c => c.ExternalCreators)
            .Include(c => c.Albums)
            .Where(c => c.ExternalCreators.Any(ec => downloadedMusicTrack.CreatorIds.Contains(ec.ExternalId)))
            .ToListAsync(cancellationToken);

        List<Creator> creatorsForMusicTrack = new List<Creator>();

        foreach (string creatorForTrack in downloadedMusicTrack.CreatorIds)
        {
            FetchedCreatorMetadata fetchedCreatorMetadata =
                await _grpcService.GetCreatorMetadataAsync(creatorForTrack, providerToUse, cancellationToken);

            Creator? creatorToUse =
                creatorsInDb.Find(c => c.ExternalCreators.Any(ec => ec.ExternalId == creatorForTrack));

            ExternalCreator? externalCreatorMusicTrack;

            if (creatorToUse is null)
            {
                string extension = Path.GetExtension(fetchedCreatorMetadata.CreatorThumbnailImageUrl);

                if (string.IsNullOrWhiteSpace(extension) || extension == ".")
                    extension = ".jpg";

                string relativeThumbnailTargetPath = Path.Combine("creators", Guid.NewGuid().ToString(), $"1{extension}");

                creatorToUse = new Creator(providerToUse, fetchedCreatorMetadata.CreatorId,
                    fetchedCreatorMetadata.CreatorName, relativeThumbnailTargetPath, fetchedCreatorMetadata.CreatorUrl);

                externalCreatorMusicTrack = creatorToUse.ExternalCreators.First();

                _context.Creators.Add(creatorToUse);
                creatorsForMusicTrack.Add(creatorToUse);
            }
            else
            {
                externalCreatorMusicTrack = creatorToUse.ExternalCreators.First(ec => ec.ExternalId == creatorForTrack);

                externalCreatorMusicTrack.SetExternalUrl(fetchedCreatorMetadata.CreatorUrl).SetExternalName(fetchedCreatorMetadata.CreatorName);

                creatorsForMusicTrack.Add(creatorToUse);
            }

            string fullThumbnailTargetPath = Path.Combine(_storageConfig.BasePath, externalCreatorMusicTrack.ThumbnailLocation);

            // always update the thumbnail
            await _thumbnailService.GetThumbnailUrlAsync(fetchedCreatorMetadata.CreatorThumbnailImageUrl,
                fullThumbnailTargetPath, cancellationToken);
        }

        Creator mainCreator = creatorsForMusicTrack.First();

        Album? albumToUse = mainCreator.Albums.FirstOrDefault(a => a.Name == Constants.UnknownName);

        if (albumToUse is null)
        {
            albumToUse = new Album(mainCreator, unknownGenre, Constants.UnknownName);
            _context.Albums.Add(albumToUse);
        }

        string fullMusicTrackPath = Path.Combine(_storageConfig.BasePath, downloadedMusicTrack.MusicTrackPath);
        int trackDuration = _audioFileMetadataService.GetDuration(fullMusicTrackPath);
        long fileSize = _audioFileMetadataService.GetFileSize(fullMusicTrackPath);

        Domain.Entities.MusicTrack musicTrack = new Domain.Entities.MusicTrack(providerToUse,
            albumToUse,
            unknownGenre,
            downloadedMusicTrack.MusicTrackPath,
            downloadedMusicTrack.Title,
            downloadedMusicTrack.ThumbnailPath,
            creatorsForMusicTrack,
            downloadedMusicTrack.SourceUrl,
            trackDuration,
            fileSize);

        _context.MusicTracks.Add(musicTrack);

        await _context.SaveChangesAsync(cancellationToken);

        return musicTrack.Id;
    }
}
