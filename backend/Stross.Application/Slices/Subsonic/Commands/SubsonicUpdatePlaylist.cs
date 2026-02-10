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

namespace Stross.Application.Slices.Subsonic.Commands;

public sealed record SubsonicUpdatePlaylistCommand(SubsonicUpdatePlaylistInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicUpdatePlaylistCommandValidator : AbstractValidator<SubsonicUpdatePlaylistCommand>
{
    public SubsonicUpdatePlaylistCommandValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicUpdatePlaylistCommandHandler : IRequestHandler<SubsonicUpdatePlaylistCommand, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicUpdatePlaylistCommandHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicUpdatePlaylistCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();

        if (!long.TryParse(request.Input.PlaylistId, out long playlistId))
            throw new EntityNotFoundException($"Playlist with id {request.Input.PlaylistId} was not found");

        Domain.Entities.Playlist? playlist = await _context.Playlists
            .Include(p => p.Owner)
            .Include(p => p.PlaylistMusicTracks)
                .ThenInclude(t => t.MusicTrack)
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.OwnerId == currentUser.Id, cancellationToken);

        if (playlist is null)
            throw new EntityNotFoundException($"Playlist with id {request.Input.PlaylistId} was not found");

        if (!string.IsNullOrWhiteSpace(request.Input.Name))
            playlist.ChangeName(request.Input.Name);

        if (!string.IsNullOrWhiteSpace(request.Input.Comment))
            playlist.ChangeComment(request.Input.Comment);
        else if (!string.IsNullOrWhiteSpace(request.Input.Description))
            playlist.ChangeComment(request.Input.Description);

        if (request.Input.Public.HasValue)
            playlist.SetPublic(request.Input.Public.Value);

        if (request.Input.SongIdToAdd is not null && request.Input.SongIdToAdd.Length > 0)
        {
            List<long> idsToAdd = request.Input.SongIdToAdd
                .Select(id => long.TryParse(id, out long parsedId) ? parsedId : 0)
                .Where(id => id > 0)
                .ToList();

            if (idsToAdd.Count > 0)
            {
                List<Stross.Domain.Entities.MusicTrack> tracksToAdd = await _context.MusicTracks
                    .Where(t => idsToAdd.Contains(t.Id))
                    .ToListAsync(cancellationToken);

                foreach (Stross.Domain.Entities.MusicTrack track in tracksToAdd)
                {
                    playlist.AddMusicTrack(track);
                }
            }
        }

        if (request.Input.SongIndexToRemove is not null && request.Input.SongIndexToRemove.Length > 0)
        {
            List<int> indexesToRemove = request.Input.SongIndexToRemove
                .Select(id => int.TryParse(id, out int parsedId) ? (int?)parsedId : null)
                .Where(id => id is not null)
                .Select(id => id!.Value)
                .ToList();

            if (indexesToRemove.Count > 0)
            {
                playlist.RemoveMusicTrackByOrders(indexesToRemove);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        PlaylistWithSongs subsonicPlaylist = playlist.ToSubsonicPlaylistWithSongsResponse(null);

        Response response = new Response
        {
            Playlist = subsonicPlaylist
        };

        return new SubsonicBaseResponse(response);
    }
}
