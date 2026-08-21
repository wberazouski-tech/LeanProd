using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationAndAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql("""
                INSERT INTO [Organization]
                    ([Id], [LegalName], [CountryCode], [DefaultCurrencyCode], [TimeZoneId], [DefaultLanguageCode], [CreatedAtUtc])
                VALUES
                    (1, N'LeanProd', N'BY', N'BYN', N'Europe/Minsk', N'be', SYSUTCDATETIME());

                INSERT INTO [Addresses]
                    ([Id], [StorageLocationId], [AddressType], [CountryCode], [AddressLine], [IsPrimary], [IsActive], [CreatedAtUtc])
                SELECT NEWID(), [Id], N'Delivery', N'BY', LTRIM(RTRIM([Address])), 1, 1, SYSUTCDATETIME()
                FROM [StorageLocations]
                WHERE [Address] IS NOT NULL AND LTRIM(RTRIM([Address])) <> N'';
                """);

            migrationBuilder.DropColumn(
                name: "Address",
                table: "StorageLocations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "StorageLocations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE s
                SET s.[Address] = a.[AddressLine]
                FROM [StorageLocations] s
                INNER JOIN [Addresses] a ON a.[StorageLocationId] = s.[Id]
                WHERE a.[IsPrimary] = 1;
                """);

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Organization");

        }
    }
}
