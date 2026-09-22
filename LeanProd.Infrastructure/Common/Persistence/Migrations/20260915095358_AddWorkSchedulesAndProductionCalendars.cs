using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeanProd.Infrastructure.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkSchedulesAndProductionCalendars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkScheduleId",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Departments_WorkScheduleId",
                table: "Departments",
                column: "WorkScheduleId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_WorkSchedules_WorkScheduleId",
                table: "Departments",
                column: "WorkScheduleId",
                principalTable: "WorkSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_WorkSchedules_WorkScheduleId",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "ProductionCalendarDays");

            migrationBuilder.DropTable(
                name: "WorkScheduleIntervals");

            migrationBuilder.DropTable(
                name: "ProductionCalendars");

            migrationBuilder.DropTable(
                name: "WorkScheduleDays");

            migrationBuilder.DropTable(
                name: "WorkShifts");

            migrationBuilder.DropTable(
                name: "WorkSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Departments_WorkScheduleId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "WorkScheduleId",
                table: "Departments");
        }
    }
}
