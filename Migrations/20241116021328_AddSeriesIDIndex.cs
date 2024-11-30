using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchNest.Migrations
{
    /// <inheritdoc />
    public partial class AddSeriesIDIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Series_SeriesID",
                table: "Series",
                column: "SeriesID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Series_SeriesID",
                table: "Series");
        }
    }
}
