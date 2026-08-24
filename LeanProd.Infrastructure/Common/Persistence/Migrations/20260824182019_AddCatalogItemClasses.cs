using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogItemClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogItemClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsGroup = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItemClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogItemClasses_CatalogItemClasses_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CatalogItemClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO [CatalogItemClasses] ([Id], [Type], [Code], [Name], [IsGroup], [ParentId], [IsActive], [CreatedAtUtc], [CreatedByUserId], [UpdatedAtUtc], [UpdatedByUserId])
                VALUES
                ('10000000-0000-0000-0000-000000000001', 'Product', 'GENERAL', 'General products', 0, NULL, 1, SYSUTCDATETIME(), NULL, NULL, NULL),
                ('10000000-0000-0000-0000-000000000002', 'Work', 'GENERAL', 'General works', 0, NULL, 1, SYSUTCDATETIME(), NULL, NULL, NULL),
                ('10000000-0000-0000-0000-000000000003', 'PrimaryMaterial', 'GENERAL', 'General primary materials', 0, NULL, 1, SYSUTCDATETIME(), NULL, NULL, NULL),
                ('10000000-0000-0000-0000-000000000004', 'AuxiliaryMaterial', 'GENERAL', 'General auxiliary materials', 0, NULL, 1, SYSUTCDATETIME(), NULL, NULL, NULL),
                ('10000000-0000-0000-0000-000000000005', 'SemiFinishedProduct', 'GENERAL', 'General semi-finished products', 0, NULL, 1, SYSUTCDATETIME(), NULL, NULL, NULL),
                ('10000000-0000-0000-0000-000000000006', 'Packaging', 'GENERAL', 'General packaging', 0, NULL, 1, SYSUTCDATETIME(), NULL, NULL, NULL),
                ('10000000-0000-0000-0000-000000000007', 'ToolingAndTools', 'GENERAL', 'General tooling and tools', 0, NULL, 1, SYSUTCDATETIME(), NULL, NULL, NULL),
                ('10000000-0000-0000-0000-000000000008', 'SparePart', 'GENERAL', 'General spare parts', 0, NULL, 1, SYSUTCDATETIME(), NULL, NULL, NULL),
                ('10000000-0000-0000-0000-000000000009', 'PurchasedService', 'GENERAL', 'General purchased services', 0, NULL, 1, SYSUTCDATETIME(), NULL, NULL, NULL),
                ('10000000-0000-0000-0000-000000000010', 'Waste', 'GENERAL', 'General waste', 0, NULL, 1, SYSUTCDATETIME(), NULL, NULL, NULL);
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "CatalogItemClassId",
                table: "CatalogItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [CatalogItems]
                SET [CatalogItemClassId] = CASE [Type]
                    WHEN 'Product' THEN '10000000-0000-0000-0000-000000000001'
                    WHEN 'Work' THEN '10000000-0000-0000-0000-000000000002'
                    WHEN 'PrimaryMaterial' THEN '10000000-0000-0000-0000-000000000003'
                    WHEN 'AuxiliaryMaterial' THEN '10000000-0000-0000-0000-000000000004'
                    WHEN 'SemiFinishedProduct' THEN '10000000-0000-0000-0000-000000000005'
                    WHEN 'Packaging' THEN '10000000-0000-0000-0000-000000000006'
                    WHEN 'ToolingAndTools' THEN '10000000-0000-0000-0000-000000000007'
                    WHEN 'SparePart' THEN '10000000-0000-0000-0000-000000000008'
                    WHEN 'PurchasedService' THEN '10000000-0000-0000-0000-000000000009'
                    WHEN 'Waste' THEN '10000000-0000-0000-0000-000000000010'
                END;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "CatalogItemClassId",
                table: "CatalogItems",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_CatalogItemClassId",
                table: "CatalogItems",
                column: "CatalogItemClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemClasses_ParentId",
                table: "CatalogItemClasses",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemClasses_Type_Code",
                table: "CatalogItemClasses",
                columns: new[] { "Type", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemClasses_Type_ParentId_IsActive_Name",
                table: "CatalogItemClasses",
                columns: new[] { "Type", "ParentId", "IsActive", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogItems_CatalogItemClasses_CatalogItemClassId",
                table: "CatalogItems",
                column: "CatalogItemClassId",
                principalTable: "CatalogItemClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogItems_CatalogItemClasses_CatalogItemClassId",
                table: "CatalogItems");

            migrationBuilder.DropTable(
                name: "CatalogItemClasses");

            migrationBuilder.DropIndex(
                name: "IX_CatalogItems_CatalogItemClassId",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "CatalogItemClassId",
                table: "CatalogItems");
        }
    }
}
