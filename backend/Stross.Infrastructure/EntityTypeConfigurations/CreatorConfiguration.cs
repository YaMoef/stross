using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public sealed class CreatorConfiguration : BaseEntityConfiguration<Creator>
{
    public static readonly int NameMaxLength = 255;

    protected override void ConfigureEntity(EntityTypeBuilder<Creator> builder)
    {
        builder.ToTable("Creators");

        // Configure ID to use shared sequence
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("nextval('shared_entity_id_seq')")
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength);

        // One-to-many relationship with ExternalCreators
        builder.HasMany(x => x.ExternalCreators)
            .WithOne(x => x.Creator)
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-many relationship with MusicTrack
        builder.HasMany(x => x.MusicTracks)
            .WithMany(x => x.Creators)
            .UsingEntity<Dictionary<string, object>>(
                "CreatorMusicTracks",
                j => j.HasOne<MusicTrack>().WithMany().HasForeignKey("MusicTrackId"),
                j => j.HasOne<Creator>().WithMany().HasForeignKey("CreatorId"),
                j =>
                {
                    j.HasKey("CreatorId", "MusicTrackId");
                    j.ToTable("CreatorMusicTracks");
                });
    }
}
