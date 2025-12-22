using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Abstractions.Accessors;
using Stross.Application.Shared.Helpers;
using Stross.Application.Slices.Subsonic.Helpers;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Domain.Entities;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicSearch2Query(SubsonicSearch2Input Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicSearch2QueryValidator : AbstractValidator<SubsonicSearch2Query>
{
    public SubsonicSearch2QueryValidator(IValidator<SubsonicSearch2Input> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicSearch2QueryHandler : IRequestHandler<SubsonicSearch2Query, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicSearch2QueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicSearch2Query request, CancellationToken cancellationToken)
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

        // Search for songs (music tracks) organized by ID2 tags
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

        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);
        StarredData starredData = currentUser is not null
            ? await StarredDataHelper.LoadStarredDataForUserAsync(_context, currentUser.Id, cancellationToken)
            : new StarredData(new(), new(), new());

        Response response = new Response
        {
            SearchResult2 = new SearchResult2
            {
                Artist = artists.Select(x => x.ToSubsonicArtistResponse(starredData.StarredArtists.GetValueOrDefault(x.Id))).ToList(),
                Album = albums.Select(x => x.ToSubsonicAlbumListResponse(starredData.StarredAlbums.GetValueOrDefault(x.Id))).ToList(),
                Song = songs.Select(x => x.ToSubsonicSongResponse(starredData.StarredTracks.GetValueOrDefault(x.Id))).ToList()
            }
        };

        return new SubsonicBaseResponse(response);
    }
}