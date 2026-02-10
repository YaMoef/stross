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

public sealed record SubsonicGetAlbumQuery(SubsonicGetAlbumInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetAlbumQueryValidator : AbstractValidator<SubsonicGetAlbumQuery>
{
    public SubsonicGetAlbumQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");

        RuleFor(x => x.Input.Id)
            .NotEmpty()
            .WithMessage("Id is required for getAlbum");
    }
}

internal sealed class SubsonicGetAlbumQueryHandler : IRequestHandler<SubsonicGetAlbumQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicGetAlbumQueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetAlbumQuery request, CancellationToken cancellationToken)
    {
        string albumId = request.Input.Id;

        // Try to parse the album ID
        if (!long.TryParse(albumId, out long parsedAlbumId))
            throw new EntityNotFoundException($"Invalid album ID: {albumId}");

        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();

        // Get the album with its songs and creators
        Album? album = await _context.Albums
            .Include(a => a.MusicTracks)
            .ThenInclude(mt => mt.Creators)
            .Include(a => a.MusicTracks)
            .ThenInclude(mt => mt.Provider)
            .Include(a => a.Creators)
            .Include(a => a.Genre)
            .FirstOrDefaultAsync(a => a.Id == parsedAlbumId, cancellationToken);

        if (album is null)
            throw new EntityNotFoundException($"Album with ID {albumId} not found");

        StarredData starredData = await StarredDataHelper.LoadStarredDataForUserAsync(_context, currentUser.Id, cancellationToken);

        DateTime? albumStarredDate = starredData.StarredAlbums.GetValueOrDefault(album.Id);

        Response response = new Response
        {
            Album = album.ToSubsonicAlbumWithSongsResponse(albumStarredDate, starredData.StarredTracks)
        };

        return new SubsonicBaseResponse(response);
    }
}
