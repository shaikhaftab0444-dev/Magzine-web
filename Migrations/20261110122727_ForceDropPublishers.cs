using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Digital_Magzine.Migrations
{
    /// <inheritdoc />
    public partial class ForceDropPublishers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_DM_Magazines_DM_Publishers_PublisherProfileId')
                BEGIN
                    ALTER TABLE [DM_Magazines] DROP CONSTRAINT [FK_DM_Magazines_DM_Publishers_PublisherProfileId];
                END

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DM_Magazines_PublisherProfileId' AND object_id = OBJECT_ID('DM_Magazines'))
                BEGIN
                    DROP INDEX [IX_DM_Magazines_PublisherProfileId] ON [DM_Magazines];
                END

                IF COL_LENGTH('DM_Magazines', 'PublisherProfileId') IS NOT NULL
                BEGIN
                    DECLARE @constraintName NVARCHAR(200);
                    SELECT @constraintName = d.name 
                    FROM sys.default_constraints d 
                    JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
                    WHERE d.parent_object_id = OBJECT_ID(N'DM_Magazines') AND c.name = N'PublisherProfileId';

                    IF @constraintName IS NOT NULL
                        EXEC('ALTER TABLE [DM_Magazines] DROP CONSTRAINT [' + @constraintName + ']');

                    ALTER TABLE [DM_Magazines] DROP COLUMN [PublisherProfileId];
                END

                IF OBJECT_ID(N'[dbo].[DM_PublisherApplications]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [dbo].[DM_PublisherApplications];
                END

                IF OBJECT_ID(N'[dbo].[DM_Publishers]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [dbo].[DM_Publishers];
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
