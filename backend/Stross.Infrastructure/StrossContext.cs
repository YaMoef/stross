using Microsoft.EntityFrameworkCore;
using Stross.Domain.Entities;
using Stross.Infrastructure.Interceptors;

namespace Stross.Infrastructure;

public class StrossContext : DbContext
{
    public StrossContext(DbContextOptions<StrossContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new DateTimeInterceptor());

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("shared_entity_id_seq")
            .StartsAt(10)
            .IncrementsBy(1);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StrossContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Creator> Creators { get; init; }
    public DbSet<ExternalCreator> ExternalCreators { get; init; }
    public DbSet<MusicTrack> MusicTracks { get; init; }
    public DbSet<Provider> Providers { get; init; }
    public DbSet<Genre> Genres { get; init; }
    public DbSet<Album> Albums { get; init; }
    public DbSet<ExternalAlbum> ExternalAlbums { get; init; }
    public DbSet<User> Users { get; init; }
    public DbSet<Playlist> Playlists { get; init; }
    public DbSet<PlaylistMusicTrack> PlaylistMusicTracks { get; init; }
    public DbSet<UserStarredItem> UserStarredItems { get; init; }
}
