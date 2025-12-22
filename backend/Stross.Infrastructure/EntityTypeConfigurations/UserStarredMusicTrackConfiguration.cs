using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public sealed class UserStarredItemConfiguration : BaseEntityConfiguration<UserStarredItem>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserStarredItem> builder)
    {
        builder.ToTable("UserStarredItems");

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MusicTrack)
            .WithMany()
            .HasForeignKey(x => x.MusicTrackId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Album)
            .WithMany()
            .HasForeignKey(x => x.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Artist)
            .WithMany()
            .HasForeignKey(x => x.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.MusicTrackId })
            .IsUnique()
            .HasFilter("\"MusicTrackId\" IS NOT NULL")
            .HasDatabaseName("IX_UserStarredItems_UserId_MusicTrackId");

        builder.HasIndex(x => new { x.UserId, x.AlbumId })
            .IsUnique()
            .HasFilter("\"AlbumId\" IS NOT NULL")
            .HasDatabaseName("IX_UserStarredItems_UserId_AlbumId");

        builder.HasIndex(x => new { x.UserId, x.ArtistId })
            .IsUnique()
            .HasFilter("\"ArtistId\" IS NOT NULL")
            .HasDatabaseName("IX_UserStarredItems_UserId_ArtistId");
    }
}
