import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NgForm } from '@angular/forms';
import { TranslocoService, TranslocoTestingModule } from '@jsverse/transloco';
import { of, Subject, throwError } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { Permissions } from '../../core/auth/permissions';
import { MasterDataService } from '../master-data/master-data.service';
import { SchedulingComponent } from './scheduling.component';
import { Schedule, SchedulingApi } from './scheduling-api.service';

describe('SchedulingComponent', () => {
  let fixture: ComponentFixture<SchedulingComponent>;
  let component: SchedulingComponent;
  let api: jasmine.SpyObj<SchedulingApi>;
  let manage: boolean;

  async function render(): Promise<void> {
    fixture.ngZone!.run(() => {});
    await fixture.whenStable();
  }

  beforeEach(async () => {
    manage = true;
    api = jasmine.createSpyObj<SchedulingApi>('SchedulingApi', ['schedules', 'shifts', 'calendars', 'schedule', 'saveSchedule', 'saveShift', 'saveCalendar']);
    api.schedules.and.returnValue(of([]));
    api.shifts.and.returnValue(of([{ id: 'shift-1', code: 'S', name: 'Shift', isActive: true }]));
    api.calendars.and.returnValue(of([]));
    await TestBed.configureTestingModule({
      imports: [SchedulingComponent, TranslocoTestingModule.forRoot({
        langs: { en: { scheduling: { title: 'Work schedules', kinds: { Optional: 'Optional' } } },
          be: { scheduling: { title: 'Графікі працы', kinds: { Optional: 'Дадатковы' } } } },
        translocoConfig: { availableLangs: ['en', 'be'], defaultLang: 'en', reRenderOnLangChange: true, missingHandler: { logMissingKey: false, useFallbackTranslation: false, allowEmpty: false } },
        preloadLangs: true
      })],
      providers: [
        { provide: SchedulingApi, useValue: api },
        { provide: MasterDataService, useValue: { departmentOptions: () => of([]) } },
        { provide: AuthService, useValue: { hasPermission: (permission: string) => manage && permission === Permissions.workSchedulesManage } }
      ]
    }).compileComponents();
    fixture = TestBed.createComponent(SchedulingComponent);
    component = fixture.componentInstance;
    fixture.autoDetectChanges();
    await render();
  });
  afterEach(() => fixture.destroy());
  const form = () => fixture.debugElement.query(By.directive(NgForm)).injector.get(NgForm);
  async function validSchedule(): Promise<void> {
    component.schedule.code = 'TEST';
    component.schedule.name = 'Test schedule';
    await render();
  }

  it('keeps read-only users from editing or issuing save requests', async () => {
    manage = false;
    await validSchedule();
    expect(fixture.nativeElement.querySelector('fieldset.editor-fields').disabled).toBeTrue();
    expect(fixture.nativeElement.querySelector('button[type="submit"]')).toBeNull();
    component.saveSchedule(form());
    component.saveShift(form());
    component.saveCalendar(form());
    expect(api.saveSchedule).not.toHaveBeenCalled();
    expect(api.saveShift).not.toHaveBeenCalled();
    expect(api.saveCalendar).not.toHaveBeenCalled();
  });

  it('renumbers intervals after deletion and saves unique sequences with the concurrency version', async () => {
    await validSchedule();
    const day = component.schedule.days[0];
    for (let i = 0; i < 3; i++) component.addInterval(day);
    component.removeInterval(day, 1);
    component.addInterval(day);
    day.intervals.forEach((x, i) => { x.startTime = (8 + i * 2).toString().padStart(2, '0') + ':00'; x.endTime = (10 + i * 2).toString().padStart(2, '0') + ':00'; });
    component.schedule.id = 'schedule-1';
    component.schedule.rowVersion = 'version-1';
    api.saveSchedule.and.callFake(x => of(x));
    await render();
    component.saveSchedule(form());
    const payload = api.saveSchedule.calls.mostRecent().args[0];
    expect(payload.days[0].intervals.map(x => x.sequenceNumber)).toEqual([1, 2, 3]);
    expect(payload.days[0].intervals.map(x => x.paidMinutes)).toEqual([120, 120, 120]);
    expect(payload.rowVersion).toBe('version-1');
  });

  it('rejects invalid forms, missing shifts and overlapping intervals', async () => {
    component.saveSchedule(form());
    expect(api.saveSchedule).not.toHaveBeenCalled();
    await validSchedule();
    const day = component.schedule.days[0];
    component.addInterval(day);
    component.addInterval(day);
    expect(component.scheduleError).toBe('scheduling.validation.overlap');
    component.saveSchedule(form());
    day.intervals[0].workShiftId = '';
    expect(component.scheduleError).toBe('scheduling.validation.shift');
    expect(api.saveSchedule).not.toHaveBeenCalled();
  });

  it('validates overnight intervals including overlap after midnight', () => {
    const day = component.schedule.days[0];
    component.addInterval(day);
    Object.assign(day.intervals[0], { startTime: '22:00', endTime: '06:00', crossesMidnight: true });
    expect(component.minutes(day.intervals[0])).toBe(480);
    expect(component.intervalError(day)).toBe('');
    component.addInterval(day);
    Object.assign(day.intervals[1], { startTime: '05:00', endTime: '07:00' });
    expect(component.intervalError(day)).toBe('scheduling.validation.overlap');
    day.intervals[0].endTime = '';
    expect(component.intervalError(day)).toBe('scheduling.validation.duration');
  });

  it('prevents duplicate submissions and preserves edits after a server failure', async () => {
    await validSchedule();
    const pending = new Subject<Schedule>();
    api.saveSchedule.and.returnValue(pending);
    component.saveSchedule(form());
    component.saveSchedule(form());
    expect(api.saveSchedule).toHaveBeenCalledTimes(1);
    expect(component.busy).toBeTrue();
    pending.error(new Error('conflict'));
    expect(component.busy).toBeFalse();
    expect(component.errorKey).toBe('scheduling.saveFailed');
    expect(component.schedule.name).toBe('Test schedule');
  });

  it('shows loading and failure states and permits retry', () => {
    const pending = new Subject<never[]>();
    api.schedules.and.returnValue(pending);
    component.reload();
    expect(component.loading).toBeTrue();
    pending.error(new Error('offline'));
    expect(component.loading).toBeFalse();
    expect(component.loadError).toBeTrue();
    api.schedules.and.returnValue(of([]));
    component.reload();
    expect(component.loadError).toBeFalse();
    expect(component.empty).toBeTrue();
  });

  it('keeps the selected schedule if loading its replacement fails', () => {
    component.schedule.name = 'Existing';
    api.schedule.and.returnValue(throwError(() => new Error('offline')));
    component.selectSchedule('other');
    expect(component.detailLoading).toBeFalse();
    expect(component.schedule.name).toBe('Existing');
    expect(component.errorKey).toBe('scheduling.loadFailed');
  });

  it('rejects duplicate calendar dates and assignment to draft schedules', () => {
    Object.assign(component.calendar, { code: 'CAL', name: 'Calendar' });
    component.addCalendarDay();
    component.addCalendarDay();
    component.calendar.days.forEach(x => x.name = 'Holiday');
    expect(component.calendarError).toBe('scheduling.validation.duplicateDate');
    Object.assign(component.schedule, { code: 'TEST', name: 'Test', departmentIds: ['department-1'] });
    expect(component.scheduleError).toBe('scheduling.validation.assignment');
  });

  it('updates headings and enum labels when the language changes', async () => {
    const translate = TestBed.inject(TranslocoService);
    expect(fixture.nativeElement.querySelector('h1').textContent).toBe('Work schedules');
    translate.setActiveLang('be');
    await render();
    expect(fixture.nativeElement.querySelector('h1').textContent).toBe('Графікі працы');
    expect(fixture.nativeElement.querySelector('option[value="Optional"]').textContent).toBe('Дадатковы');
  });

  it('labels interval and calendar inputs and removal buttons', async () => {
    component.addInterval(component.schedule.days[0]);
    await render();
    for (const input of fixture.nativeElement.querySelectorAll('input[type="time"]')) expect(input.closest('label')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('.interval button').getAttribute('aria-label')).toBeTruthy();
    component.tab = 'calendar';
    component.addCalendarDay();
    await render();
    for (const input of fixture.nativeElement.querySelectorAll('.calendar-row input')) expect(input.closest('label')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('.calendar-row button').getAttribute('aria-label')).toBeTruthy();
  });

  it('identifies each missing schedule header field', async () => {
    expect(component.scheduleError).toBe('scheduling.validation.scheduleCode');
    component.schedule.code = 'TEST';
    expect(component.scheduleError).toBe('scheduling.validation.scheduleName');
    component.schedule.name = 'Schedule'; component.schedule.timeZoneId = '';
    expect(component.scheduleError).toBe('scheduling.validation.scheduleTimeZone');
    component.schedule.timeZoneId = 'Europe/Warsaw'; component.schedule.cycleAnchorDate = '';
    expect(component.scheduleError).toBe('scheduling.validation.scheduleAnchor');
  });

  it('saves a valid schedule regardless of the calendar country', async () => {
    component.calendar.countryCode = '';
    await validSchedule();
    api.saveSchedule.and.callFake(x => of(x));
    component.saveSchedule(form());
    expect(api.saveSchedule).toHaveBeenCalledTimes(1);
    expect(component.calendarError).toBe('scheduling.validation.country');
  });

  it('sends the pictured schedule with three non-working templates and API-compatible times', async () => {
    await validSchedule();
    component.schedule.cycleType = 'Daily'; component.cycleChanged();
    component.schedule.usePreHolidayTemplate = true;
    component.schedule.useHolidayTemplate = true;
    component.schedule.useSaturdayTemplate = true;
    component.schedule.useSundayTemplate = true;
    for (const type of ['PreHoliday', 'Holiday', 'Saturday', 'Sunday'] as const) component.syncTemplate(type, true);
    const day = component.schedule.days[0];
    component.addInterval(day); component.addInterval(day);
    Object.assign(day.intervals[0], { startTime: '08:00', endTime: '16:00' });
    Object.assign(day.intervals[1], { startTime: '16:00', endTime: '02:00', crossesMidnight: true });
    const preHoliday = component.schedule.days.find(x => x.typeOfDay === 'PreHoliday')!;
    component.addInterval(preHoliday);
    Object.assign(preHoliday.intervals[0], { startTime: '08:00', endTime: '15:00' });
    api.saveSchedule.and.callFake(x => of(x));
    await render();
    expect(component.scheduleError).toBe('');
    expect(form().valid).toBeTrue();
    component.saveSchedule(form());
    expect(api.saveSchedule).toHaveBeenCalledTimes(1);
    const payload = api.saveSchedule.calls.mostRecent().args[0];
    expect(payload.days.filter(x => ['Holiday', 'Saturday', 'Sunday'].includes(x.typeOfDay)).map(x => x.intervals)).toEqual([[], [], []]);
    expect(payload.days[0].intervals.map(x => [x.startTime, x.endTime, x.paidMinutes])).toEqual([['08:00:00', '16:00:00', 480], ['16:00:00', '02:00:00', 600]]);
    expect(payload.days.find(x => x.typeOfDay === 'PreHoliday')!.intervals[0].paidMinutes).toBe(420);
  });
});
