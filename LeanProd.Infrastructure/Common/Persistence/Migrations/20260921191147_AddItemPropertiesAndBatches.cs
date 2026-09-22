using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddItemPropertiesAndBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogItemBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReceiptDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReceiptReference = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItemBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogItemBatches_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemPropertyDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogItemClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: true),
                    MaxLength = table.Column<int>(type: "int", nullable: true),
                    Minimum = table.Column<decimal>(type: "decimal(24,6)", precision: 24, scale: 6, nullable: true),
                    Maximum = table.Column<decimal>(type: "decimal(24,6)", precision: 24, scale: 6, nullable: true),
                    IsBatchProperty = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPropertyDefinitions", x => x.Id);
                    table.CheckConstraint("CK_ItemPropertyDefinitions_Settings", "([Type] = 'Number' AND [DecimalPlaces] BETWEEN 0 AND 6 AND [DecimalPlaces] IS NOT NULL AND [MaxLength] IS NULL AND [Minimum] IS NULL AND [Maximum] IS NULL) OR ([Type] = 'Range' AND [DecimalPlaces] BETWEEN 0 AND 6 AND [DecimalPlaces] IS NOT NULL AND [MaxLength] IS NULL AND [Minimum] IS NOT NULL AND [Maximum] IS NOT NULL AND [Minimum] <= [Maximum]) OR ([Type] = 'Text' AND [MaxLength] BETWEEN 1 AND 4000 AND [MaxLength] IS NOT NULL AND [DecimalPlaces] IS NULL AND [Minimum] IS NULL AND [Maximum] IS NULL) OR ([Type] IN ('Choice', 'Boolean') AND [MaxLength] IS NULL AND [DecimalPlaces] IS NULL AND [Minimum] IS NULL AND [Maximum] IS NULL)");
                    table.ForeignKey(
                        name: "FK_ItemPropertyDefinitions_CatalogItemClasses_CatalogItemClassId",
                        column: x => x.CatalogItemClassId,
                        principalTable: "CatalogItemClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemPropertyOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPropertyOptions", x => x.Id);
                    table.UniqueConstraint("AK_ItemPropertyOptions_PropertyId_Id", x => new { x.PropertyId, x.Id });
                    table.ForeignKey(
                        name: "FK_ItemPropertyOptions_ItemPropertyDefinitions_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "ItemPropertyDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BatchPropertyValues",
                columns: table => new
                {
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<decimal>(type: "decimal(24,6)", precision: 24, scale: 6, nullable: true),
                    Upper = table.Column<decimal>(type: "decimal(24,6)", precision: 24, scale: 6, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Boolean = table.Column<bool>(type: "bit", nullable: true),
                    OptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchPropertyValues", x => new { x.BatchId, x.PropertyId });
                    table.CheckConstraint("CK_BatchPropertyValues_Shape", "([Number] IS NOT NULL AND [Text] IS NULL AND [Boolean] IS NULL AND [OptionId] IS NULL AND ([Upper] IS NULL OR [Number] <= [Upper])) OR ([Text] IS NOT NULL AND [Number] IS NULL AND [Upper] IS NULL AND [Boolean] IS NULL AND [OptionId] IS NULL) OR ([Boolean] IS NOT NULL AND [Number] IS NULL AND [Upper] IS NULL AND [Text] IS NULL AND [OptionId] IS NULL) OR ([OptionId] IS NOT NULL AND [Number] IS NULL AND [Upper] IS NULL AND [Text] IS NULL AND [Boolean] IS NULL)");
                    table.ForeignKey(
                        name: "FK_BatchPropertyValues_CatalogItemBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "CatalogItemBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BatchPropertyValues_ItemPropertyDefinitions_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "ItemPropertyDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BatchPropertyValues_ItemPropertyOptions_PropertyId_OptionId",
                        columns: x => new { x.PropertyId, x.OptionId },
                        principalTable: "ItemPropertyOptions",
                        principalColumns: new[] { "PropertyId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogItemPropertyValues",
                columns: table => new
                {
                    CatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<decimal>(type: "decimal(24,6)", precision: 24, scale: 6, nullable: true),
                    Upper = table.Column<decimal>(type: "decimal(24,6)", precision: 24, scale: 6, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Boolean = table.Column<bool>(type: "bit", nullable: true),
                    OptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItemPropertyValues", x => new { x.CatalogItemId, x.PropertyId });
                    table.CheckConstraint("CK_CatalogItemPropertyValues_Shape", "([Number] IS NOT NULL AND [Text] IS NULL AND [Boolean] IS NULL AND [OptionId] IS NULL AND ([Upper] IS NULL OR [Number] <= [Upper])) OR ([Text] IS NOT NULL AND [Number] IS NULL AND [Upper] IS NULL AND [Boolean] IS NULL AND [OptionId] IS NULL) OR ([Boolean] IS NOT NULL AND [Number] IS NULL AND [Upper] IS NULL AND [Text] IS NULL AND [OptionId] IS NULL) OR ([OptionId] IS NOT NULL AND [Number] IS NULL AND [Upper] IS NULL AND [Text] IS NULL AND [Boolean] IS NULL)");
                    table.ForeignKey(
                        name: "FK_CatalogItemPropertyValues_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogItemPropertyValues_ItemPropertyDefinitions_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "ItemPropertyDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogItemPropertyValues_ItemPropertyOptions_PropertyId_OptionId",
                        columns: x => new { x.PropertyId, x.OptionId },
                        principalTable: "ItemPropertyOptions",
                        principalColumns: new[] { "PropertyId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchPropertyValues_PropertyId_OptionId",
                table: "BatchPropertyValues",
                columns: new[] { "PropertyId", "OptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemBatches_CatalogItemId_Number",
                table: "CatalogItemBatches",
                columns: new[] { "CatalogItemId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemPropertyValues_PropertyId_OptionId",
                table: "CatalogItemPropertyValues",
                columns: new[] { "PropertyId", "OptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemPropertyDefinitions_CatalogItemClassId_Code",
                table: "ItemPropertyDefinitions",
                columns: new[] { "CatalogItemClassId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemPropertyOptions_PropertyId_Label",
                table: "ItemPropertyOptions",
                columns: new[] { "PropertyId", "Label" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchPropertyValues");

            migrationBuilder.DropTable(
                name: "CatalogItemPropertyValues");

            migrationBuilder.DropTable(
                name: "CatalogItemBatches");

            migrationBuilder.DropTable(
                name: "ItemPropertyOptions");

            migrationBuilder.DropTable(
                name: "ItemPropertyDefinitions");
        }
    }
}
