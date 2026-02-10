using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public sealed class ExternalCreatorConfiguration : BaseEntityConfiguration<ExternalCreator>
{
    public static readonly int ExternalIdMaxLength = 255;
    public static readonly int ExternalNameMaxLength = 500;
    public static readonly int ThumbnailLocationMaxLength = 2048;
    public static readonly int ExternalUrlMaxLength = 2048;

    protected override void ConfigureEntity(EntityTypeBuilder<ExternalCreator> builder)
    {
        builder.ToTable("ExternalCreators");

        builder.Property(x => x.IsDefaultExternalCreator)
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
            .WithMany(x => x.ExternalCreators)
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Provider)
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Composite unique index to prevent duplicate external creator entries per provider
        builder.HasIndex(x => new
        {
            x.ExternalId,
            x.ProviderId
        })
            .IsUnique();
    }
}
