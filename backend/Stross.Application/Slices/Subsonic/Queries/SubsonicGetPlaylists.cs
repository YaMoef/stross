using FluentValidation;
using MediatR;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Stross.Abstractions.Accessors;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Exception.Exceptions;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetPlaylistsQuery(SubsonicGetPlaylistsInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetPlaylistsQueryValidator : AbstractValidator<SubsonicGetPlaylistsQuery>
{
    public SubsonicGetPlaylistsQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetPlaylistsQueryHandler : IRequestHandler<SubsonicGetPlaylistsQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicGetPlaylistsQueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetPlaylistsQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();
        
        List<Domain.Entities.Playlist> publicPlaylists = await _context.Playlists
            .Include(p => p.Owner)
            .Include(p => p.PlaylistMusicTracks)
                .ThenInclude(t => t.MusicTrack)
            .Where(p => p.Public || p.OwnerId == currentUser.Id)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        List<Playlist> subsonicPlaylists = publicPlaylists
            .Select(p => p.ToSubsonicPlaylistResponse())
            .ToList();

        Response response = new Response
        {
            Playlists = subsonicPlaylists
        };

        return new SubsonicBaseResponse(response);
    }
}