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

public sealed record SubsonicGetSongQuery(SubsonicGetSongInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetSongQueryValidator : AbstractValidator<SubsonicGetSongQuery>
{
    public SubsonicGetSongQueryValidator(IValidator<SubsonicGetSongInput> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicGetSongQueryHandler : IRequestHandler<SubsonicGetSongQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicGetSongQueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetSongQuery request, CancellationToken cancellationToken)
    {
        // Parse the song ID
        if (!long.TryParse(request.Input.Id, out long songId))
            throw new Exception.Exceptions.ValidationException("Invalid song ID format");

        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();

        // Retrieve the song with its creators
        Domain.Entities.MusicTrack? musicTrack = await _context.MusicTracks
            .Include(x => x.Creators)
            .FirstOrDefaultAsync(x => x.Id == songId, cancellationToken);

        if (musicTrack == null)
            throw new EntityNotFoundException($"Song with ID '{request.Input.Id}' not found");

        StarredData starredData = await StarredDataHelper.LoadStarredDataForUserAsync(_context, currentUser.Id, cancellationToken);

        // Convert to Subsonic format using existing mapping
        Child song = musicTrack.ToSubsonicSongResponse(starredData.StarredTracks.GetValueOrDefault(musicTrack.Id));

        Response response = new Response
        {
            Song = song
        };

        return new SubsonicBaseResponse(response);
    }
}