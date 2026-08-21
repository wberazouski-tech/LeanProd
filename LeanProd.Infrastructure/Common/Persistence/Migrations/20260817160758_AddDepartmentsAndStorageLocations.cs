using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentsAndStorageLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ParentDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                name: "StorageLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorageLocationTypeAssignments");

            migrationBuilder.DropTable(
                name: "StorageLocationTypes");

            migrationBuilder.DropTable(
                name: "StorageLocations");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "StorageLocationKinds");
        }
    }
}
