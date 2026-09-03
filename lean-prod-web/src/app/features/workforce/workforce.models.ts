import { Page } from '../master-data/master-data.models';

export type WorkforcePage<T> = Page<T>;
export interface EmployeeSummary { id: string; personnelNumber: string; fullName: string; position: string | null; departmentId: string | null; departmentName: string | null; activeBrigadeCount: number; isActive: boolean; }
export interface EmployeeDetails { id: string; personnelNumber: string; lastName: string; firstName: string; middleName: string | null; position: string | null; departmentId: string | null; isActive: boolean; rowVersion: string; }
export interface EmployeeOption { id: string; personnelNumber: string; fullName: string; }
export interface BrigadeSummary { id: string; code: string; name: string; departmentId: string | null; departmentName: string | null; activeMemberCount: number; isActive: boolean; }
export interface BrigadeDetails { id: string; code: string; name: string; description: string | null; departmentId: string | null; isActive: boolean; rowVersion: string; }
export interface BrigadeMembership { id: string; brigadeId: string; brigadeCode: string; brigadeName: string; employeeId: string; personnelNumber: string; employeeName: string; startedAtUtc: string; endedAtUtc: string | null; laborParticipationCoefficient: number; rowVersion: string; }
