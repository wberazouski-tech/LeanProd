using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogItemCostHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogItemCostHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItemCostHistory", x => x.Id);
                    table.CheckConstraint("CK_CatalogItemCostHistory_Amount_NonNegative", "[Amount] >= 0");
                    table.ForeignKey(
                        name: "FK_CatalogItemCostHistory_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO [CatalogItemCostHistory]
                    ([Id], [CatalogItemId], [Amount], [EffectiveFromUtc], [CreatedAtUtc], [CreatedByUserId], [UpdatedAtUtc], [UpdatedByUserId])
                SELECT NEWID(), [Id], CAST(0 AS decimal(18,4)), SYSUTCDATETIME(), SYSUTCDATETIME(), NULL, NULL, NULL
                FROM [CatalogItems];
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemCostHistory_CatalogItemId_EffectiveFromUtc",
                table: "CatalogItemCostHistory",
                columns: new[] { "CatalogItemId", "EffectiveFromUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogItemCostHistory");
        }
    }
}
