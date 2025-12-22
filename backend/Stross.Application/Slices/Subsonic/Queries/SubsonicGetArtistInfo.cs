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

public sealed record SubsonicGetArtistInfoQuery(SubsonicGetArtistInfoInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetArtistInfoQueryValidator : AbstractValidator<SubsonicGetArtistInfoQuery>
{
    public SubsonicGetArtistInfoQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");

        RuleFor(x => x.Input.Id)
            .NotEmpty()
            .WithMessage("Id is required for getArtistInfo");
    }
}

internal sealed class SubsonicGetArtistInfoQueryHandler : IRequestHandler<SubsonicGetArtistInfoQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicGetArtistInfoQueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetArtistInfoQuery request, CancellationToken cancellationToken)
    {
        string artistId = request.Input.Id;

        // Try to parse the artist ID
        if (!long.TryParse(artistId, out long creatorId))
        {
            throw new EntityNotFoundException($"Invalid artist ID: {artistId}");
        }

        // Get the creator (artist)
        Creator? creator = await _context.Creators
            .FirstOrDefaultAsync(c => c.Id == creatorId, cancellationToken);

        if (creator is null)
        {
            throw new EntityNotFoundException($"Artist with ID {artistId} not found");
        }

        // Get similar artists (for now, just get other artists - TODO: implement actual similarity logic)
        List<Creator> similarArtists = await GetSimilarArtists(creatorId, request.Input.Count, request.Input.IncludeNotPresent, cancellationToken);

        ArtistInfo artistInfo = new ArtistInfo
        {
            Biography = GenerateBasicBiography(creator),
            MusicBrainzId = null, // TODO: Implement MusicBrainz integration
            LastFmUrl = null, // TODO: Implement Last.fm integration
            SmallImageUrl = $"/api/v1/thumbnails/creator/{creator.Id}?type=small",
            MediumImageUrl = $"/api/v1/thumbnails/creator/{creator.Id}?type=medium", 
            LargeImageUrl = $"/api/v1/thumbnails/creator/{creator.Id}?type=large",
            SimilarArtist = similarArtists.Select(a => a.ToSubsonicArtistResponse(null)).ToList()
        };

        Response response = new Response
        {
            ArtistInfo = artistInfo
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
                .Where(c => c.Id != excludeCreatorId && c.Albums.Any())
                .OrderBy(c => Guid.NewGuid())
                .Take(count)
                .ToListAsync(cancellationToken);
        }
        else
        {
            // Include all artists
            return await _context.Creators
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