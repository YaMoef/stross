using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetGenresQuery(SubsonicGetGenresInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetGenresQueryValidator : AbstractValidator<SubsonicGetGenresQuery>
{
    public SubsonicGetGenresQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetGenresQueryHandler : IRequestHandler<SubsonicGetGenresQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicGetGenresQueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetGenresQuery request, CancellationToken cancellationToken)
    {
        int totalSongCount = await _context.MusicTracks.CountAsync(cancellationToken);

        Response response = new Response
        {
            Genres = new List<Genre>
            {
                new Genre
                {
                    Text = ["UnKnown"],
                    SongCount = totalSongCount,
                    AlbumCount = 0
                }
            }
        };

        return new SubsonicBaseResponse(response);
    }
}