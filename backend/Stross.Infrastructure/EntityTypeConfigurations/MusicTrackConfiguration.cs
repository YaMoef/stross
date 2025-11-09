using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public sealed class MusicTrackConfiguration : BaseEntityConfiguration<MusicTrack>
{
    public static readonly int AudioFileLocationMaxLength = 2048;
    public static readonly int OriginalNameMaxLength = 500;
    public static readonly int FriendlyNameMaxLength = 500;
    public static readonly int ThumbnailLocationMaxLength = 2048;
    public static readonly int ExternalUrlMaxLength = 2048;

    protected override void ConfigureEntity(EntityTypeBuilder<MusicTrack> builder)
    {
        builder.ToTable("MusicTracks");

        // Configure ID to use shared sequence
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("nextval('shared_entity_id_seq')")
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None);

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

        // Foreign key relationship with Album
        builder.HasOne(x => x.Album)
            .WithMany(x => x.MusicTracks)
            .HasForeignKey(x => x.AlbumId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Foreign key relationship with Genre
        builder.HasOne(x => x.Genre)
            .WithMany(g => g.MusicTracks)
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Many-to-many relationship with Creator
        builder.HasMany(x => x.Creators)
            .WithMany(x => x.MusicTracks)
            .UsingEntity(
                "MusicTrackCreators",
                l => l.HasOne(typeof(Creator)).WithMany().HasForeignKey("CreatorId"),
                r => r.HasOne(typeof(MusicTrack)).WithMany().HasForeignKey("MusicTrackId"),
                j => j.HasKey("MusicTrackId", "CreatorId"));

        // Indexes for performance
        builder.HasIndex(x => x.ExternalUrl)
            .HasDatabaseName("IX_MusicTracks_ExternalUrl");

        builder.HasIndex(x => x.FriendlyName)
            .HasDatabaseName("IX_MusicTracks_FriendlyName");

        builder.HasIndex(x => x.AlbumId)
            .HasDatabaseName("IX_MusicTracks_AlbumId");

        builder.HasIndex(x => x.GenreId)
            .HasDatabaseName("IX_MusicTracks_GenreId");
    }
}