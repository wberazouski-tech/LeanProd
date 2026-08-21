using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeEquipmentStateIntervalsEditable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EquipmentStateEvents_EquipmentId_StartedAtUtc",
                table: "EquipmentStateEvents");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentStateEvents_EquipmentId_StartedAtUtc",
                table: "EquipmentStateEvents",
                columns: new[] { "EquipmentId", "StartedAtUtc" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EquipmentStateEvents_EquipmentId_StartedAtUtc",
                table: "EquipmentStateEvents");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentStateEvents_EquipmentId_StartedAtUtc",
                table: "EquipmentStateEvents",
                columns: new[] { "EquipmentId", "StartedAtUtc" });
        }
    }
}
