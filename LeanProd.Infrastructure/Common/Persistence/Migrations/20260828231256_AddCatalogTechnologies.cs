using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogTechnologies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogTechnologies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CatalogItemClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_CatalogTechnologies", x => x.Id);
                    table.CheckConstraint("CK_CatalogTechnologies_Target", "([CatalogItemId] IS NOT NULL AND [CatalogItemClassId] IS NULL) OR ([CatalogItemId] IS NULL AND [CatalogItemClassId] IS NOT NULL)");
                    table.CheckConstraint("CK_CatalogTechnologies_ValidPeriod", "[ValidTo] IS NULL OR [ValidFrom] IS NULL OR [ValidTo] >= [ValidFrom]");
                    table.ForeignKey(
                        name: "FK_CatalogTechnologies_CatalogItemClasses_CatalogItemClassId",
                        column: x => x.CatalogItemClassId,
                        principalTable: "CatalogItemClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologies_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TechnologyStageTemplates",
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
                    table.PrimaryKey("PK_TechnologyStageTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogTechnologyStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogTechnologyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StageTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    PlannedDurationMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogTechnologyStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStages_CatalogTechnologies_CatalogTechnologyId",
                        column: x => x.CatalogTechnologyId,
                        principalTable: "CatalogTechnologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStages_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStages_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStages_TechnologyStageTemplates_StageTemplateId",
                        column: x => x.StageTemplateId,
                        principalTable: "TechnologyStageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogTechnologyMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnologyStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ConsumptionTrackingMode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DefaultSourceStorageLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScrapPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogTechnologyMaterials", x => x.Id);
                    table.CheckConstraint("CK_CatalogTechnologyMaterials_Quantity", "[Quantity] > 0");
                    table.CheckConstraint("CK_CatalogTechnologyMaterials_ScrapPercent", "[ScrapPercent] >= 0 AND [ScrapPercent] <= 100");
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyMaterials_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyMaterials_CatalogTechnologyStages_TechnologyStageId",
                        column: x => x.TechnologyStageId,
                        principalTable: "CatalogTechnologyStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyMaterials_StorageLocations_DefaultSourceStorageLocationId",
                        column: x => x.DefaultSourceStorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyMaterials_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogTechnologyOperations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnologyStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SetupMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RunMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LaborMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Workers = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogTechnologyOperations", x => x.Id);
                    table.CheckConstraint("CK_CatalogTechnologyOperations_Minutes", "[SetupMinutes] >= 0 AND [RunMinutes] >= 0 AND [LaborMinutes] >= 0");
                    table.CheckConstraint("CK_CatalogTechnologyOperations_Workers", "[Workers] > 0");
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyOperations_CatalogTechnologyStages_TechnologyStageId",
                        column: x => x.TechnologyStageId,
                        principalTable: "CatalogTechnologyStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyOperations_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyOperations_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogTechnologyStageLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogTechnologyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LagMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "CatalogTechnologyStageOutputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnologyStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReceiptStorageLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogTechnologyStageOutputs", x => x.Id);
                    table.CheckConstraint("CK_CatalogTechnologyStageOutputs_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStageOutputs_CatalogItems_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "CatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStageOutputs_CatalogTechnologyStages_TechnologyStageId",
                        column: x => x.TechnologyStageId,
                        principalTable: "CatalogTechnologyStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStageOutputs_StorageLocations_ReceiptStorageLocationId",
                        column: x => x.ReceiptStorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStageOutputs_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogTechnologyMaterialSupplyRouteSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnologyMaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    FromStorageLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToStorageLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsConsumptionPoint = table.Column<bool>(type: "bit", nullable: false),
                    MovementKind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LeadTimeMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogTechnologyMaterialSupplyRouteSteps", x => x.Id);
                    table.CheckConstraint("CK_CatalogTechnologyMaterialSupplyRouteSteps_LeadTime", "[LeadTimeMinutes] >= 0");
                    table.CheckConstraint("CK_CatalogTechnologyMaterialSupplyRouteSteps_Target", "([ToStorageLocationId] IS NOT NULL AND [IsConsumptionPoint] = 0) OR ([ToStorageLocationId] IS NULL AND [IsConsumptionPoint] = 1)");
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyMaterialSupplyRouteSteps_CatalogTechnologyMaterials_TechnologyMaterialId",
                        column: x => x.TechnologyMaterialId,
                        principalTable: "CatalogTechnologyMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyMaterialSupplyRouteSteps_StorageLocations_FromStorageLocationId",
                        column: x => x.FromStorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyMaterialSupplyRouteSteps_StorageLocations_ToStorageLocationId",
                        column: x => x.ToStorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologies_CatalogItemClassId_IsDefault",
                table: "CatalogTechnologies",
                columns: new[] { "CatalogItemClassId", "IsDefault" },
                unique: true,
                filter: "[CatalogItemClassId] IS NOT NULL AND [IsDefault] = 1 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologies_CatalogItemId_IsDefault",
                table: "CatalogTechnologies",
                columns: new[] { "CatalogItemId", "IsDefault" },
                unique: true,
                filter: "[CatalogItemId] IS NOT NULL AND [IsDefault] = 1 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologies_Code",
                table: "CatalogTechnologies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologies_IsActive_Name",
                table: "CatalogTechnologies",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyMaterials_CatalogItemId",
                table: "CatalogTechnologyMaterials",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyMaterials_DefaultSourceStorageLocationId",
                table: "CatalogTechnologyMaterials",
                column: "DefaultSourceStorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyMaterials_TechnologyStageId",
                table: "CatalogTechnologyMaterials",
                column: "TechnologyStageId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyMaterials_UnitOfMeasureId",
                table: "CatalogTechnologyMaterials",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyMaterialSupplyRouteSteps_FromStorageLocationId",
                table: "CatalogTechnologyMaterialSupplyRouteSteps",
                column: "FromStorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyMaterialSupplyRouteSteps_TechnologyMaterialId_LineNo",
                table: "CatalogTechnologyMaterialSupplyRouteSteps",
                columns: new[] { "TechnologyMaterialId", "LineNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyMaterialSupplyRouteSteps_ToStorageLocationId",
                table: "CatalogTechnologyMaterialSupplyRouteSteps",
                column: "ToStorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyOperations_DepartmentId",
                table: "CatalogTechnologyOperations",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyOperations_EquipmentId",
                table: "CatalogTechnologyOperations",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyOperations_TechnologyStageId_Code",
                table: "CatalogTechnologyOperations",
                columns: new[] { "TechnologyStageId", "Code" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStageOutputs_CatalogItemId",
                table: "CatalogTechnologyStageOutputs",
                column: "CatalogItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStageOutputs_ReceiptStorageLocationId",
                table: "CatalogTechnologyStageOutputs",
                column: "ReceiptStorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStageOutputs_TechnologyStageId",
                table: "CatalogTechnologyStageOutputs",
                column: "TechnologyStageId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStageOutputs_UnitOfMeasureId",
                table: "CatalogTechnologyStageOutputs",
                column: "UnitOfMeasureId");

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
                name: "IX_CatalogTechnologyStages_DepartmentId",
                table: "CatalogTechnologyStages",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_EquipmentId",
                table: "CatalogTechnologyStages",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_StageTemplateId",
                table: "CatalogTechnologyStages",
                column: "StageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStageTemplates_Code",
                table: "TechnologyStageTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStageTemplates_IsActive_Name",
                table: "TechnologyStageTemplates",
                columns: new[] { "IsActive", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogTechnologyMaterialSupplyRouteSteps");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyOperations");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyStageLinks");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyStageOutputs");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyMaterials");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyStages");

            migrationBuilder.DropTable(
                name: "CatalogTechnologies");

            migrationBuilder.DropTable(
                name: "TechnologyStageTemplates");
        }
    }
}
