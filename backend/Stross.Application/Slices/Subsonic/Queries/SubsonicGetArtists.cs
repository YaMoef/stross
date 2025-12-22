using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Abstractions.Accessors;
using Stross.Application.Slices.Subsonic.Helpers;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Domain.Entities;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetArtistsQuery(SubsonicGetArtistsInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetArtistsQueryValidator : AbstractValidator<SubsonicGetArtistsQuery>
{
    public SubsonicGetArtistsQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetArtistsQueryHandler : IRequestHandler<SubsonicGetArtistsQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;
    private readonly IUserAccessor _userAccessor;

    public SubsonicGetArtistsQueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetArtistsQuery request, CancellationToken cancellationToken)
    {
        // Build the query for creators (artists)
        IQueryable<Creator> creatorsQuery = _context.Creators
            .Include(c => c.Albums)
            .Include(c => c.ExternalCreators);

        // Filter by music folder if specified
        if (!string.IsNullOrEmpty(request.Input.MusicFolderId) && int.TryParse(request.Input.MusicFolderId, out int musicFolderId))
            creatorsQuery = creatorsQuery.Where(c =>
                c.ExternalCreators.Any(e => e.ProviderId == musicFolderId));

        // Get all creators and group them alphabetically
        List<Creator> creators = await creatorsQuery
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        // Group creators by first letter (ignoring articles) - same logic as getIndexes but for ID3
        Dictionary<string, List<Creator>> groupedCreators = GroupCreatorsByFirstLetter(creators);

        Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);
        StarredData starredData = currentUser is not null
            ? await StarredDataHelper.LoadStarredDataForUserAsync(_context, currentUser.Id, cancellationToken)
            : new StarredData(new(), new(), new());

        // Convert to response format using ID3 response models
        List<IndexId3> indexes = groupedCreators
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => new IndexId3
            {
                Name = kvp.Key,
                Artist = kvp.Value.Select(c => c.ToSubsonicArtistID3Response(starredData.StarredArtists.GetValueOrDefault(c.Id))).ToList()
            })
            .ToList();

        Response response = new Response
        {
            Artists = new ArtistsId3
            {
                IgnoredArticles = "The El La Los Las Le Les",
                Index = indexes
            }
        };

        return new SubsonicBaseResponse(response);
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