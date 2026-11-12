using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital_Magzine.Migrations
{
    /// <inheritdoc />
    public partial class AddMagazineDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DM_MagazineIssues",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "DM_MagazineIssues");
        }
    }
}
