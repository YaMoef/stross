using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public sealed class PlaylistConfiguration : BaseEntityConfiguration<Playlist>
{
    public static readonly int NameMaxLength = 255;
    public static readonly int CommentMaxLength = 1024;
    public static readonly int CoverArtLocationMaxLength = 2048;

    protected override void ConfigureEntity(EntityTypeBuilder<Playlist> builder)
    {
        builder.ToTable("Playlists");

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("nextval('shared_entity_id_seq')")
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(NameMaxLength);

        builder.Property(x => x.Comment)
            .IsRequired()
            .HasMaxLength(CommentMaxLength);

        builder.Property(x => x.Public)
            .IsRequired();

        builder.Property(x => x.CoverArtLocation)
            .HasMaxLength(CoverArtLocationMaxLength);

        builder.HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Contributors)
            .WithMany();

        builder.HasMany(x => x.PlaylistMusicTracks)
            .WithOne(x => x.Playlist)
            .HasForeignKey(x => x.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OwnerId)
            .HasDatabaseName("IX_Playlists_OwnerId");

        builder.HasIndex(x => x.Public)
            .HasDatabaseName("IX_Playlists_Public");
    }
}
