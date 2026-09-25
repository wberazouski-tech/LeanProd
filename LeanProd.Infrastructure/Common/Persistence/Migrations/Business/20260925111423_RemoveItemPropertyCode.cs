using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations.Business
{
    /// <inheritdoc />
    public partial class RemoveItemPropertyCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemPropertyDefinitions_CatalogItemClassId_Code",
                table: "ItemPropertyDefinitions");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ItemPropertyDefinitions");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPropertyDefinitions_CatalogItemClassId",
                table: "ItemPropertyDefinitions",
                column: "CatalogItemClassId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new System.NotSupportedException("Restore the verified pre-migration backup to recover original property codes.");
        }
    }
}
