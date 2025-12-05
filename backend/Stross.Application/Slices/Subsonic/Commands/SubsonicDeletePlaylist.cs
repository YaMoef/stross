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

public sealed record SubsonicDeletePlaylistCommand(SubsonicDeletePlaylistInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicDeletePlaylistCommandValidator : AbstractValidator<SubsonicDeletePlaylistCommand>
{
    public SubsonicDeletePlaylistCommandValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicDeletePlaylistCommandHandler : IRequestHandler<SubsonicDeletePlaylistCommand, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicDeletePlaylistCommandHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicDeletePlaylistCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();
        
        if (!long.TryParse(request.Input.Id, out long playlistId))
            throw new EntityNotFoundException($"Playlist with id {request.Input.Id} was not found");

        Domain.Entities.Playlist? playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.OwnerId == currentUser.Id, cancellationToken);

        if (playlist is null)
            throw new EntityNotFoundException($"Playlist with id {request.Input.Id} was not found");

        _context.Playlists.Remove(playlist);
        await _context.SaveChangesAsync(cancellationToken);

        Response response = new Response();

        return new SubsonicBaseResponse(response);
    }
}
