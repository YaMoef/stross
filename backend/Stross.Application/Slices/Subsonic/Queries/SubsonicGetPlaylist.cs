using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Abstractions.Accessors;
using Stross.Application.Slices.Subsonic.Helpers;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetPlaylistQuery(SubsonicGetPlaylistInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetPlaylistQueryValidator : AbstractValidator<SubsonicGetPlaylistQuery>
{
    public SubsonicGetPlaylistQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetPlaylistQueryHandler : IRequestHandler<SubsonicGetPlaylistQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicGetPlaylistQueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetPlaylistQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();

        if (!long.TryParse(request.Input.Id, out long playlistId))
            throw new EntityNotFoundException($"Playlist with id {request.Input.Id} was not found");

        Stross.Domain.Entities.Playlist? playlist = await _context.Playlists
            .Include(p => p.Owner)
            .Include(p => p.PlaylistMusicTracks)
                .ThenInclude(t => t.MusicTrack)
            .FirstOrDefaultAsync(p => p.Id == playlistId && (p.Public || p.OwnerId == currentUser.Id), cancellationToken);

        if (playlist is null)
            throw new EntityNotFoundException($"Playlist with id {request.Input.Id} was not found");

        StarredData starredData = await StarredDataHelper.LoadStarredDataForUserAsync(_context, currentUser.Id, cancellationToken);

        PlaylistWithSongs subsonicPlaylist = playlist.ToSubsonicPlaylistWithSongsResponse(starredData.StarredTracks);

        Response response = new Response
        {
            Playlist = subsonicPlaylist
        };

        return new SubsonicBaseResponse(response);
    }
}
