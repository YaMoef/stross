using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Commands;

public sealed record SubsonicScrobbleCommand(SubsonicScrobbleInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicScrobbleCommandValidator : AbstractValidator<SubsonicScrobbleCommand>
{
    public SubsonicScrobbleCommandValidator(IValidator<SubsonicScrobbleInput> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicScrobbleCommandHandler : IRequestHandler<SubsonicScrobbleCommand, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicScrobbleCommandHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicScrobbleCommand request, CancellationToken cancellationToken)
    {
        // Parse the song ID
        if (!long.TryParse(request.Input.Id, out long songId))
        {
            throw new Stross.Exception.Exceptions.ValidationException("Invalid song ID format");
        }

        // Verify the song exists
        bool songExists = await _context.MusicTracks
            .AnyAsync(x => x.Id == songId, cancellationToken);

        if (!songExists)
        {
            throw new Exception.Exceptions.EntityNotFoundException($"Song with ID '{request.Input.Id}' not found");
        }

        // TODO: Implement actual scrobbling logic here
        // This could involve:
        // - Recording play statistics
        // - Updating last played timestamp
        // - Incrementing play count
        // - Submitting to external scrobbling services (Last.fm, etc.)
        // For now, we just return a successful empty response

        Response response = new Response();

        return new SubsonicBaseResponse(response);
    }
}
