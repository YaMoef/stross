using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public class ProviderConfiguration : BaseEntityConfiguration<Provider>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Enabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(2048);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}