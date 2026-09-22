import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Observable, Subject, finalize, forkJoin, takeUntil } from 'rxjs';
import { TranslocoPipe } from '@jsverse/transloco';
import { FormFieldErrorComponent } from '../../core/ui/form-field-error.component';
import { AuthService } from '../../core/auth/auth.service';
import { Permissions } from '../../core/auth/permissions';
import { MasterDataService } from '../master-data/master-data.service';
import { OptionItem } from '../master-data/master-data.models';
import { Calendar, CalendarDayType, DayType, Interval, Schedule, ScheduleDay, ScheduleSummary, SchedulingApi, Shift } from './scheduling-api.service';

@Component({
  selector: 'app-scheduling',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslocoPipe, FormFieldErrorComponent],
  templateUrl: './scheduling.component.html',
  styleUrl: './scheduling.component.css'
})
export class SchedulingComponent implements OnInit, OnDestroy {
  private readonly api = inject(SchedulingApi);
  private readonly master = inject(MasterDataService);
  private readonly auth = inject(AuthService);
  private readonly destroyed = new Subject<void>();
  tab: 'schedules' | 'shifts' | 'calendar' = 'schedules';
  schedules: ScheduleSummary[] = [];
  shifts: Shift[] = [];
  calendars: Calendar[] = [];
  departments: OptionItem[] = [];
  schedule = this.newSchedule();
  shift = this.newShift();
  calendar = this.newCalendar();
  busy = false;
  loading = false;
  detailLoading = false;
  loadError = false;
  departmentPickerOpen = false;
  messageKey = '';
  errorKey = '';
  readonly calendarTypes: CalendarDayType[] = ['Holiday', 'PreHoliday', 'TransferredWorkingDay', 'TransferredDayOff', 'SpecialWorkingDay', 'SpecialDayOff'];

  get canManage(): boolean { return this.auth.hasPermission(Permissions.workSchedulesManage); }
  get locked(): boolean { return this.busy || this.loading || this.detailLoading || this.loadError; }
  get empty(): boolean { return (this.tab === 'schedules' ? this.schedules : this.tab === 'shifts' ? this.shifts : this.calendars).length === 0; }
  get activeShifts(): Shift[] { return this.shifts.filter(x => x.isActive); }
  get assignedDepartments(): OptionItem[] { return this.departments.filter(x => this.schedule.departmentIds.includes(x.id)); }
  get availableDepartments(): OptionItem[] { return this.departments.filter(x => !this.schedule.departmentIds.includes(x.id)); }

  ngOnInit(): void { this.reload(); }
  ngOnDestroy(): void { this.destroyed.next(); this.destroyed.complete(); }

  reload(): void {
    if (this.loading) return;
    this.loading = true;
    this.loadError = false;
    forkJoin({
      schedules: this.api.schedules(), shifts: this.api.shifts(),
      calendars: this.api.calendars(), departments: this.master.departmentOptions()
    }).pipe(takeUntil(this.destroyed), finalize(() => this.loading = false)).subscribe({
      next: data => {
        this.schedules = [...data.schedules].sort((left, right) =>
          Number(left.kind !== 'Main') - Number(right.kind !== 'Main') || left.code.localeCompare(right.code));
        this.shifts = data.shifts;
        this.calendars = data.calendars;
        this.departments = data.departments;
        const selectedId = this.schedule.id && this.schedules.some(x => x.id === this.schedule.id)
          ? this.schedule.id
          : this.schedules.find(x => x.status === 'Active')?.id ?? this.schedules[0]?.id;
        if (selectedId) this.loadSchedule(selectedId);
      },
      error: () => this.loadError = true
    });
  }

  private today(): string {
    const now = new Date();
    return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
  }
  newSchedule(): Schedule {
    return {
      code: '', name: '', kind: 'Optional', status: 'Draft', timeZoneId: 'Europe/Warsaw',
      cycleType: 'Weekly', cycleLengthDays: 7, cycleAnchorDate: this.today(),
      usePreHolidayTemplate: false, useHolidayTemplate: false, useSaturdayTemplate: false,
      useSundayTemplate: false, departmentIds: [],
      days: Array.from({ length: 7 }, (_, i) => ({ dayNumber: i + 1, typeOfDay: 'Workday', intervals: [] }))
    };
  }
  newShift(): Shift { return { code: '', name: '', isActive: true }; }
  newCalendar(): Calendar {
    return { code: '', name: '', countryCode: 'BY', timeZoneId: 'Europe/Warsaw', isMain: true, isActive: true, days: [] };
  }
  clearFeedback(): void { this.messageKey = ''; this.errorKey = ''; }
  createSchedule(): void { if (this.canManage && !this.locked) { this.schedule = this.newSchedule(); this.clearFeedback(); } }
  createShift(): void { if (this.canManage && !this.locked) { this.shift = this.newShift(); this.clearFeedback(); } }
  createCalendar(): void { if (this.canManage && !this.locked) { this.calendar = this.newCalendar(); this.clearFeedback(); } }

  selectSchedule(id: string): void {
    if (this.locked) return;
    this.clearFeedback();
    this.loadSchedule(id);
  }
  private loadSchedule(id: string): void {
    this.clearFeedback();
    this.detailLoading = true;
    this.api.schedule(id).pipe(takeUntil(this.destroyed), finalize(() => this.detailLoading = false)).subscribe({
      next: x => { this.schedule = x; this.departmentPickerOpen = false; },
      error: () => this.errorKey = 'scheduling.loadFailed'
    });
  }
  selectShift(x: Shift): void { if (!this.locked) { this.shift = structuredClone(x); this.clearFeedback(); } }
  selectCalendar(x: Calendar): void { if (!this.locked) { this.calendar = structuredClone(x); this.clearFeedback(); } }
  cycleChanged(): void {
    this.schedule.cycleLengthDays = this.schedule.cycleType === 'Daily' ? 1 : this.schedule.cycleType === 'Weekly' ? 7 : this.schedule.cycleLengthDays;
    this.rebuildDays();
  }
  rebuildDays(): void {
    const n = this.schedule.cycleLengthDays;
    if (!Number.isInteger(n) || n < 1 || n > 366) return;
    this.schedule.days = this.schedule.days.filter(x => x.typeOfDay !== 'Workday' || x.dayNumber <= n);
    for (let i = 1; i <= n; i++) {
      if (!this.schedule.days.some(x => x.typeOfDay === 'Workday' && x.dayNumber === i))
        this.schedule.days.push({ dayNumber: i, typeOfDay: 'Workday', intervals: [] });
    }
  }
  syncTemplate(type: DayType, on: boolean): void {
    if (on && !this.schedule.days.some(x => x.typeOfDay === type))
      this.schedule.days.push({ dayNumber: 0, typeOfDay: type, intervals: [] });
    if (!on) this.schedule.days = this.schedule.days.filter(x => x.typeOfDay !== type);
  }
  addInterval(day: ScheduleDay): void {
    if (!this.canManage || this.locked || !this.activeShifts.length) return;
    day.intervals.push({
      workShiftId: this.activeShifts[0].id!, sequenceNumber: Math.max(0, ...day.intervals.map(x => x.sequenceNumber)) + 1,
      startTime: '08:00:00', endTime: '16:00:00', paidMinutes: 480, crossesMidnight: false, isActive: true
    });
  }
  removeInterval(day: ScheduleDay, index: number): void {
    if (!this.canManage || this.locked) return;
    day.intervals.splice(index, 1);
    day.intervals.forEach((x, i) => x.sequenceNumber = i + 1);
  }
  private timeMinutes(value: string): number {
    if (!value || !/^([01]\d|2[0-3]):[0-5]\d(:[0-5]\d)?$/.test(value)) return NaN;
    const [hours, minutes] = value.split(':').map(Number);
    return hours * 60 + minutes;
  }
  minutes(x: Interval): number {
    return this.timeMinutes(x.endTime) - this.timeMinutes(x.startTime) + (x.crossesMidnight ? 1440 : 0);
  }
  intervalError(day: ScheduleDay): string {
    const ranges: { start: number; end: number }[] = [];
    for (const interval of day.intervals) {
      if (!this.activeShifts.some(x => x.id === interval.workShiftId)) return 'scheduling.validation.shift';
      const duration = this.minutes(interval);
      if (!Number.isFinite(duration) || duration <= 0 || duration > 1440) return 'scheduling.validation.duration';
      if (!interval.isActive) continue;
      const start = this.timeMinutes(interval.startTime), end = this.timeMinutes(interval.endTime);
      if (interval.crossesMidnight) {
        ranges.push({ start, end: 1440 });
        if (end > 0) ranges.push({ start: 0, end });
      } else ranges.push({ start, end });
    }
    ranges.sort((a, b) => a.start - b.start);
    return ranges.some((x, i) => i > 0 && ranges[i - 1].end > x.start) ? 'scheduling.validation.overlap' : '';
  }
  get scheduleError(): string {
    const s = this.schedule;
    if (!s.code.trim()) return 'scheduling.validation.scheduleCode';
    if (!s.name.trim()) return 'scheduling.validation.scheduleName';
    if (!s.timeZoneId.trim()) return 'scheduling.validation.scheduleTimeZone';
    if (!s.cycleAnchorDate) return 'scheduling.validation.scheduleAnchor';
    const expected = s.cycleType === 'Daily' ? 1 : s.cycleType === 'Weekly' ? 7 : s.cycleLengthDays;
    if (!Number.isInteger(expected) || expected < 1 || expected > 366 || s.cycleLengthDays !== expected) return 'scheduling.validation.cycle';
    if (s.departmentIds.length && (s.kind !== 'Optional' || s.status !== 'Active')) return 'scheduling.validation.assignment';
    return s.days.map(day => this.intervalError(day)).find(Boolean) || '';
  }
  get shiftError(): string { return this.shift.code.trim() && this.shift.name.trim() ? '' : 'scheduling.validation.required'; }
  get calendarError(): string {
    const c = this.calendar;
    if (c.countryCode.trim().length !== 2) return 'scheduling.validation.country';
    if (!c.code.trim() || !c.name.trim() || !c.timeZoneId.trim() ||
        c.days.some(x => !x.date || !x.name.trim())) return 'scheduling.validation.required';
    return new Set(c.days.map(x => x.date)).size !== c.days.length ? 'scheduling.validation.duplicateDate' : '';
  }
  addCalendarDay(): void {
    if (this.canManage && !this.locked) this.calendar.days.push({ date: this.today(), dayType: 'Holiday', name: '' });
  }
  removeCalendarDay(index: number): void {
    if (this.canManage && !this.locked) this.calendar.days.splice(index, 1);
  }
  private canSave(form: NgForm, error: string): boolean {
    if (!this.canManage || this.locked) return false;
    if (form.invalid || error) {
      form.control.markAllAsTouched();
      this.errorKey = error || 'scheduling.validation.required';
      return false;
    }
    return true;
  }
  saveSchedule(form: NgForm): void {
    if (!this.canSave(form, this.scheduleError)) return;
    const payload = structuredClone(this.schedule);
    payload.days.forEach(day => day.intervals.forEach((x, i) => { x.sequenceNumber = i + 1; x.paidMinutes = this.minutes(x); x.startTime = x.startTime.length === 5 ? x.startTime + ':00' : x.startTime; x.endTime = x.endTime.length === 5 ? x.endTime + ':00' : x.endTime; }));
    this.run(this.api.saveSchedule(payload), x => this.schedule = x);
  }
  assignDepartment(departmentId: string): void {
    if (!this.canManage || this.locked || !this.schedule.id || this.schedule.kind !== 'Optional' || this.schedule.status !== 'Active') return;
    this.run(this.api.assignDepartment(this.schedule.id, departmentId), x => this.schedule = x);
  }
  removeDepartment(departmentId: string): void {
    if (!this.canManage || this.locked || !this.schedule.id) return;
    this.run(this.api.removeDepartment(this.schedule.id, departmentId), x => this.schedule = x);
  }
  saveShift(form: NgForm): void {
    if (this.canSave(form, this.shiftError)) this.run(this.api.saveShift(structuredClone(this.shift)), x => this.shift = x);
  }
  saveCalendar(form: NgForm): void {
    if (!this.canSave(form, this.calendarError)) return;
    const payload = structuredClone(this.calendar);
    payload.days.forEach(day => day.transferredFromDate = day.transferredFromDate || null);
    this.run(this.api.saveCalendar(payload), x => this.calendar = x);
  }
  private run<T>(request: Observable<T>, accept: (x: T) => void): void {
    this.busy = true;
    this.clearFeedback();
    request.pipe(takeUntil(this.destroyed), finalize(() => this.busy = false)).subscribe({
      next: x => { accept(x); this.messageKey = 'scheduling.saved'; this.reload(); },
      // The shared error interceptor displays server validation and trace information.
      error: () => this.errorKey = 'scheduling.saveFailed'
    });
  }
}
