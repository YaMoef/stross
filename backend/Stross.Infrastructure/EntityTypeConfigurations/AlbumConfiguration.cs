using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public sealed class AlbumConfiguration : BaseEntityConfiguration<Album>
{
    public static readonly int NameMaxLength = 255;

    protected override void ConfigureEntity(EntityTypeBuilder<Album> builder)
    {
        builder.ToTable("Albums");

        // Configure ID to use shared sequence
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("nextval('shared_entity_id_seq')")
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength);

        // Configure relationship with ExternalAlbums (one-to-many)
        builder.HasMany(x => x.ExternalAlbums)
            .WithOne()
            .HasForeignKey("AlbumId")
            .OnDelete(DeleteBehavior.Cascade);

        // Configure many-to-many relationship with Creators
        builder.HasMany(x => x.Creators)
            .WithMany(x => x.Albums)
            .UsingEntity(
                "AlbumCreators",
                l => l.HasOne(typeof(Creator)).WithMany().HasForeignKey("CreatorId"),
                r => r.HasOne(typeof(Album)).WithMany().HasForeignKey("AlbumId"),
                j => j.HasKey("AlbumId", "CreatorId"));

        // Configure relationship with MusicTracks (one-to-many)
        builder.HasMany(x => x.MusicTracks)
            .WithOne()
            .HasForeignKey("AlbumId")
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
        
        // Foreign key relationship with Genre
        builder.HasOne(x => x.Genre)
            .WithMany(g => g.Albums)
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Index for performance
        builder.HasIndex(x => x.Name)
            .HasDatabaseName("IX_Albums_Name");
    }
}