using LeanProd.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanProd.Infrastructure.Features.Scheduling.Persistence;

internal sealed class WorkScheduleConfiguration : IEntityTypeConfiguration<WorkSchedule>
{
    public void Configure(EntityTypeBuilder<WorkSchedule> b)
    {
        b.ToTable("WorkSchedules", t =>
        {
            t.HasCheckConstraint("CK_WorkSchedules_CycleLength", "[CycleLengthDays] BETWEEN 1 AND 366");
            t.HasCheckConstraint("CK_WorkSchedules_CycleTypeLength", "([CycleType] = 'Daily' AND [CycleLengthDays] = 1) OR ([CycleType] = 'Weekly' AND [CycleLengthDays] = 7) OR [CycleType] = 'Custom'");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(30).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.CycleType).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
        b.Property(x => x.CycleAnchorDate).HasColumnType("date");
        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => x.Kind).IsUnique().HasFilter("[Kind] = 'Main' AND [Status] = 'Active'");
    }
}

internal sealed class WorkShiftConfiguration : IEntityTypeConfiguration<WorkShift>
{
    public void Configure(EntityTypeBuilder<WorkShift> b)
    {
        b.ToTable("WorkShifts"); b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(30).IsRequired(); b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000); b.HasIndex(x => x.Code).IsUnique(); b.HasIndex(x => x.IsActive);
    }
}

internal sealed class WorkScheduleDayConfiguration : IEntityTypeConfiguration<WorkScheduleDay>
{
    public void Configure(EntityTypeBuilder<WorkScheduleDay> b)
    {
        b.ToTable("WorkScheduleDays", t => t.HasCheckConstraint("CK_WorkScheduleDays_DayNumber", "[DayNumber] BETWEEN 0 AND 366"));
        b.HasKey(x => x.Id); b.Property(x => x.TypeOfDay).HasConversion<string>().HasMaxLength(20); b.Property(x => x.Name).HasMaxLength(200);
        b.HasOne(x => x.WorkSchedule).WithMany(x => x.Days).HasForeignKey(x => x.WorkScheduleId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.WorkScheduleId, x.DayNumber, x.TypeOfDay }).IsUnique();
    }
}

internal sealed class WorkScheduleIntervalConfiguration : IEntityTypeConfiguration<WorkScheduleInterval>
{
    public void Configure(EntityTypeBuilder<WorkScheduleInterval> b)
    {
        b.ToTable("WorkScheduleIntervals", t =>
        {
            t.HasCheckConstraint("CK_WorkScheduleIntervals_Sequence", "[SequenceNumber] > 0");
            t.HasCheckConstraint("CK_WorkScheduleIntervals_PaidMinutes", "[PaidMinutes] BETWEEN 1 AND 1440");
        });
        b.HasKey(x => x.Id); b.Property(x => x.StartTime).HasColumnType("time(0)"); b.Property(x => x.EndTime).HasColumnType("time(0)");
        b.HasOne(x => x.WorkScheduleDay).WithMany(x => x.Intervals).HasForeignKey(x => x.WorkScheduleDayId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.WorkShift).WithMany(x => x.Intervals).HasForeignKey(x => x.WorkShiftId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.WorkScheduleDayId, x.WorkShiftId, x.SequenceNumber }).IsUnique();
    }
}

internal sealed class ProductionCalendarConfiguration : IEntityTypeConfiguration<ProductionCalendar>
{
    public void Configure(EntityTypeBuilder<ProductionCalendar> b)
    {
        b.ToTable("ProductionCalendars"); b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(30).IsRequired(); b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.CountryCode).HasMaxLength(2).IsFixedLength().IsRequired(); b.Property(x => x.RegionCode).HasMaxLength(20);
        b.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired(); b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => x.IsMain).IsUnique().HasFilter("[IsMain] = 1 AND [IsActive] = 1");
    }
}

internal sealed class ProductionCalendarDayConfiguration : IEntityTypeConfiguration<ProductionCalendarDay>
{
    public void Configure(EntityTypeBuilder<ProductionCalendarDay> b)
    {
        b.ToTable("ProductionCalendarDays"); b.HasKey(x => x.Id);
        b.Property(x => x.Date).HasColumnType("date"); b.Property(x => x.TransferredFromDate).HasColumnType("date");
        b.Property(x => x.DayType).HasConversion<string>().HasMaxLength(30); b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.HasOne(x => x.ProductionCalendar).WithMany(x => x.Days).HasForeignKey(x => x.ProductionCalendarId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.ProductionCalendarId, x.Date }).IsUnique();
    }
}
