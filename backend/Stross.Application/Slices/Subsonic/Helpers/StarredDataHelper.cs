using Microsoft.EntityFrameworkCore;
using Stross.Domain.Entities;
using Stross.Infrastructure;

namespace Stross.Application.Slices.Subsonic.Helpers;

public sealed record StarredData(
    Dictionary<long, DateTime> StarredTracks,
    Dictionary<long, DateTime> StarredAlbums,
    Dictionary<long, DateTime> StarredArtists);

public static class StarredDataHelper
{
    public static async Task<StarredData> LoadStarredDataForUserAsync(
        StrossContext context,
        long userId,
        CancellationToken cancellationToken = default)
    {
        List<UserStarredItem> starredItems = await context.UserStarredItems
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);

        Dictionary<long, DateTime> starredTracks = starredItems
            .Where(s => s.MusicTrackId.HasValue)
            .ToDictionary(s => s.MusicTrackId!.Value, s => s.CreatedAt);

        Dictionary<long, DateTime> starredAlbums = starredItems
            .Where(s => s.AlbumId.HasValue)
            .ToDictionary(s => s.AlbumId!.Value, s => s.CreatedAt);

        Dictionary<long, DateTime> starredArtists = starredItems
            .Where(s => s.ArtistId.HasValue)
            .ToDictionary(s => s.ArtistId!.Value, s => s.CreatedAt);

        return new StarredData(starredTracks, starredAlbums, starredArtists);
    }
}
