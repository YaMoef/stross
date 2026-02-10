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

public sealed record SubsonicCreatePlaylistCommand(SubsonicCreatePlaylistInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicCreatePlaylistCommandValidator : AbstractValidator<SubsonicCreatePlaylistCommand>
{
    public SubsonicCreatePlaylistCommandValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicCreatePlaylistCommandHandler : IRequestHandler<SubsonicCreatePlaylistCommand, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicCreatePlaylistCommandHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicCreatePlaylistCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();

        Domain.Entities.Playlist playlist;

        if (!string.IsNullOrEmpty(request.Input.PlaylistId))
        {
            if (!long.TryParse(request.Input.PlaylistId, out long parsedId))
                throw new EntityNotFoundException($"Playlist with id {request.Input.PlaylistId} was not found");

            playlist = await _context.Playlists.FirstOrDefaultAsync(p => p.Id == parsedId, cancellationToken) ??
                       throw new EntityNotFoundException($"Playlist with id {request.Input.PlaylistId} was not found");
        }
        else
        {
            playlist = new Domain.Entities.Playlist(request.Input.Name, string.Empty, currentUser);
        }

        if (request.Input.SongId is not null && request.Input.SongId.Length > 0)
        {
            List<long> songIds = request.Input.SongId
                .Select(id => long.TryParse(id, out long parsedId) ? parsedId : 0)
                .Where(id => id > 0)
                .ToList();

            if (songIds.Count > 0)
            {
                List<Stross.Domain.Entities.MusicTrack> tracks = await _context.MusicTracks
                    .Where(t => songIds.Contains(t.Id))
                    .ToListAsync(cancellationToken);

                foreach (Stross.Domain.Entities.MusicTrack track in tracks)
                {
                    playlist.AddMusicTrack(track);
                }
            }
        }

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync(cancellationToken);

        PlaylistWithSongs subsonicPlaylist = playlist.ToSubsonicPlaylistWithSongsResponse(null);

        Response response = new Response
        {
            Playlist = subsonicPlaylist
        };

        return new SubsonicBaseResponse(response);
    }
}
