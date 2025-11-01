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

    public DbSet<Creator> Creators { get; init; }
    public DbSet<ExternalCreatorMusicTrack> ExternalCreatorMusicTracks { get; init; }
    public DbSet<MusicTrack> MusicTracks { get; init; }
    public DbSet<Provider> Providers { get; init; }
}