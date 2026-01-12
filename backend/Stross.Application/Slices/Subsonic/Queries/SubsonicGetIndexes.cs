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
using Index = Stross.SubsonicModels.Index;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetIndexesQuery(SubsonicGetIndexesInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetIndexesQueryValidator : AbstractValidator<SubsonicGetIndexesQuery>
{
    public SubsonicGetIndexesQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetIndexesQueryHandler : IRequestHandler<SubsonicGetIndexesQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicGetIndexesQueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetIndexesQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

        if (currentUser is null)
            throw new AuthenticationException();

        // Get the last modified timestamp from the most recently updated creator
        long lastModified = await GetLastModifiedTimestamp(request.Input.IfModifiedSince, cancellationToken);

        // If ifModifiedSince is provided and nothing has changed, return early
        if (request.Input.IfModifiedSince.HasValue && lastModified <= request.Input.IfModifiedSince.Value)
        {
            Response notModifiedResponse = new Response
            {
                Indexes = new Indexes
                {
                    LastModified = lastModified,
                    IgnoredArticles = "The El La Los Las Le Les",
                    Index = []
                }
            };

            return new SubsonicBaseResponse(notModifiedResponse);
        }

        // Build the query for creators
        IQueryable<Creator> creatorsQuery = _context.Creators
            .Include(c => c.ExternalCreators)
            .Include(c => c.MusicTracks);

        // Filter by music folder if specified
        if (!string.IsNullOrEmpty(request.Input.MusicFolderId) && int.TryParse(request.Input.MusicFolderId, out int musicFolderId))
            creatorsQuery = creatorsQuery.Where(c =>
                c.ExternalCreators.Any(e => e.ProviderId == musicFolderId));

        // Get all creators and group them alphabetically
        List<Creator> creators = await creatorsQuery
            .Include(c => c.Albums)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        // Group creators by first letter (ignoring articles)
        Dictionary<string, List<Creator>> groupedCreators = GroupCreatorsByFirstLetter(creators);

        StarredData starredData = await StarredDataHelper.LoadStarredDataForUserAsync(_context, currentUser.Id, cancellationToken);

        // Convert to response format
        List<Index> indexes = groupedCreators
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => new Index
            {
                Name = kvp.Key,
                Artist = kvp.Value.Select(c => c.ToSubsonicIndexArtistResponse(starredData.StarredArtists.GetValueOrDefault(c.Id))).ToList()
            })
            .ToList();

        Response response = new Response
        {
            Indexes = new Indexes
            {
                LastModified = lastModified,
                IgnoredArticles = "The El La Los Las Le Les",
                Index = indexes,
                Child = creators.SelectMany(c => c.MusicTracks.Select(t => t.ToSubsonicSongResponse(starredData.StarredTracks.GetValueOrDefault(t.Id)))).ToList(),
                Shortcut = []
            }
        };

        return new SubsonicBaseResponse(response);
    }

    private async Task<long> GetLastModifiedTimestamp(long? ifModifiedSince, CancellationToken cancellationToken)
    {
        DateTime? latestUpdate = await _context.Creators
            .MaxAsync(c => (DateTime?)c.UpdatedAt, cancellationToken);

        if (latestUpdate.HasValue)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(latestUpdate.Value, TimeSpan.Zero);

            return dateTimeOffset.ToUnixTimeMilliseconds();
        }

        // Return current time if no creators exist
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private static Dictionary<string, List<Creator>> GroupCreatorsByFirstLetter(List<Creator> creators)
    {
        Dictionary<string, List<Creator>> groupedCreators = new Dictionary<string, List<Creator>>();
        string[] ignoredArticles = ["The", "El", "La", "Los", "Las", "Le", "Les"];

        foreach (Creator creator in creators)
        {
            string nameForSorting = GetNameForSorting(creator.Name, ignoredArticles);
            string firstLetter = nameForSorting.Length > 0
                ? nameForSorting[0].ToString().ToUpperInvariant()
                : "#";

            // Use # for non-alphabetic characters
            if (!char.IsLetter(firstLetter[0]))
                firstLetter = "#";

            if (!groupedCreators.ContainsKey(firstLetter))
                groupedCreators[firstLetter] = [];

            groupedCreators[firstLetter].Add(creator);
        }

        return groupedCreators;
    }

    private static string GetNameForSorting(string name, string[] ignoredArticles)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        foreach (string article in ignoredArticles)
        {
            string articleWithSpace = article + " ";
            if (name.StartsWith(articleWithSpace, StringComparison.OrdinalIgnoreCase))
                return name.Substring(articleWithSpace.Length).Trim();
        }

        return name;
    }
}