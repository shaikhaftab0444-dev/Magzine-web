using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital_Magzine.Migrations
{
    /// <inheritdoc />
    public partial class SingleCompanyPivot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DM_Magazines_DM_Publishers_PublisherProfileId",
                table: "DM_Magazines");

            migrationBuilder.DropTable(
                name: "DM_PublisherApplications");

            migrationBuilder.DropTable(
                name: "DM_Publishers");

            migrationBuilder.DropIndex(
                name: "IX_DM_Magazines_PublisherProfileId",
                table: "DM_Magazines");

            migrationBuilder.DropColumn(
                name: "PublisherProfileId",
                table: "DM_Magazines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PublisherProfileId",
                table: "DM_Magazines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DM_PublisherApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ApplicantName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BrandName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DM_PublisherApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DM_Publishers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DM_Publishers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DM_Magazines_PublisherProfileId",
                table: "DM_Magazines",
                column: "PublisherProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_DM_Magazines_DM_Publishers_PublisherProfileId",
                table: "DM_Magazines",
                column: "PublisherProfileId",
                principalTable: "DM_Publishers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
