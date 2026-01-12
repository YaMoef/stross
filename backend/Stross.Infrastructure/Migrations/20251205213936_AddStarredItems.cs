using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Stross.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStarredItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserStarredItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    MusicTrackId = table.Column<long>(type: "bigint", nullable: true),
                    AlbumId = table.Column<long>(type: "bigint", nullable: true),
                    ArtistId = table.Column<long>(type: "bigint", nullable: true),
                    UserId1 = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStarredItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStarredItems_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserStarredItems_Creators_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Creators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserStarredItems_MusicTracks_MusicTrackId",
                        column: x => x.MusicTrackId,
                        principalTable: "MusicTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserStarredItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserStarredItems_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserStarredItems_AlbumId",
                table: "UserStarredItems",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStarredItems_ArtistId",
                table: "UserStarredItems",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStarredItems_MusicTrackId",
                table: "UserStarredItems",
                column: "MusicTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStarredItems_UserId_AlbumId",
                table: "UserStarredItems",
                columns: new[] { "UserId", "AlbumId" },
                unique: true,
                filter: "\"AlbumId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserStarredItems_UserId_ArtistId",
                table: "UserStarredItems",
                columns: new[] { "UserId", "ArtistId" },
                unique: true,
                filter: "\"ArtistId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserStarredItems_UserId_MusicTrackId",
                table: "UserStarredItems",
                columns: new[] { "UserId", "MusicTrackId" },
                unique: true,
                filter: "\"MusicTrackId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserStarredItems_UserId1",
                table: "UserStarredItems",
                column: "UserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserStarredItems");
        }
    }
}
