using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Abstractions.Accessors;
using Stross.Application.Slices.Subsonic.Helpers;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Domain.Entities;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetAlbumListQuery(SubsonicGetAlbumListInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetAlbumListQueryValidator : AbstractValidator<SubsonicGetAlbumListQuery>
{
    public SubsonicGetAlbumListQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetAlbumListQueryHandler : IRequestHandler<SubsonicGetAlbumListQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicGetAlbumListQueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetAlbumListQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();

        IQueryable<Album> albumsQuery = _context.Albums
            .Include(a => a.Creators)
            .Include(a => a.Genre)
            .Include(a => a.MusicTracks);

        // Filter by music folder if specified
        if (!string.IsNullOrEmpty(request.Input.MusicFolderId) && int.TryParse(request.Input.MusicFolderId, out int musicFolderId))
        {
            albumsQuery = albumsQuery.Where(a => 
                a.MusicTracks.Any(mt => mt.ProviderId == musicFolderId));
        }

        // Apply type-specific filtering and ordering
        string type = request.Input.Type.ToLowerInvariant();
        
        albumsQuery = type switch
        {
            "random" => albumsQuery.OrderBy(a => Guid.NewGuid()),
            "newest" => albumsQuery.OrderByDescending(a => a.CreatedAt),
            "highest" => albumsQuery.OrderByDescending(a => 0), // TODO: Implement rating functionality
            "frequent" => albumsQuery.OrderByDescending(a => 0), // TODO: Implement play count functionality
            "recent" => albumsQuery.OrderByDescending(a => a.UpdatedAt),
            "alphabeticalByName" => albumsQuery.OrderBy(a => a.Name),
            "alphabeticalByArtist" => albumsQuery.OrderBy(a => a.Creators.FirstOrDefault()!.Name).ThenBy(a => a.Name),
            "starred" => albumsQuery.Where(a => _context.UserStarredItems.Any(usi => usi.AlbumId == a.Id && usi.UserId == currentUser.Id)),
            "byYear" => ApplyYearFilter(albumsQuery, request.Input.FromYear, request.Input.ToYear),
            "byGenre" => ApplyGenreFilter(albumsQuery, request.Input.Genre),
            _ => albumsQuery.OrderBy(a => a.Name)
        };

        // Apply pagination
        List<Album> albums = await albumsQuery
            .Skip(request.Input.Offset)
            .Take(request.Input.Size)
            .ToListAsync(cancellationToken);

        StarredData starredData = await StarredDataHelper.LoadStarredDataForUserAsync(_context, currentUser.Id, cancellationToken);

        Response response = new Response
        {
            AlbumList = albums.Select(a => a.ToSubsonicAlbumListResponse(starredData.StarredAlbums.GetValueOrDefault(a.Id))).ToList()
        };

        return new SubsonicBaseResponse(response);
    }

    private static IQueryable<Album> ApplyYearFilter(IQueryable<Album> query, int? fromYear, int? toYear)
    {
        // TODO: Implement year filtering when album year metadata is available
        // For now, just return the query as-is
        return query.OrderByDescending(a => a.CreatedAt);
    }

    private static IQueryable<Album> ApplyGenreFilter(IQueryable<Album> query, string? genre)
    {
        if (string.IsNullOrEmpty(genre))
            return query;

        return query.Where(a => a.Genre != null && a.Genre.Name.ToLower().Contains(genre.ToLower()))
                   .OrderBy(a => a.Name);
    }
}