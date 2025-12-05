using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
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

    public SubsonicGetSongQueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetSongQuery request, CancellationToken cancellationToken)
    {
        // Parse the song ID
        if (!long.TryParse(request.Input.Id, out long songId))
        {
            throw new Stross.Exception.Exceptions.ValidationException("Invalid song ID format");
        }

        // Retrieve the song with its creators
        Domain.Entities.MusicTrack? musicTrack = await _context.MusicTracks
            .Include(x => x.Creators)
            .FirstOrDefaultAsync(x => x.Id == songId, cancellationToken);

        if (musicTrack == null)
        {
            throw new Exception.Exceptions.EntityNotFoundException($"Song with ID '{request.Input.Id}' not found");
        }

        // Convert to Subsonic format using existing mapping
        Child song = musicTrack.ToSubsonicSongResponse();

        Response response = new Response
        {
            Song = song
        };

        return new SubsonicBaseResponse(response);
    }
}