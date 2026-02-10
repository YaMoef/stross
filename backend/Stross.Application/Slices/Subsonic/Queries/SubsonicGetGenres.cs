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
        List<Domain.Entities.Genre> allGenres = await _context.Genres
            .Include(g => g.MusicTracks)
            .Include(g => g.Albums)
            .ToListAsync(cancellationToken);

        Response response = new Response
        {
            Genres = allGenres.Select(g => new Genre()
            {
                Text = [g.Name],
                SongCount = g.MusicTracks.Count,
                AlbumCount = g.Albums.Count
            }).ToList()
        };

        return new SubsonicBaseResponse(response);
    }
}
