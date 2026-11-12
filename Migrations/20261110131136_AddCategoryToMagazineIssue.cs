using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital_Magzine.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToMagazineIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "DM_MagazineIssues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DM_MagazineIssues_CategoryId",
                table: "DM_MagazineIssues",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_DM_MagazineIssues_DM_Categories_CategoryId",
                table: "DM_MagazineIssues",
                column: "CategoryId",
                principalTable: "DM_Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DM_MagazineIssues_DM_Categories_CategoryId",
                table: "DM_MagazineIssues");

            migrationBuilder.DropIndex(
                name: "IX_DM_MagazineIssues_CategoryId",
                table: "DM_MagazineIssues");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "DM_MagazineIssues");
        }
    }
}
