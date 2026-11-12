using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital_Magzine.Migrations
{
    /// <inheritdoc />
    public partial class MarketplaceArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MagazineId",
                table: "DM_MagazineIssues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                table: "DM_MagazineIssues",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SingleIssuePrice",
                table: "DM_MagazineIssues",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "DM_Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DM_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DM_Publishers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BrandName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DM_Publishers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DM_Magazines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublisherProfileId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubscriptionPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DM_Magazines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DM_Magazines_DM_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "DM_Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DM_Magazines_DM_Publishers_PublisherProfileId",
                        column: x => x.PublisherProfileId,
                        principalTable: "DM_Publishers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DM_Purchases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MagazineIssueId = table.Column<int>(type: "int", nullable: true),
                    MagazineId = table.Column<int>(type: "int", nullable: true),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdminCut = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PublisherCut = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DM_Purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DM_Purchases_DM_MagazineIssues_MagazineIssueId",
                        column: x => x.MagazineIssueId,
                        principalTable: "DM_MagazineIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DM_Purchases_DM_Magazines_MagazineId",
                        column: x => x.MagazineId,
                        principalTable: "DM_Magazines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DM_MagazineIssues_MagazineId",
                table: "DM_MagazineIssues",
                column: "MagazineId");

            migrationBuilder.CreateIndex(
                name: "IX_DM_Magazines_CategoryId",
                table: "DM_Magazines",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DM_Magazines_PublisherProfileId",
                table: "DM_Magazines",
                column: "PublisherProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DM_Purchases_MagazineId",
                table: "DM_Purchases",
                column: "MagazineId");

            migrationBuilder.CreateIndex(
                name: "IX_DM_Purchases_MagazineIssueId",
                table: "DM_Purchases",
                column: "MagazineIssueId");

            // Seed default Category, PublisherProfile, and Magazine so existing MagazineIssues have a valid foreign key target
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [DM_Categories])
                BEGIN
                    INSERT INTO [DM_Categories] ([Name], [Description])
                    VALUES ('General', 'General culture, style, and editorial topics');
                END

                DECLARE @catId INT = (SELECT TOP 1 [Id] FROM [DM_Categories]);

                IF NOT EXISTS (SELECT 1 FROM [DM_Publishers])
                BEGIN
                    INSERT INTO [DM_Publishers] ([UserId], [BrandName], [CommissionRate], [CreatedAt])
                    VALUES ('admin', 'Culture Club Media', 70.0, GETUTCDATE());
                END

                DECLARE @pubId INT = (SELECT TOP 1 [Id] FROM [DM_Publishers]);

                IF NOT EXISTS (SELECT 1 FROM [DM_Magazines])
                BEGIN
                    INSERT INTO [DM_Magazines] ([PublisherProfileId], [Title], [CategoryId], [SubscriptionPrice], [CreatedAt])
                    VALUES (@pubId, 'Culture Club Magazine', @catId, 29.99, GETUTCDATE());
                END

                DECLARE @magId INT = (SELECT TOP 1 [Id] FROM [DM_Magazines]);

                UPDATE [DM_MagazineIssues]
                SET [MagazineId] = @magId
                WHERE [MagazineId] = 0 OR [MagazineId] NOT IN (SELECT [Id] FROM [DM_Magazines]);
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_DM_MagazineIssues_DM_Magazines_MagazineId",
                table: "DM_MagazineIssues",
                column: "MagazineId",
                principalTable: "DM_Magazines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DM_MagazineIssues_DM_Magazines_MagazineId",
                table: "DM_MagazineIssues");

            migrationBuilder.DropTable(
                name: "DM_Purchases");

            migrationBuilder.DropTable(
                name: "DM_Magazines");

            migrationBuilder.DropTable(
                name: "DM_Categories");

            migrationBuilder.DropTable(
                name: "DM_Publishers");

            migrationBuilder.DropIndex(
                name: "IX_DM_MagazineIssues_MagazineId",
                table: "DM_MagazineIssues");

            migrationBuilder.DropColumn(
                name: "MagazineId",
                table: "DM_MagazineIssues");

            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                table: "DM_MagazineIssues");

            migrationBuilder.DropColumn(
                name: "SingleIssuePrice",
                table: "DM_MagazineIssues");
        }
    }
}
