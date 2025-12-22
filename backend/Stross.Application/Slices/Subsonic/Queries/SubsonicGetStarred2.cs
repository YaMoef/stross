using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Abstractions.Accessors;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Domain.Entities;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetStarred2Query(SubsonicGetStarred2Input Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetStarred2QueryValidator : AbstractValidator<SubsonicGetStarred2Query>
{
    public SubsonicGetStarred2QueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetStarred2QueryHandler : IRequestHandler<SubsonicGetStarred2Query, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicGetStarred2QueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetStarred2Query request, CancellationToken cancellationToken)
    {
        Stross.Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();

        List<UserStarredItem> starredItems = await _context.UserStarredItems
            .AsSplitQuery()
            .Include(s => s.MusicTrack)!
                .ThenInclude(t => t.Album)!
                    .ThenInclude(a => a!.Genre)
            .Include(s => s.MusicTrack)!
                .ThenInclude(t => t.Creators)
            .Include(s => s.Album)!
                .ThenInclude(a => a.Genre)
            .Include(s => s.Album)!
                .ThenInclude(a => a.Creators)
            .Include(s => s.Artist)
            .Where(s => s.UserId == currentUser.Id)
            .ToListAsync(cancellationToken);

        List<Child> songs = starredItems
            .Where(s => s.MusicTrackId.HasValue && s.MusicTrack != null)
            .Select(s => s.MusicTrack!.ToSubsonicSongResponse(s.CreatedAt))
            .ToList();

        List<AlbumId3> albums = starredItems
            .Where(s => s.AlbumId.HasValue && s.Album != null)
            .Select(s => s.Album!.ToSubsonicAlbumId3Response(s.CreatedAt))
            .ToList();

        List<ArtistId3> artists = starredItems
            .Where(s => s.ArtistId.HasValue && s.Artist != null)
            .Select(s => s.Artist!.ToSubsonicArtistID3Response(s.CreatedAt))
            .ToList();

        Response response = new Response
        {
            Starred2 = new Starred2
            {
                Artist = artists,
                Album = albums,
                Song = songs
            }
        };

        return new SubsonicBaseResponse(response);
    }
}