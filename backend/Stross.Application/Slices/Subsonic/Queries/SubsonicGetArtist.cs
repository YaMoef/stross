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

public sealed record SubsonicGetArtistQuery(SubsonicGetArtistInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetArtistQueryValidator : AbstractValidator<SubsonicGetArtistQuery>
{
    public SubsonicGetArtistQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");

        RuleFor(x => x.Input.Id)
            .NotEmpty()
            .WithMessage("Id is required for getArtist");
    }
}

internal sealed class SubsonicGetArtistQueryHandler : IRequestHandler<SubsonicGetArtistQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicGetArtistQueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetArtistQuery request, CancellationToken cancellationToken)
    {
        string artistId = request.Input.Id;

        // Try to parse the artist ID
        if (!long.TryParse(artistId, out long creatorId))
            throw new EntityNotFoundException($"Invalid artist ID: {artistId}");

        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();

        // Get the creator (artist) with their albums
        Creator? creator = await _context.Creators
            .Include(c => c.Albums)
            .ThenInclude(a => a.MusicTracks)
            .Include(c => c.Albums)
            .ThenInclude(a => a.Creators)
            .Include(a => a.Albums)
            .ThenInclude(a => a.Genre)
            .FirstOrDefaultAsync(c => c.Id == creatorId, cancellationToken);

        if (creator is null)
            throw new EntityNotFoundException($"Artist with ID {artistId} not found");

        StarredData starredData = await StarredDataHelper.LoadStarredDataForUserAsync(_context, currentUser.Id, cancellationToken);

        DateTime? artistStarredDate = starredData.StarredArtists.GetValueOrDefault(creator.Id);

        Response response = new Response
        {
            Artist = creator.ToSubsonicArtistWithAlbumsResponse(artistStarredDate, starredData.StarredAlbums)
        };

        return new SubsonicBaseResponse(response);
    }
}