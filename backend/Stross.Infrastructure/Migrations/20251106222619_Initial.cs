using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Stross.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "shared_entity_id_seq",
                startValue: 10L);

            migrationBuilder.CreateTable(
                name: "Creators",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('shared_entity_id_seq')"),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('shared_entity_id_seq')"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('shared_entity_id_seq')"),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('shared_entity_id_seq')"),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    GenreId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ExternalCreators",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsDefaultExternalCreator = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExternalId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ExternalName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailLocation = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ExternalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    ProviderId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalCreators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalCreators_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExternalCreators_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlbumCreators",
                columns: table => new
                {
                    AlbumId = table.Column<long>(type: "bigint", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumCreators", x => new { x.AlbumId, x.CreatorId });
                    table.ForeignKey(
                        name: "FK_AlbumCreators_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumCreators_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalAlbums",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsDefaultExternalAlbum = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ExternalId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ExternalName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailLocation = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ExternalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ProviderId = table.Column<long>(type: "bigint", nullable: false),
                    AlbumId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalAlbums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalAlbums_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExternalAlbums_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusicTracks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('shared_entity_id_seq')"),
                    AudioFileLocation = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    OriginalName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FriendlyName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailLocation = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ExternalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    ProviderId = table.Column<long>(type: "bigint", nullable: false),
                    AlbumId = table.Column<long>(type: "bigint", nullable: false),
                    GenreId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusicTracks_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MusicTracks_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MusicTracks_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CreatorMusicTracks",
                columns: table => new
                {
                    MusicTrackId = table.Column<long>(type: "bigint", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreatorMusicTracks", x => new { x.MusicTrackId, x.CreatorId });
                    table.ForeignKey(
                        name: "FK_CreatorMusicTracks_Creators_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Creators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreatorMusicTracks_MusicTracks_MusicTrackId",
                        column: x => x.MusicTrackId,
                        principalTable: "MusicTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumCreators_CreatorId",
                table: "AlbumCreators",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_GenreId",
                table: "Albums",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Name",
                table: "Albums",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CreatorMusicTracks_CreatorId",
                table: "CreatorMusicTracks",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalAlbums_AlbumId",
                table: "ExternalAlbums",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalAlbums_ExternalName",
                table: "ExternalAlbums",
                column: "ExternalName");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalAlbums_Provider_ExternalId_Unique",
                table: "ExternalAlbums",
                columns: new[] { "ProviderId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCreators_CreatorId",
                table: "ExternalCreators",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCreators_ExternalId_ProviderId",
                table: "ExternalCreators",
                columns: new[] { "ExternalId", "ProviderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCreators_ProviderId",
                table: "ExternalCreators",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_Name_Unique",
                table: "Genres",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_AlbumId",
                table: "MusicTracks",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_ExternalUrl",
                table: "MusicTracks",
                column: "ExternalUrl");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_FriendlyName",
                table: "MusicTracks",
                column: "FriendlyName");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_GenreId",
                table: "MusicTracks",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_ProviderId",
                table: "MusicTracks",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_Name",
                table: "Providers",
                column: "Name",
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO ""Genres"" (""Id"", ""Name"", ""CreatedAt"", ""UpdatedAt"", ""CreatedBy"", ""UpdatedBy"")
                VALUES (1, 'Unknown', NOW(), NULL, 0, 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumCreators");

            migrationBuilder.DropTable(
                name: "CreatorMusicTracks");

            migrationBuilder.DropTable(
                name: "ExternalAlbums");

            migrationBuilder.DropTable(
                name: "ExternalCreators");

            migrationBuilder.DropTable(
                name: "MusicTracks");

            migrationBuilder.DropTable(
                name: "Creators");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "Providers");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropSequence(
                name: "shared_entity_id_seq");
        }
    }
}
