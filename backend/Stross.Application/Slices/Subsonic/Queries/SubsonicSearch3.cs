using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Shared.Helpers;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Domain.Entities;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicSearch3Query(SubsonicSearch3Input Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicSearch3QueryValidator : AbstractValidator<SubsonicSearch3Query>
{
    public SubsonicSearch3QueryValidator(IValidator<SubsonicSearch3Input> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicSearch3QueryHandler : IRequestHandler<SubsonicSearch3Query, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicSearch3QueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicSearch3Query request, CancellationToken cancellationToken)
    {
        string searchQuery = request.Input.Query.SanitizeSearchString()!;

        // Search for artists (creators) organized by ID3 tags
        IQueryable<Creator> artistsQuery = _context.Creators
            .Include(c => c.Albums)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchQuery))
            artistsQuery = artistsQuery.Where(x => x.Name.ToLower().Contains(searchQuery));

        List<Creator> artists = await artistsQuery
            .Skip(request.Input.ArtistOffset)
            .Take(request.Input.ArtistCount)
            .ToListAsync(cancellationToken);

        // Search for songs (music tracks) organized by ID3 tags
        IQueryable<Domain.Entities.MusicTrack> songsQuery = _context.MusicTracks
            .Include(x => x.Creators)
            .Include(x => x.Album)
            .Include(x => x.Provider)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchQuery))
            songsQuery = songsQuery.Where(x => x.FriendlyName.ToLower().Contains(searchQuery) ||
                                               x.OriginalName.ToLower().Contains(searchQuery) ||
                                               x.Creators.Any(c => c.Name.ToLower().Contains(searchQuery)));

        List<Domain.Entities.MusicTrack> songs = await songsQuery
            .Skip(request.Input.SongOffset)
            .Take(request.Input.SongCount)
            .ToListAsync(cancellationToken);

        IQueryable<Album> albumsQuery = _context.Albums.Include(a => a.Genre).AsQueryable();

        if (!string.IsNullOrEmpty(searchQuery))
            albumsQuery = albumsQuery.Where(x => x.Name.ToLower().Contains(searchQuery));

        List<Album> albums = await albumsQuery
            .Skip(request.Input.AlbumOffset)
            .Take(request.Input.AlbumCount)
            .ToListAsync(cancellationToken);

        Response response = new Response
        {
            SearchResult3 = new SearchResult3
            {
                Artist = artists.Select(x => x.ToSubsonicArtistID3Response()).ToList(),
                Album = albums.Select(x => x.ToSubsonicAlbumId3Response()).ToList(),
                Song = songs.Select(x => x.ToSubsonicSongResponse()).ToList()
            }
        };

        return new SubsonicBaseResponse(response);
    }
}