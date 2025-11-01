using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public class MusicTrackConfiguration : BaseEntityConfiguration<MusicTrack>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MusicTrack> builder)
    {
        builder.ToTable("MusicTracks");

        builder.Property(x => x.AudioFileLocation)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.OriginalName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.FriendlyName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ThumbnailLocation)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.ExternalUrl)
            .IsRequired()
            .HasMaxLength(2048);

        // Foreign key relationship with Provider
        builder.HasOne(x => x.Provider)
            .WithMany()
            .HasForeignKey(x => x.MusicTrackProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-many relationship with Creator (configured in CreatorConfiguration)
        builder.HasMany(x => x.Creators)
            .WithMany(x => x.MusicTracks);

        // Index on external URL for faster lookups
        builder.HasIndex(x => x.ExternalUrl);

        // Index on friendly name for search functionality
        builder.HasIndex(x => x.FriendlyName);
    }
}