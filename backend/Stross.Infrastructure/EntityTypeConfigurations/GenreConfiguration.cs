using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public sealed class GenreConfiguration : BaseEntityConfiguration<Genre>
{
    public static readonly int NameMaxLength = 100;

    protected override void ConfigureEntity(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable("Genres");

        // Configure ID to use shared sequence
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("nextval('shared_entity_id_seq')")
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength);

        // Configure navigation collections to use backing fields
        builder.Navigation(x => x.Albums)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.MusicTracks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Ensure genre names are unique
        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("IX_Genres_Name_Unique");
    }
}
