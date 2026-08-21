using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDefaultMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultDepartmentId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultStorageLocationId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DefaultDepartmentId",
                table: "AspNetUsers",
                column: "DefaultDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DefaultStorageLocationId",
                table: "AspNetUsers",
                column: "DefaultStorageLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Departments_DefaultDepartmentId",
                table: "AspNetUsers",
                column: "DefaultDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_StorageLocations_DefaultStorageLocationId",
                table: "AspNetUsers",
                column: "DefaultStorageLocationId",
                principalTable: "StorageLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Departments_DefaultDepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_StorageLocations_DefaultStorageLocationId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DefaultDepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DefaultStorageLocationId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultDepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultStorageLocationId",
                table: "AspNetUsers");
        }
    }
}
