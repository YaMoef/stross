using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Abstractions.Accessors;
using Stross.Application.Shared.Helpers;
using Stross.Application.Slices.Subsonic.Helpers;
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
    private readonly IUserAccessor _userAccessor;

    public SubsonicSearchQueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicSearchQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.MusicTrack> query = _context.MusicTracks
            .Include(x => x.Creators)
            .Include(x => x.Provider)
            .AsQueryable();

        // Apply search filters
        string? sanitizedArtistSearch = request.Input.Artist.SanitizeSearchString();

        if (!string.IsNullOrEmpty(sanitizedArtistSearch))
            query = query.Where(x => x.Creators.Any(c => c.Name.Contains(sanitizedArtistSearch)));

        string? sanitizedAlbumSearch = request.Input.Album.SanitizeSearchString();

        if (!string.IsNullOrEmpty(sanitizedAlbumSearch))
            query = query.Where(x => x.FriendlyName.Contains(sanitizedAlbumSearch) ||
                                     x.OriginalName.Contains(sanitizedAlbumSearch));

        string? sanitizedAnySearch = request.Input.Any.SanitizeSearchString();

        if (!string.IsNullOrEmpty(sanitizedAnySearch))
            query = query.Where(x => x.FriendlyName.Contains(sanitizedAnySearch) ||
                                     x.OriginalName.Contains(sanitizedAnySearch) ||
                                     x.Creators.Any(c => c.Name.Contains(sanitizedAnySearch)));

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

        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);
        StarredData starredData = currentUser is not null
            ? await StarredDataHelper.LoadStarredDataForUserAsync(_context, currentUser.Id, cancellationToken)
            : new StarredData(new(), new(), new());

        Response response = new Response
        {
            SearchResult = new SearchResult
            {
                Offset = request.Input.Offset,
                TotalHits = totalHits,
                Match = results.Select(x => x.ToSubsonicSongResponse(starredData.StarredTracks.GetValueOrDefault(x.Id))).ToList()
            }
        };

        return new SubsonicBaseResponse(response);
    }
}