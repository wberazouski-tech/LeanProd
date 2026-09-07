using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeTechnologyStagesAndTransitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LineNo",
                table: "CatalogTechnologyStages",
                newName: "StageNumber");

            migrationBuilder.AddColumn<Guid>(
                name: "TechnologyStageId",
                table: "CatalogTechnologyStages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_CatalogTechnologyStages_Id_CatalogTechnologyId",
                table: "CatalogTechnologyStages",
                columns: new[] { "Id", "CatalogTechnologyId" });

            migrationBuilder.CreateTable(
                name: "CatalogTechnologyStageTransitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogTechnologyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromCatalogTechnologyStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToCatalogTechnologyStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogTechnologyStageTransitions", x => x.Id);
                    table.CheckConstraint("CK_CatalogTechnologyStageTransitions_NoSelfTransition", "[FromCatalogTechnologyStageId] <> [ToCatalogTechnologyStageId]");
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStageTransitions_CatalogTechnologies_CatalogTechnologyId",
                        column: x => x.CatalogTechnologyId,
                        principalTable: "CatalogTechnologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStageTransitions_CatalogTechnologyStages_FromCatalogTechnologyStageId_CatalogTechnologyId",
                        columns: x => new { x.FromCatalogTechnologyStageId, x.CatalogTechnologyId },
                        principalTable: "CatalogTechnologyStages",
                        principalColumns: new[] { "Id", "CatalogTechnologyId" });
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStageTransitions_CatalogTechnologyStages_ToCatalogTechnologyStageId_CatalogTechnologyId",
                        columns: x => new { x.ToCatalogTechnologyStageId, x.CatalogTechnologyId },
                        principalTable: "CatalogTechnologyStages",
                        principalColumns: new[] { "Id", "CatalogTechnologyId" });
                });

            migrationBuilder.CreateTable(
                name: "TechnologyStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnologyStages", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO [TechnologyStages] ([Id], [Code], [Name], [Description], [IsActive], [CreatedAtUtc], [CreatedByUserId], [UpdatedAtUtc], [UpdatedByUserId])
                SELECT [Id],
                    CASE WHEN COUNT(*) OVER (PARTITION BY [Code]) = 1 THEN [Code]
                         ELSE CONCAT('STG-', REPLACE(CONVERT(varchar(36), [Id]), '-', '')) END,
                    [Name], [Description], CAST(1 AS bit), [CreatedAtUtc], [CreatedByUserId], [UpdatedAtUtc], [UpdatedByUserId]
                FROM [CatalogTechnologyStages];
                UPDATE [CatalogTechnologyStages] SET [TechnologyStageId] = [Id], [StageNumber] = [StageNumber] * 10;
                INSERT INTO [CatalogTechnologyStageTransitions] ([Id], [CatalogTechnologyId], [FromCatalogTechnologyStageId], [ToCatalogTechnologyStageId], [CreatedAtUtc], [CreatedByUserId], [UpdatedAtUtc], [UpdatedByUserId])
                SELECT [Id], [CatalogTechnologyId], [FromStageId], [ToStageId], [CreatedAtUtc], [CreatedByUserId], [UpdatedAtUtc], [UpdatedByUserId]
                FROM [CatalogTechnologyStageLinks];
                """);
            migrationBuilder.DropForeignKey(name: "FK_CatalogTechnologyStages_TechnologyStageTemplates_StageTemplateId", table: "CatalogTechnologyStages");
            migrationBuilder.DropTable(name: "CatalogTechnologyStageLinks");
            migrationBuilder.DropIndex(name: "IX_CatalogTechnologyStages_CatalogTechnologyId_Code", table: "CatalogTechnologyStages");
            migrationBuilder.DropIndex(name: "IX_CatalogTechnologyStages_CatalogTechnologyId_LineNo", table: "CatalogTechnologyStages");
            migrationBuilder.DropIndex(name: "IX_CatalogTechnologyStages_StageTemplateId", table: "CatalogTechnologyStages");
            migrationBuilder.DropColumn(name: "Code", table: "CatalogTechnologyStages");
            migrationBuilder.DropColumn(name: "Name", table: "CatalogTechnologyStages");
            migrationBuilder.DropColumn(name: "StageTemplateId", table: "CatalogTechnologyStages");
            migrationBuilder.AlterColumn<Guid>(name: "TechnologyStageId", table: "CatalogTechnologyStages", type: "uniqueidentifier", nullable: false, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_CatalogTechnologyId_StageNumber",
                table: "CatalogTechnologyStages",
                columns: new[] { "CatalogTechnologyId", "StageNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_Id_CatalogTechnologyId",
                table: "CatalogTechnologyStages",
                columns: new[] { "Id", "CatalogTechnologyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_TechnologyStageId",
                table: "CatalogTechnologyStages",
                column: "TechnologyStageId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CatalogTechnologyStages_StageNumber",
                table: "CatalogTechnologyStages",
                sql: "[StageNumber] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStageTransitions_CatalogTechnologyId_FromCatalogTechnologyStageId_ToCatalogTechnologyStageId",
                table: "CatalogTechnologyStageTransitions",
                columns: new[] { "CatalogTechnologyId", "FromCatalogTechnologyStageId", "ToCatalogTechnologyStageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStageTransitions_FromCatalogTechnologyStageId_CatalogTechnologyId",
                table: "CatalogTechnologyStageTransitions",
                columns: new[] { "FromCatalogTechnologyStageId", "CatalogTechnologyId" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStageTransitions_ToCatalogTechnologyStageId_CatalogTechnologyId",
                table: "CatalogTechnologyStageTransitions",
                columns: new[] { "ToCatalogTechnologyStageId", "CatalogTechnologyId" });

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStages_Code",
                table: "TechnologyStages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStages_IsActive_Name",
                table: "TechnologyStages",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogTechnologyStages_TechnologyStages_TechnologyStageId",
                table: "CatalogTechnologyStages",
                column: "TechnologyStageId",
                principalTable: "TechnologyStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogTechnologyStages_TechnologyStages_TechnologyStageId",
                table: "CatalogTechnologyStages");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyStageTransitions");

            migrationBuilder.DropTable(
                name: "TechnologyStages");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_CatalogTechnologyStages_Id_CatalogTechnologyId",
                table: "CatalogTechnologyStages");

            migrationBuilder.DropIndex(
                name: "IX_CatalogTechnologyStages_CatalogTechnologyId_StageNumber",
                table: "CatalogTechnologyStages");

            migrationBuilder.DropIndex(
                name: "IX_CatalogTechnologyStages_Id_CatalogTechnologyId",
                table: "CatalogTechnologyStages");

            migrationBuilder.DropIndex(
                name: "IX_CatalogTechnologyStages_TechnologyStageId",
                table: "CatalogTechnologyStages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CatalogTechnologyStages_StageNumber",
                table: "CatalogTechnologyStages");

            migrationBuilder.DropColumn(
                name: "TechnologyStageId",
                table: "CatalogTechnologyStages");

            migrationBuilder.RenameColumn(
                name: "StageNumber",
                table: "CatalogTechnologyStages",
                newName: "LineNo");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "CatalogTechnologyStages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CatalogTechnologyStages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "StageTemplateId",
                table: "CatalogTechnologyStages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CatalogTechnologyStageLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogTechnologyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LagMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LinkType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogTechnologyStageLinks", x => x.Id);
                    table.CheckConstraint("CK_CatalogTechnologyStageLinks_NoSelfLink", "[FromStageId] <> [ToStageId]");
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStageLinks_CatalogTechnologies_CatalogTechnologyId",
                        column: x => x.CatalogTechnologyId,
                        principalTable: "CatalogTechnologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStageLinks_CatalogTechnologyStages_FromStageId",
                        column: x => x.FromStageId,
                        principalTable: "CatalogTechnologyStages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStageLinks_CatalogTechnologyStages_ToStageId",
                        column: x => x.ToStageId,
                        principalTable: "CatalogTechnologyStages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_CatalogTechnologyId_Code",
                table: "CatalogTechnologyStages",
                columns: new[] { "CatalogTechnologyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_CatalogTechnologyId_LineNo",
                table: "CatalogTechnologyStages",
                columns: new[] { "CatalogTechnologyId", "LineNo" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_StageTemplateId",
                table: "CatalogTechnologyStages",
                column: "StageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStageLinks_CatalogTechnologyId_FromStageId_ToStageId_LinkType",
                table: "CatalogTechnologyStageLinks",
                columns: new[] { "CatalogTechnologyId", "FromStageId", "ToStageId", "LinkType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStageLinks_FromStageId",
                table: "CatalogTechnologyStageLinks",
                column: "FromStageId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStageLinks_ToStageId",
                table: "CatalogTechnologyStageLinks",
                column: "ToStageId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogTechnologyStages_TechnologyStageTemplates_StageTemplateId",
                table: "CatalogTechnologyStages",
                column: "StageTemplateId",
                principalTable: "TechnologyStageTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
