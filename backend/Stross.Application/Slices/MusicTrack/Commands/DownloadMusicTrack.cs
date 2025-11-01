using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stross.Application.Slices.MusicTrack.InputModels;
using Stross.Application.Slices.MusicTrack.ResponseModels;
using Stross.Config;
using Stross.Domain.Entities;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;
using Stross.Infrastructure.Services.GrpcService;
using Stross.Infrastructure.Services.GrpcService.Models;
using Stross.Infrastructure.Services.ThumbnailService;

namespace Stross.Application.Slices.MusicTrack.Commands;

public sealed record DownloadMusicTrackCommand(DownloadMusicTrackInput Input) : IRequest<long>;

internal sealed class DownloadMusicTrackCommandHandler : IRequestHandler<DownloadMusicTrackCommand, long>
{
    private readonly StrossContext _context;
    private readonly IGrpcService _grpcService;
    private readonly StrossStorageConfig _storageConfig;
    private readonly IThumbnailService _thumbnailService;

    public DownloadMusicTrackCommandHandler(StrossContext context, IGrpcService grpcService, IOptionsSnapshot<StrossStorageConfig> storageConfigSnapshot, IThumbnailService thumbnailService)
    {
        _context = context;
        _grpcService = grpcService;
        _thumbnailService = thumbnailService;
        _storageConfig = storageConfigSnapshot.Value;
    }

    public async Task<long> Handle(DownloadMusicTrackCommand request, CancellationToken cancellationToken)
    {
        // TODO: sanitize the input url
        Domain.Entities.MusicTrack? musicTrackInDb = await _context.MusicTracks.FirstOrDefaultAsync(m => m.ExternalUrl == request.Input.SourceUrl, cancellationToken);

        if (musicTrackInDb is not null)
            throw new StrossException("This music track has already been downloaded.");

        Provider? providerToUse =
            await _context.Providers.FirstOrDefaultAsync(p => p.Id == request.Input.ProviderId, cancellationToken);

        if (providerToUse is null)
            throw new EntityNotFound(nameof(Provider));

        if (!providerToUse.Enabled)
            throw new ProviderException("This provider is not enabled.");

        DownloadedMusicTrack downloadedMusicTrack =
            await _grpcService.DownloadMusicTrackAsync(request.Input.SourceUrl, providerToUse, cancellationToken);

        List<Creator> creatorsInDb = await _context.Creators
            .Where(c => c.ExternalCreatorMusicTrack.Any(ec => downloadedMusicTrack.CreatorIds.Contains(ec.ExternalId)))
            .ToListAsync(cancellationToken);

        List<Creator> creatorsForMusicTrack = new List<Creator>();

        foreach (string creatorForTrack in downloadedMusicTrack.CreatorIds)
        {
            FetchedCreatorMetadata fetchedCreatorMetadata =
                await _grpcService.GetCreatorMetadataAsync(creatorForTrack, providerToUse, cancellationToken);

            Creator? creatorToUse =
                creatorsInDb.Find(c => c.ExternalCreatorMusicTrack.Any(ec => ec.ExternalId == creatorForTrack));

            ExternalCreatorMusicTrack? externalCreatorMusicTrack;

            if (creatorToUse is null)
            {
                string extension = Path.GetExtension(fetchedCreatorMetadata.CreatorThumbnailImageUrl);

                if (string.IsNullOrWhiteSpace(extension) || extension == ".")
                    extension = ".jpg";

                string thumbnailTargetPath = Path.Combine(_storageConfig.BasePath, "creators", Guid.NewGuid().ToString(), $"1{extension}");

                creatorToUse = new Creator(providerToUse, fetchedCreatorMetadata.CreatorId,
                    fetchedCreatorMetadata.CreatorName, thumbnailTargetPath, fetchedCreatorMetadata.CreatorUrl);

                externalCreatorMusicTrack = creatorToUse.ExternalCreatorMusicTrack.First();

                _context.Creators.Add(creatorToUse);
                creatorsForMusicTrack.Add(creatorToUse);
            }
            else
            {
                externalCreatorMusicTrack = creatorToUse.ExternalCreatorMusicTrack.First(ec => ec.ExternalId == creatorForTrack);

                externalCreatorMusicTrack.SetExternalUrl(fetchedCreatorMetadata.CreatorUrl).SetExternalName(fetchedCreatorMetadata.CreatorName);
            }

            // always update the thumbnail
            await _thumbnailService.GetThumbnailUrlAsync(fetchedCreatorMetadata.CreatorThumbnailImageUrl,
                externalCreatorMusicTrack.ThumbnailLocation, cancellationToken);
        }

        Domain.Entities.MusicTrack musicTrack = new Domain.Entities.MusicTrack(providerToUse,
            downloadedMusicTrack.MusicTrackPath, downloadedMusicTrack.Title, downloadedMusicTrack.ThumbnailPath, creatorsForMusicTrack, downloadedMusicTrack.ExternalUrl);

        _context.MusicTracks.Add(musicTrack);

        await _context.SaveChangesAsync(cancellationToken);

        return musicTrack.Id;
    }
}