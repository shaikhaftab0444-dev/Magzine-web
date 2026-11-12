using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Digital_Magzine.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DM_PlatformSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DM_PlatformSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DM_PlatformSettings",
                columns: new[] { "Id", "SettingKey", "SettingValue" },
                values: new object[,]
                {
                    { 1, "BrandName", "Culture Club" },
                    { 2, "Tagline", "ALL THINGS THAT MATTER." },
                    { 3, "Currency", "INR" },
                    { 4, "Timezone", "UTC+05:30" },
                    { 5, "LogoPath", "/images/system/logo.png" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DM_PlatformSettings");
        }
    }
}
