using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public sealed class ProviderConfiguration : BaseEntityConfiguration<Provider>
{
    public static readonly int NameMaxLength = 255;
    public static readonly int UrlMaxLength = 2048;

    protected override void ConfigureEntity(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Providers");

        // Configure ID to use shared sequence
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("nextval('shared_entity_id_seq')")
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None);

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
