using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public class ExternalCreatorMusicTrackConfiguration : BaseEntityConfiguration<ExternalCreatorMusicTrack>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ExternalCreatorMusicTrack> builder)
    {
        builder.ToTable("ExternalCreatorMusicTracks");

        builder.Property(x => x.IsDefaultMusicTrackProvider)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.ExternalId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.ExternalName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ThumbnailLocation)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.ExternalUrl)
            .IsRequired()
            .HasMaxLength(2048);

        // Foreign key relationships
        builder.HasOne(x => x.Creator)
            .WithMany(x => x.ExternalCreatorMusicTrack)
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Provider)
            .WithMany()
            .HasForeignKey(x => x.MusicTrackProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Composite unique index to prevent duplicate external creator entries per provider
        builder.HasIndex(x => new { x.ExternalId, x.MusicTrackProviderId })
            .IsUnique();
    }
}