using System.Xml.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public sealed class ExternalAlbumConfiguration : BaseEntityConfiguration<ExternalAlbum>
{
    public static readonly int ExternalIdMaxLength = 255;
    public static readonly int ExternalNameMaxLength = 500;
    public static readonly int ThumbnailLocationMaxLength = 2048;
    public static readonly int ExternalUrlMaxLength = 2048;

    protected override void ConfigureEntity(EntityTypeBuilder<ExternalAlbum> builder)
    {
        builder.ToTable("ExternalAlbums");

        // Add AlbumId foreign key property
        builder.Property<long>("AlbumId")
            .IsRequired();

        builder.Property(x => x.IsDefaultExternalAlbum)
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

        builder.Property(x => x.ProviderId)
            .IsRequired();

        // Configure relationship with Provider
        builder.HasOne(x => x.Provider)
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Index for performance and uniqueness
        builder.HasIndex(x => new
            {
                x.ProviderId,
                x.ExternalId
            })
            .IsUnique()
            .HasDatabaseName("IX_ExternalAlbums_Provider_ExternalId_Unique");

        builder.HasIndex(x => x.ExternalName)
            .HasDatabaseName("IX_ExternalAlbums_ExternalName");

        builder.HasIndex("AlbumId")
            .HasDatabaseName("IX_ExternalAlbums_AlbumId");
    }
}