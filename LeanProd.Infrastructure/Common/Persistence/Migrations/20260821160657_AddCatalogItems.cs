using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ArticleNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BaseUnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_CatalogItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogItems_UnitOfMeasures_BaseUnitOfMeasureId",
                        column: x => x.BaseUnitOfMeasureId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_BaseUnitOfMeasureId",
                table: "CatalogItems",
                column: "BaseUnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_Type_ArticleNumber",
                table: "CatalogItems",
                columns: new[] { "Type", "ArticleNumber" },
                unique: true,
                filter: "[ArticleNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_Type_IsActive_WorkingName",
                table: "CatalogItems",
                columns: new[] { "Type", "IsActive", "WorkingName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogItems");
        }
    }
}
