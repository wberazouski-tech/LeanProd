import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
export type ScheduleKind='Main'|'Optional'; export type ScheduleStatus='Draft'|'Active'|'Archived'; export type CycleType='Daily'|'Weekly'|'Custom'; export type DayType='Workday'|'PreHoliday'|'Holiday'|'Saturday'|'Sunday';
export interface Interval { id?:string; workShiftId:string; workShiftCode?:string; workShiftName?:string; sequenceNumber:number; startTime:string; endTime:string; paidMinutes:number; crossesMidnight:boolean; isActive:boolean; rowVersion?:string; }
export interface ScheduleDay { id?:string; dayNumber:number; typeOfDay:DayType; name?:string; intervals:Interval[]; rowVersion?:string; }
export interface Schedule { id?:string; code:string; name:string; description?:string; kind:ScheduleKind; status:ScheduleStatus; timeZoneId:string; cycleType:CycleType; cycleLengthDays:number; cycleAnchorDate:string; usePreHolidayTemplate:boolean; useHolidayTemplate:boolean; useSaturdayTemplate:boolean; useSundayTemplate:boolean; departmentIds:string[]; days:ScheduleDay[]; rowVersion?:string; }
export interface ScheduleSummary { id:string; code:string; name:string; kind:ScheduleKind; status:ScheduleStatus; cycleType:CycleType; cycleLengthDays:number; departmentCount:number; }
export interface Shift { id?:string; code:string; name:string; description?:string; isActive:boolean; rowVersion?:string; }
export type CalendarDayType='Holiday'|'PreHoliday'|'TransferredWorkingDay'|'TransferredDayOff'|'SpecialWorkingDay'|'SpecialDayOff';
export interface CalendarDay { id?:string; date:string; dayType:CalendarDayType; name:string; transferredFromDate?:string|null; description?:string; rowVersion?:string; }
export interface Calendar { id?:string; code:string; name:string; countryCode:string; regionCode?:string; timeZoneId:string; isMain:boolean; isActive:boolean; days:CalendarDay[]; rowVersion?:string; }
@Injectable({providedIn:'root'}) export class SchedulingApi {
 private http=inject(HttpClient); private api=environment.apiUrl;
 schedules(){return this.http.get<ScheduleSummary[]>(`${this.api}work-schedules`)} schedule(id:string){return this.http.get<Schedule>(`${this.api}work-schedules/${id}`)} saveSchedule(x:Schedule){return x.id?this.http.put<Schedule>(`${this.api}work-schedules/${x.id}`,x):this.http.post<Schedule>(`${this.api}work-schedules`,x)} assignDepartment(scheduleId:string, departmentId:string){return this.http.post<Schedule>(`${this.api}work-schedules/${scheduleId}/departments/${departmentId}`,{})} removeDepartment(scheduleId:string, departmentId:string){return this.http.delete<Schedule>(`${this.api}work-schedules/${scheduleId}/departments/${departmentId}`)}
 shifts(){return this.http.get<Shift[]>(`${this.api}work-shifts`)} saveShift(x:Shift){return x.id?this.http.put<Shift>(`${this.api}work-shifts/${x.id}`,x):this.http.post<Shift>(`${this.api}work-shifts`,x)}
 calendars(){return this.http.get<Calendar[]>(`${this.api}production-calendars`)} saveCalendar(x:Calendar){return x.id?this.http.put<Calendar>(`${this.api}production-calendars/${x.id}`,x):this.http.post<Calendar>(`${this.api}production-calendars`,x)}
}
