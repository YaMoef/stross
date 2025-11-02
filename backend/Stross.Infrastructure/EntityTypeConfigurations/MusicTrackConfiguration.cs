using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public class MusicTrackConfiguration : BaseEntityConfiguration<MusicTrack>
{
    public static readonly int AudioFileLocationMaxLength = 2048;
    public static readonly int OriginalNameMaxLength = 500;
    public static readonly int FriendlyNameMaxLength = 500;
    public static readonly int ThumbnailLocationMaxLength = 2048;
    public static readonly int ExternalUrlMaxLength = 2048;

    protected override void ConfigureEntity(EntityTypeBuilder<MusicTrack> builder)
    {
        builder.ToTable("MusicTracks");

        builder.Property(x => x.AudioFileLocation)
            .IsRequired()
            .HasMaxLength(AudioFileLocationMaxLength);

        builder.Property(x => x.OriginalName)
            .IsRequired()
            .HasMaxLength(OriginalNameMaxLength);

        builder.Property(x => x.FriendlyName)
            .IsRequired()
            .HasMaxLength(FriendlyNameMaxLength);

        builder.Property(x => x.ThumbnailLocation)
            .IsRequired()
            .HasMaxLength(ThumbnailLocationMaxLength);

        builder.Property(x => x.ExternalUrl)
            .IsRequired()
            .HasMaxLength(ExternalUrlMaxLength);

        // Foreign key relationship with Provider
        builder.HasOne(x => x.Provider)
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
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