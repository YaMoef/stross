using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public class CreatorConfiguration : BaseEntityConfiguration<Creator>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Creator> builder)
    {
        builder.ToTable("Creators");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);

        // One-to-many relationship with ExternalCreatorMusicTrack
        builder.HasMany(x => x.ExternalCreatorMusicTrack)
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