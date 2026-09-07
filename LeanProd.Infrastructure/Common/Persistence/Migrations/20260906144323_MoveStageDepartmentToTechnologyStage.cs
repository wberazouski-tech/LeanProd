using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveStageDepartmentToTechnologyStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "TechnologyStages",
                type: "uniqueidentifier",
                nullable: true);

            // A reusable stage can have been used by legacy technologies with different
            // departments. Split it before dropping the usage-level department so every
            // resulting directory stage has one unambiguous department.
            migrationBuilder.Sql("""
                DECLARE @TechnologyStageId uniqueidentifier, @DepartmentId uniqueidentifier,
                    @CurrentDepartmentId uniqueidentifier, @NewTechnologyStageId uniqueidentifier;
                DECLARE StageDepartmentCursor CURSOR LOCAL FAST_FORWARD FOR
                    SELECT DISTINCT [TechnologyStageId], [DepartmentId]
                    FROM [CatalogTechnologyStages]
                    WHERE [DepartmentId] IS NOT NULL
                    ORDER BY [TechnologyStageId], [DepartmentId];

                OPEN StageDepartmentCursor;
                FETCH NEXT FROM StageDepartmentCursor INTO @TechnologyStageId, @DepartmentId;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    SELECT @CurrentDepartmentId = [DepartmentId]
                    FROM [TechnologyStages] WHERE [Id] = @TechnologyStageId;

                    IF @CurrentDepartmentId IS NULL
                        UPDATE [TechnologyStages] SET [DepartmentId] = @DepartmentId
                        WHERE [Id] = @TechnologyStageId;
                    ELSE IF @CurrentDepartmentId <> @DepartmentId
                    BEGIN
                        SET @NewTechnologyStageId = NEWID();
                        INSERT INTO [TechnologyStages]
                            ([Id], [Code], [Name], [Description], [IsActive], [DepartmentId],
                             [CreatedAtUtc], [CreatedByUserId], [UpdatedAtUtc], [UpdatedByUserId])
                        SELECT @NewTechnologyStageId,
                            CONCAT('STG-', REPLACE(CONVERT(varchar(36), @NewTechnologyStageId), '-', '')),
                            [Name], [Description], [IsActive], @DepartmentId,
                            [CreatedAtUtc], [CreatedByUserId], [UpdatedAtUtc], [UpdatedByUserId]
                        FROM [TechnologyStages] WHERE [Id] = @TechnologyStageId;

                        UPDATE [CatalogTechnologyStages]
                        SET [TechnologyStageId] = @NewTechnologyStageId
                        WHERE [TechnologyStageId] = @TechnologyStageId AND [DepartmentId] = @DepartmentId;
                    END;

                    FETCH NEXT FROM StageDepartmentCursor INTO @TechnologyStageId, @DepartmentId;
                END;
                CLOSE StageDepartmentCursor;
                DEALLOCATE StageDepartmentCursor;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogTechnologyStages_Departments_DepartmentId",
                table: "CatalogTechnologyStages");

            migrationBuilder.DropIndex(
                name: "IX_CatalogTechnologyStages_DepartmentId",
                table: "CatalogTechnologyStages");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "CatalogTechnologyStages");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStages_DepartmentId",
                table: "TechnologyStages",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStages_Name_DepartmentId",
                table: "TechnologyStages",
                columns: new[] { "Name", "DepartmentId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TechnologyStages_Departments_DepartmentId",
                table: "TechnologyStages",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "CatalogTechnologyStages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [CatalogTechnologyStages]
                SET [DepartmentId] = [TechnologyStages].[DepartmentId]
                FROM [CatalogTechnologyStages]
                INNER JOIN [TechnologyStages]
                    ON [TechnologyStages].[Id] = [CatalogTechnologyStages].[TechnologyStageId];
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_DepartmentId",
                table: "CatalogTechnologyStages",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogTechnologyStages_Departments_DepartmentId",
                table: "CatalogTechnologyStages",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_TechnologyStages_Departments_DepartmentId",
                table: "TechnologyStages");

            migrationBuilder.DropIndex(
                name: "IX_TechnologyStages_DepartmentId",
                table: "TechnologyStages");

            migrationBuilder.DropIndex(
                name: "IX_TechnologyStages_Name_DepartmentId",
                table: "TechnologyStages");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "TechnologyStages");
        }
    }
}
