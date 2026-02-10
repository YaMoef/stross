using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Domain.Entities;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetArtistInfo2Query(SubsonicGetArtistInfo2Input Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetArtistInfo2QueryValidator : AbstractValidator<SubsonicGetArtistInfo2Query>
{
    public SubsonicGetArtistInfo2QueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");

        RuleFor(x => x.Input.Id)
            .NotEmpty()
            .WithMessage("Id is required for getArtistInfo2");
    }
}

internal sealed class SubsonicGetArtistInfo2QueryHandler : IRequestHandler<SubsonicGetArtistInfo2Query, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicGetArtistInfo2QueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetArtistInfo2Query request, CancellationToken cancellationToken)
    {
        string artistId = request.Input.Id;

        // Try to parse the artist ID
        if (!long.TryParse(artistId, out long creatorId))
        {
            throw new EntityNotFoundException($"Invalid artist ID: {artistId}");
        }

        // Get the creator (artist) with albums
        Creator? creator = await _context.Creators
            .Include(c => c.Albums)
            .FirstOrDefaultAsync(c => c.Id == creatorId, cancellationToken);

        if (creator is null)
        {
            throw new EntityNotFoundException($"Artist with ID {artistId} not found");
        }

        // Get similar artists (for now, just get other artists - TODO: implement actual similarity logic)
        List<Creator> similarArtists = await GetSimilarArtists(creatorId, request.Input.Count, request.Input.IncludeNotPresent, cancellationToken);

        ArtistInfo2 artistInfo2 = new ArtistInfo2
        {
            Biography = GenerateBasicBiography(creator),
            MusicBrainzId = null, // TODO: Implement MusicBrainz integration
            LastFmUrl = null, // TODO: Implement Last.fm integration
            SmallImageUrl = $"/api/v1/thumbnails/creator/{creator.Id}?type=small",
            MediumImageUrl = $"/api/v1/thumbnails/creator/{creator.Id}?type=medium",
            LargeImageUrl = $"/api/v1/thumbnails/creator/{creator.Id}?type=large",
            SimilarArtist = similarArtists.Select(a => a.ToSubsonicArtistID3Response(null)).ToList()
        };

        Response response = new Response
        {
            ArtistInfo2 = artistInfo2
        };

        return new SubsonicBaseResponse(response);
    }

    private async Task<List<Creator>> GetSimilarArtists(long excludeCreatorId, int count, bool includeNotPresent, CancellationToken cancellationToken)
    {
        // TODO: Implement proper similarity algorithm based on genres, collaborations, etc.
        // For now, just return random other artists

        if (!includeNotPresent)
        {
            // Only return artists that have albums/tracks in the library
            return await _context.Creators
                .Include(c => c.Albums)
                .Where(c => c.Id != excludeCreatorId && c.Albums.Any())
                .OrderBy(c => Guid.NewGuid())
                .Take(count)
                .ToListAsync(cancellationToken);
        }
        else
        {
            // Include all artists
            return await _context.Creators
                .Include(c => c.Albums)
                .Where(c => c.Id != excludeCreatorId)
                .OrderBy(c => Guid.NewGuid())
                .Take(count)
                .ToListAsync(cancellationToken);
        }
    }

    private static string GenerateBasicBiography(Creator creator)
    {
        // TODO: Integrate with Last.fm or other biography sources
        return $"Information about {creator.Name} is not available at this time.";
    }
}