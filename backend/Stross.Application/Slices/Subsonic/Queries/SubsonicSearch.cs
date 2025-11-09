using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Domain.Entities;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicSearchQuery(SubsonicSearchInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicSearchQueryValidator : AbstractValidator<SubsonicSearchQuery>
{
    public SubsonicSearchQueryValidator(IValidator<SubsonicSearchInput> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicSearchQueryHandler : IRequestHandler<SubsonicSearchQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicSearchQueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicSearchQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.MusicTrack> query = _context.MusicTracks
            .Include(x => x.Creators)
            .Include(x => x.Provider)
            .AsQueryable();

        // Apply search filters
        if (!string.IsNullOrEmpty(request.Input.Artist))
            query = query.Where(x => x.Creators.Any(c => c.Name.Contains(request.Input.Artist)));

        if (!string.IsNullOrEmpty(request.Input.Title))
            query = query.Where(x => x.FriendlyName.Contains(request.Input.Title) ||
                                     x.OriginalName.Contains(request.Input.Title));

        if (!string.IsNullOrEmpty(request.Input.Any))
            query = query.Where(x => x.FriendlyName.Contains(request.Input.Any) ||
                                     x.OriginalName.Contains(request.Input.Any) ||
                                     x.Creators.Any(c => c.Name.Contains(request.Input.Any)));

        if (request.Input.NewerThan.HasValue)
        {
            DateTime newerThanDate = DateTimeOffset.FromUnixTimeMilliseconds(request.Input.NewerThan.Value).DateTime;
            query = query.Where(x => x.CreatedAt > newerThanDate);
        }

        int totalHits = await query.CountAsync(cancellationToken);

        List<Domain.Entities.MusicTrack> results = await query
            .Skip(request.Input.Offset)
            .Take(request.Input.Count)
            .ToListAsync(cancellationToken);

        Response response = new Response
        {
            SearchResult = new SearchResult
            {
                Offset = request.Input.Offset,
                TotalHits = totalHits,
                Match = results.Select(x => x.ToSubsonicSongResponse()).ToList()
            }
        };

        return new SubsonicBaseResponse(response);
    }
}