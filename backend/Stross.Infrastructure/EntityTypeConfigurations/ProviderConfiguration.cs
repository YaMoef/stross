using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public class ProviderConfiguration : BaseEntityConfiguration<Provider>
{
    public static readonly int NameMaxLength = 255;
    public static readonly int UrlMaxLength = 2048;

    protected override void ConfigureEntity(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength);

        builder.Property(x => x.Enabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(UrlMaxLength);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}