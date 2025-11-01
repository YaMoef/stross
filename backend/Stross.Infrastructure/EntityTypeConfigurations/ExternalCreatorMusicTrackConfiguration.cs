using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public class ExternalCreatorMusicTrackConfiguration : BaseEntityConfiguration<ExternalCreatorMusicTrack>
{
    public static readonly int ExternalIdMaxLength = 255;
    public static readonly int ExternalNameMaxLength = 500;
    public static readonly int ThumbnailLocationMaxLength = 2048;
    public static readonly int ExternalUrlMaxLength = 2048;

    protected override void ConfigureEntity(EntityTypeBuilder<ExternalCreatorMusicTrack> builder)
    {
        builder.ToTable("ExternalCreatorMusicTracks");

        builder.Property(x => x.IsDefaultMusicTrackProvider)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.ExternalId)
            .IsRequired()
            .HasMaxLength(ExternalIdMaxLength);

        builder.Property(x => x.ExternalName)
            .IsRequired()
            .HasMaxLength(ExternalNameMaxLength);

        builder.Property(x => x.ThumbnailLocation)
            .IsRequired()
            .HasMaxLength(ThumbnailLocationMaxLength);

        builder.Property(x => x.ExternalUrl)
            .IsRequired()
            .HasMaxLength(ExternalUrlMaxLength);

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
        builder.HasIndex(x => new
            {
                x.ExternalId,
                x.MusicTrackProviderId
            })
            .IsUnique();
    }
}