using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public sealed class PlaylistMusicTrackConfiguration : BaseEntityConfiguration<PlaylistMusicTrack>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PlaylistMusicTrack> builder)
    {
        builder.ToTable("PlaylistMusicTracks");

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.HasOne(x => x.Playlist)
            .WithMany(x => x.PlaylistMusicTracks)
            .HasForeignKey(x => x.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MusicTrack)
            .WithMany()
            .HasForeignKey(x => x.MusicTrackId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.PlaylistId, x.Order })
            .IsUnique()
            .HasDatabaseName("IX_PlaylistMusicTracks_PlaylistId_Order");

        builder.HasIndex(x => x.MusicTrackId)
            .HasDatabaseName("IX_PlaylistMusicTracks_MusicTrackId");
    }
}
