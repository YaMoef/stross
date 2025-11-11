using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stross.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSizeAndDurationToMusicTrack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "MusicTracks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "MusicTracks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "MusicTracks");
        }
    }
}
