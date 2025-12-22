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

public sealed record SubsonicUnstarCommand(SubsonicUnstarInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicUnstarCommandValidator : AbstractValidator<SubsonicUnstarCommand>
{
    public SubsonicUnstarCommandValidator(IValidator<SubsonicUnstarInput> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicUnstarCommandHandler : IRequestHandler<SubsonicUnstarCommand, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicUnstarCommandHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicUnstarCommand request, CancellationToken cancellationToken)
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

        List<UserStarredItem> starredEntries = await _context.UserStarredItems
            .Where(s => s.UserId == currentUser.Id
                        && ((s.MusicTrackId.HasValue && songIds.Contains(s.MusicTrackId.Value))
                            || (s.AlbumId.HasValue && albumIds.Contains(s.AlbumId.Value))
                            || (s.ArtistId.HasValue && artistIds.Contains(s.ArtistId.Value))))
            .ToListAsync(cancellationToken);

        if (starredEntries.Count > 0)
        {
            _context.UserStarredItems.RemoveRange(starredEntries);
            await _context.SaveChangesAsync(cancellationToken);
        }

        Response response = new Response();

        return new SubsonicBaseResponse(response);
    }
}
