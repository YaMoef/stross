using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Abstractions.Accessors;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Domain.Entities;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Commands;

public sealed record SubsonicStarCommand(SubsonicStarInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicStarCommandValidator : AbstractValidator<SubsonicStarCommand>
{
    public SubsonicStarCommandValidator(IValidator<SubsonicStarInput> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicStarCommandHandler : IRequestHandler<SubsonicStarCommand, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicStarCommandHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicStarCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();

        List<long> songIds = request.Input.Id is null
            ? new List<long>()
            : request.Input.Id
                .Select(id => long.TryParse(id, out long parsedId) ? parsedId : 0)
                .Where(id => id > 0)
                .ToList();

        List<long> albumIds = request.Input.AlbumId is null
            ? new List<long>()
            : request.Input.AlbumId
                .Select(id => long.TryParse(id, out long parsedId) ? parsedId : 0)
                .Where(id => id > 0)
                .ToList();

        List<long> artistIds = request.Input.ArtistId is null
            ? new List<long>()
            : request.Input.ArtistId
                .Select(id => long.TryParse(id, out long parsedId) ? parsedId : 0)
                .Where(id => id > 0)
                .ToList();

        if (songIds.Count == 0 && albumIds.Count == 0 && artistIds.Count == 0)
        {
            Response emptyResponse = new Response();

            return new SubsonicBaseResponse(emptyResponse);
        }

        List<Stross.Domain.Entities.MusicTrack> tracks = songIds.Count == 0
            ? new List<Stross.Domain.Entities.MusicTrack>()
            : await _context.MusicTracks
                .Where(t => songIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

        List<Stross.Domain.Entities.Album> albums = albumIds.Count == 0
            ? new List<Stross.Domain.Entities.Album>()
            : await _context.Albums
                .Where(a => albumIds.Contains(a.Id))
                .ToListAsync(cancellationToken);

        List<Stross.Domain.Entities.Creator> artists = artistIds.Count == 0
            ? new List<Stross.Domain.Entities.Creator>()
            : await _context.Creators
                .Where(a => artistIds.Contains(a.Id))
                .ToListAsync(cancellationToken);

        if (tracks.Count == 0 && albums.Count == 0 && artists.Count == 0)
            throw new EntityNotFoundException("No valid items found for provided ids");

        List<long?> existingTrackIds = await _context.UserStarredItems
            .Where(s => s.UserId == currentUser.Id && s.MusicTrackId.HasValue && songIds.Contains(s.MusicTrackId.Value))
            .Select(s => s.MusicTrackId)
            .ToListAsync(cancellationToken);

        List<long?> existingAlbumIds = await _context.UserStarredItems
            .Where(s => s.UserId == currentUser.Id && s.AlbumId.HasValue && albumIds.Contains(s.AlbumId.Value))
            .Select(s => s.AlbumId)
            .ToListAsync(cancellationToken);

        List<long?> existingArtistIds = await _context.UserStarredItems
            .Where(s => s.UserId == currentUser.Id && s.ArtistId.HasValue && artistIds.Contains(s.ArtistId.Value))
            .Select(s => s.ArtistId)
            .ToListAsync(cancellationToken);

        foreach (Stross.Domain.Entities.MusicTrack track in tracks)
        {
            if (existingTrackIds.Contains(track.Id))
                continue;

            UserStarredItem starredTrack = new UserStarredItem(currentUser, track);
            _context.UserStarredItems.Add(starredTrack);
        }

        foreach (Stross.Domain.Entities.Album album in albums)
        {
            if (existingAlbumIds.Contains(album.Id))
                continue;

            UserStarredItem starredAlbum = new UserStarredItem(currentUser, album);
            _context.UserStarredItems.Add(starredAlbum);
        }

        foreach (Stross.Domain.Entities.Creator artist in artists)
        {
            if (existingArtistIds.Contains(artist.Id))
                continue;

            UserStarredItem starredArtist = new UserStarredItem(currentUser, artist);
            _context.UserStarredItems.Add(starredArtist);
        }

        await _context.SaveChangesAsync(cancellationToken);

        Response response = new Response();

        return new SubsonicBaseResponse(response);
    }
}
