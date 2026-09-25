using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LeanProd.Infrastructure.Common.Persistence.Migrations.Business
{
    /// <inheritdoc />
    public partial class BusinessInitialCreate : Migration
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

            migrationBuilder.CreateTable(
                name: "EquipmentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_EquipmentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErpDatabaseInfo",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    DatabaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstallationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationLegalName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    OrganizationTaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CountryCode = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false),
                    CreatedByApplicationVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SchemaVersion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpDatabaseInfo", x => x.Id);
                    table.CheckConstraint("CK_ErpDatabaseInfo_Singleton", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TradingName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LegalForm = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CountryCode = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatisticalNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompanyRegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultCurrencyCode = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultLanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LogoFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrintFooter = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                    table.CheckConstraint("CK_Organization_Singleton", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "ProductionCalendars",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CountryCode = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false),
                    RegionCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionCalendars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StorageLocationKinds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocationKinds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StorageLocationTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocationTypes", x => x.Id);
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
                name: "UnitOfMeasures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    LetterCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    QuantityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DecimalPlaces = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CycleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CycleLengthDays = table.Column<int>(type: "int", nullable: false),
                    CycleAnchorDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UsePreHolidayTemplate = table.Column<bool>(type: "bit", nullable: false),
                    UseHolidayTemplate = table.Column<bool>(type: "bit", nullable: false),
                    UseSaturdayTemplate = table.Column<bool>(type: "bit", nullable: false),
                    UseSundayTemplate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSchedules", x => x.Id);
                    table.CheckConstraint("CK_WorkSchedules_CycleLength", "[CycleLengthDays] BETWEEN 1 AND 366");
                    table.CheckConstraint("CK_WorkSchedules_CycleTypeLength", "([CycleType] = 'Daily' AND [CycleLengthDays] = 1) OR ([CycleType] = 'Weekly' AND [CycleLengthDays] = 7) OR [CycleType] = 'Custom'");
                });

            migrationBuilder.CreateTable(
                name: "WorkShifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("PK_WorkShifts", x => x.Id);
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
                name: "ProductionCalendarDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionCalendarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DayType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TransferredFromDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionCalendarDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionCalendarDays_ProductionCalendars_ProductionCalendarId",
                        column: x => x.ProductionCalendarId,
                        principalTable: "ProductionCalendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ArticleNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CatalogItemClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                        name: "FK_CatalogItems_CatalogItemClasses_CatalogItemClassId",
                        column: x => x.CatalogItemClassId,
                        principalTable: "CatalogItemClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogItems_UnitOfMeasures_BaseUnitOfMeasureId",
                        column: x => x.BaseUnitOfMeasureId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasureConversions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Multiplier = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: false),
                    Offset = table.Column<decimal>(type: "decimal(28,12)", precision: 28, scale: 12, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasureConversions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnitOfMeasureConversions_UnitOfMeasures_FromUnitId",
                        column: x => x.FromUnitId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UnitOfMeasureConversions_UnitOfMeasures_ToUnitId",
                        column: x => x.ToUnitId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitOfMeasureTranslations",
                columns: table => new
                {
                    UnitOfMeasureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitOfMeasureTranslations", x => new { x.UnitOfMeasureId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_UnitOfMeasureTranslations_UnitOfMeasures_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitOfMeasures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nchar(4)", fixedLength: true, maxLength: 4, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ParentDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Departments_WorkSchedules_WorkScheduleId",
                        column: x => x.WorkScheduleId,
                        principalTable: "WorkSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkScheduleDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayNumber = table.Column<int>(type: "int", nullable: false),
                    TypeOfDay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkScheduleDays", x => x.Id);
                    table.CheckConstraint("CK_WorkScheduleDays_DayNumber", "[DayNumber] BETWEEN 0 AND 366");
                    table.ForeignKey(
                        name: "FK_WorkScheduleDays_WorkSchedules_WorkScheduleId",
                        column: x => x.WorkScheduleId,
                        principalTable: "WorkSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "CatalogItemCostHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "InDevelopment"),
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
                name: "Brigades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brigades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brigades_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonnelNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InventoryNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EquipmentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentEquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommissionedOn = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipment_EquipmentTypes_EquipmentTypeId",
                        column: x => x.EquipmentTypeId,
                        principalTable: "EquipmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipment_Equipment_ParentEquipmentId",
                        column: x => x.ParentEquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StorageLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nchar(4)", fixedLength: true, maxLength: 4, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KindId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentStorageLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageLocations_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorageLocations_StorageLocationKinds_KindId",
                        column: x => x.KindId,
                        principalTable: "StorageLocationKinds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorageLocations_StorageLocations_ParentStorageLocationId",
                        column: x => x.ParentStorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnologyStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnologyStages_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkScheduleIntervals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkScheduleDayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    PaidMinutes = table.Column<int>(type: "int", nullable: false),
                    CrossesMidnight = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkScheduleIntervals", x => x.Id);
                    table.CheckConstraint("CK_WorkScheduleIntervals_PaidMinutes", "[PaidMinutes] BETWEEN 1 AND 1440");
                    table.CheckConstraint("CK_WorkScheduleIntervals_Sequence", "[SequenceNumber] > 0");
                    table.ForeignKey(
                        name: "FK_WorkScheduleIntervals_WorkScheduleDays_WorkScheduleDayId",
                        column: x => x.WorkScheduleDayId,
                        principalTable: "WorkScheduleDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkScheduleIntervals_WorkShifts_WorkShiftId",
                        column: x => x.WorkShiftId,
                        principalTable: "WorkShifts",
                        principalColumn: "Id",
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
                name: "BrigadeMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrigadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LaborParticipationCoefficient = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false, defaultValue: 1m),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrigadeMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrigadeMemberships_Brigades_BrigadeId",
                        column: x => x.BrigadeId,
                        principalTable: "Brigades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BrigadeMemberships_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentStateEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentStateEvents", x => x.Id);
                    table.CheckConstraint("CK_EquipmentStateEvents_Period", "[EndedAtUtc] IS NULL OR [EndedAtUtc] > [StartedAtUtc]");
                    table.ForeignKey(
                        name: "FK_EquipmentStateEvents_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<byte>(type: "tinyint", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StorageLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AddressType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CountryCode = table.Column<string>(type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: false),
                    Locality = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    AddressLine = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Gln = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.CheckConstraint("CK_Addresses_OneOwner", "(CASE WHEN [OrganizationId] IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN [DepartmentId] IS NOT NULL THEN 1 ELSE 0 END + CASE WHEN [StorageLocationId] IS NOT NULL THEN 1 ELSE 0 END) = 1");
                    table.ForeignKey(
                        name: "FK_Addresses_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StorageLocationTypeAssignments",
                columns: table => new
                {
                    StorageLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageLocationTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageLocationTypeAssignments", x => new { x.StorageLocationId, x.StorageLocationTypeId });
                    table.ForeignKey(
                        name: "FK_StorageLocationTypeAssignments_StorageLocationTypes_StorageLocationTypeId",
                        column: x => x.StorageLocationTypeId,
                        principalTable: "StorageLocationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorageLocationTypeAssignments_StorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "StorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CatalogTechnologyStages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogTechnologyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TechnologyStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StageNumber = table.Column<int>(type: "int", nullable: false),
                    PlannedDurationMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
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
                    table.UniqueConstraint("AK_CatalogTechnologyStages_Id_CatalogTechnologyId", x => new { x.Id, x.CatalogTechnologyId });
                    table.CheckConstraint("CK_CatalogTechnologyStages_StageNumber", "[StageNumber] > 0");
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStages_CatalogTechnologies_CatalogTechnologyId",
                        column: x => x.CatalogTechnologyId,
                        principalTable: "CatalogTechnologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStages_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CatalogTechnologyStages_TechnologyStages_TechnologyStageId",
                        column: x => x.TechnologyStageId,
                        principalTable: "TechnologyStages",
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

            migrationBuilder.InsertData(
                table: "StorageLocationKinds",
                columns: new[] { "Id", "Code", "IsActive" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "Premises", true },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "Territory", true },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "Container", true },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "Zone", true },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "Other", true }
                });

            migrationBuilder.InsertData(
                table: "StorageLocationTypes",
                columns: new[] { "Id", "Code", "IsActive" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "Warehouse", true },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "Production", true },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "QualityHold", true },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "Waste", true },
                    { new Guid("20000000-0000-0000-0000-000000000005"), "Other", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_DepartmentId_IsActive_IsPrimary",
                table: "Addresses",
                columns: new[] { "DepartmentId", "IsActive", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_Gln",
                table: "Addresses",
                column: "Gln",
                unique: true,
                filter: "[Gln] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_OrganizationId_IsActive_IsPrimary",
                table: "Addresses",
                columns: new[] { "OrganizationId", "IsActive", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_StorageLocationId_IsActive_IsPrimary",
                table: "Addresses",
                columns: new[] { "StorageLocationId", "IsActive", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_BatchPropertyValues_PropertyId_OptionId",
                table: "BatchPropertyValues",
                columns: new[] { "PropertyId", "OptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_BrigadeMemberships_BrigadeId_EmployeeId_StartedAtUtc",
                table: "BrigadeMemberships",
                columns: new[] { "BrigadeId", "EmployeeId", "StartedAtUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BrigadeMemberships_BrigadeId_StartedAtUtc_EndedAtUtc",
                table: "BrigadeMemberships",
                columns: new[] { "BrigadeId", "StartedAtUtc", "EndedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BrigadeMemberships_EmployeeId_StartedAtUtc_EndedAtUtc",
                table: "BrigadeMemberships",
                columns: new[] { "EmployeeId", "StartedAtUtc", "EndedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Brigades_Code",
                table: "Brigades",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brigades_DepartmentId",
                table: "Brigades",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Brigades_IsActive_Name",
                table: "Brigades",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemBatches_CatalogItemId_Number",
                table: "CatalogItemBatches",
                columns: new[] { "CatalogItemId", "Number" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemCostHistory_CatalogItemId_EffectiveFromUtc",
                table: "CatalogItemCostHistory",
                columns: new[] { "CatalogItemId", "EffectiveFromUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItemPropertyValues_PropertyId_OptionId",
                table: "CatalogItemPropertyValues",
                columns: new[] { "PropertyId", "OptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_BaseUnitOfMeasureId",
                table: "CatalogItems",
                column: "BaseUnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_CatalogItemClassId",
                table: "CatalogItems",
                column: "CatalogItemClassId");

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
                name: "IX_CatalogTechnologyStages_CatalogTechnologyId_StageNumber",
                table: "CatalogTechnologyStages",
                columns: new[] { "CatalogTechnologyId", "StageNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_EquipmentId",
                table: "CatalogTechnologyStages",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_Id_CatalogTechnologyId",
                table: "CatalogTechnologyStages",
                columns: new[] { "Id", "CatalogTechnologyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogTechnologyStages_TechnologyStageId",
                table: "CatalogTechnologyStages",
                column: "TechnologyStageId");

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
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_IsActive",
                table: "Departments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_WorkScheduleId",
                table: "Departments",
                column: "WorkScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsActive_LastName_FirstName",
                table: "Employees",
                columns: new[] { "IsActive", "LastName", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PersonnelNumber",
                table: "Employees",
                column: "PersonnelNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_DepartmentId",
                table: "Equipment",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EquipmentTypeId",
                table: "Equipment",
                column: "EquipmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_InventoryNumber",
                table: "Equipment",
                column: "InventoryNumber",
                unique: true,
                filter: "[InventoryNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_IsActive",
                table: "Equipment",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Name",
                table: "Equipment",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_ParentEquipmentId",
                table: "Equipment",
                column: "ParentEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentStateEvents_EquipmentId",
                table: "EquipmentStateEvents",
                column: "EquipmentId",
                unique: true,
                filter: "[EndedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentStateEvents_EquipmentId_StartedAtUtc",
                table: "EquipmentStateEvents",
                columns: new[] { "EquipmentId", "StartedAtUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentTypes_IsActive",
                table: "EquipmentTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentTypes_Name",
                table: "EquipmentTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ErpDatabaseInfo_DatabaseId",
                table: "ErpDatabaseInfo",
                column: "DatabaseId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_ProductionCalendarDays_ProductionCalendarId_Date",
                table: "ProductionCalendarDays",
                columns: new[] { "ProductionCalendarId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionCalendars_Code",
                table: "ProductionCalendars",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionCalendars_IsMain",
                table: "ProductionCalendars",
                column: "IsMain",
                unique: true,
                filter: "[IsMain] = 1 AND [IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocationKinds_Code",
                table: "StorageLocationKinds",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_Code",
                table: "StorageLocations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_DepartmentId",
                table: "StorageLocations",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_IsActive",
                table: "StorageLocations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_KindId",
                table: "StorageLocations",
                column: "KindId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocations_ParentStorageLocationId",
                table: "StorageLocations",
                column: "ParentStorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocationTypeAssignments_StorageLocationTypeId",
                table: "StorageLocationTypeAssignments",
                column: "StorageLocationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageLocationTypes_Code",
                table: "StorageLocationTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStages_Code",
                table: "TechnologyStages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStages_DepartmentId",
                table: "TechnologyStages",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStages_IsActive_Name",
                table: "TechnologyStages",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStages_Name_DepartmentId",
                table: "TechnologyStages",
                columns: new[] { "Name", "DepartmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStageTemplates_Code",
                table: "TechnologyStageTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyStageTemplates_IsActive_Name",
                table: "TechnologyStageTemplates",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasureConversions_FromUnitId_ToUnitId",
                table: "UnitOfMeasureConversions",
                columns: new[] { "FromUnitId", "ToUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasureConversions_ToUnitId",
                table: "UnitOfMeasureConversions",
                column: "ToUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasures_Code",
                table: "UnitOfMeasures",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasures_IsActive",
                table: "UnitOfMeasures",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UnitOfMeasures_LetterCode",
                table: "UnitOfMeasures",
                column: "LetterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkScheduleDays_WorkScheduleId_DayNumber_TypeOfDay",
                table: "WorkScheduleDays",
                columns: new[] { "WorkScheduleId", "DayNumber", "TypeOfDay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkScheduleIntervals_WorkScheduleDayId_WorkShiftId_SequenceNumber",
                table: "WorkScheduleIntervals",
                columns: new[] { "WorkScheduleDayId", "WorkShiftId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkScheduleIntervals_WorkShiftId",
                table: "WorkScheduleIntervals",
                column: "WorkShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_Code",
                table: "WorkSchedules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_Kind",
                table: "WorkSchedules",
                column: "Kind",
                unique: true,
                filter: "[Kind] = 'Main' AND [Status] = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_WorkShifts_Code",
                table: "WorkShifts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkShifts_IsActive",
                table: "WorkShifts",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "BatchPropertyValues");

            migrationBuilder.DropTable(
                name: "BrigadeMemberships");

            migrationBuilder.DropTable(
                name: "CatalogItemCostHistory");

            migrationBuilder.DropTable(
                name: "CatalogItemPropertyValues");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyMaterialSupplyRouteSteps");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyOperations");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyStageOutputs");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyStageTransitions");

            migrationBuilder.DropTable(
                name: "EquipmentStateEvents");

            migrationBuilder.DropTable(
                name: "ErpDatabaseInfo");

            migrationBuilder.DropTable(
                name: "ProductionCalendarDays");

            migrationBuilder.DropTable(
                name: "StorageLocationTypeAssignments");

            migrationBuilder.DropTable(
                name: "TechnologyStageTemplates");

            migrationBuilder.DropTable(
                name: "UnitOfMeasureConversions");

            migrationBuilder.DropTable(
                name: "UnitOfMeasureTranslations");

            migrationBuilder.DropTable(
                name: "WorkScheduleIntervals");

            migrationBuilder.DropTable(
                name: "Organization");

            migrationBuilder.DropTable(
                name: "CatalogItemBatches");

            migrationBuilder.DropTable(
                name: "Brigades");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "ItemPropertyOptions");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyMaterials");

            migrationBuilder.DropTable(
                name: "ProductionCalendars");

            migrationBuilder.DropTable(
                name: "StorageLocationTypes");

            migrationBuilder.DropTable(
                name: "WorkScheduleDays");

            migrationBuilder.DropTable(
                name: "WorkShifts");

            migrationBuilder.DropTable(
                name: "ItemPropertyDefinitions");

            migrationBuilder.DropTable(
                name: "CatalogTechnologyStages");

            migrationBuilder.DropTable(
                name: "StorageLocations");

            migrationBuilder.DropTable(
                name: "CatalogTechnologies");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "TechnologyStages");

            migrationBuilder.DropTable(
                name: "StorageLocationKinds");

            migrationBuilder.DropTable(
                name: "CatalogItems");

            migrationBuilder.DropTable(
                name: "EquipmentTypes");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "CatalogItemClasses");

            migrationBuilder.DropTable(
                name: "UnitOfMeasures");

            migrationBuilder.DropTable(
                name: "WorkSchedules");
        }
    }
}
