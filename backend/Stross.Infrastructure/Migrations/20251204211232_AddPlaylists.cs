using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Stross.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPlaylists : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Playlists",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('shared_entity_id_seq')"),
                Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Comment = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                Public = table.Column<bool>(type: "boolean", nullable: false),
                CoverArtLocation = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                OwnerId = table.Column<long>(type: "bigint", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                UpdatedBy = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Playlists", x => x.Id);
                table.ForeignKey(
                    name: "FK_Playlists_Users_OwnerId",
                    column: x => x.OwnerId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PlaylistMusicTracks",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Order = table.Column<int>(type: "integer", nullable: false),
                PlaylistId = table.Column<long>(type: "bigint", nullable: false),
                MusicTrackId = table.Column<long>(type: "bigint", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                UpdatedBy = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlaylistMusicTracks", x => x.Id);
                table.ForeignKey(
                    name: "FK_PlaylistMusicTracks_MusicTracks_MusicTrackId",
                    column: x => x.MusicTrackId,
                    principalTable: "MusicTracks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PlaylistMusicTracks_Playlists_PlaylistId",
                    column: x => x.PlaylistId,
                    principalTable: "Playlists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PlaylistUser",
            columns: table => new
            {
                ContributorsId = table.Column<long>(type: "bigint", nullable: false),
                PlaylistId = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlaylistUser", x => new { x.ContributorsId, x.PlaylistId });
                table.ForeignKey(
                    name: "FK_PlaylistUser_Playlists_PlaylistId",
                    column: x => x.PlaylistId,
                    principalTable: "Playlists",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlaylistUser_Users_ContributorsId",
                    column: x => x.ContributorsId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PlaylistMusicTracks_MusicTrackId",
            table: "PlaylistMusicTracks",
            column: "MusicTrackId");

        migrationBuilder.CreateIndex(
            name: "IX_PlaylistMusicTracks_PlaylistId_Order",
            table: "PlaylistMusicTracks",
            columns: new[] { "PlaylistId", "Order" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Playlists_OwnerId",
            table: "Playlists",
            column: "OwnerId");

        migrationBuilder.CreateIndex(
            name: "IX_Playlists_Public",
            table: "Playlists",
            column: "Public");

        migrationBuilder.CreateIndex(
            name: "IX_PlaylistUser_PlaylistId",
            table: "PlaylistUser",
            column: "PlaylistId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PlaylistMusicTracks");

        migrationBuilder.DropTable(
            name: "PlaylistUser");

        migrationBuilder.DropTable(
            name: "Playlists");
    }
}
