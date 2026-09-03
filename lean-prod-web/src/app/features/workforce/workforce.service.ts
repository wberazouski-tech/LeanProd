import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { BrigadeDetails, BrigadeMembership, BrigadeSummary, EmployeeDetails, EmployeeOption, EmployeeSummary, WorkforcePage } from './workforce.models';

@Injectable({ providedIn: 'root' })
export class WorkforceService {
  private readonly http = inject(HttpClient); private readonly api = environment.apiUrl;
  employees(search = '', isActive = '') { let p = new HttpParams().set('page', 1).set('pageSize', 5000); if (search) p = p.set('search', search); if (isActive) p = p.set('isActive', isActive); return this.http.get<WorkforcePage<EmployeeSummary>>(`${this.api}employees`, { params: p }); }
  employee(id: string) { return this.http.get<EmployeeDetails>(`${this.api}employees/${id}`); }
  employeeOptions() { return this.http.get<EmployeeOption[]>(`${this.api}employees/options`); }
  employeeHistory(id: string) { return this.http.get<BrigadeMembership[]>(`${this.api}employees/${id}/brigade-history`); }
  saveEmployee(id: string | undefined, body: object) { return id ? this.http.put<EmployeeDetails>(`${this.api}employees/${id}`, body) : this.http.post<EmployeeDetails>(`${this.api}employees`, body); }
  setEmployeeActive(id: string, active: boolean) { return this.http.post<boolean>(`${this.api}employees/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  brigades(search = '', isActive = '') { let p = new HttpParams().set('page', 1).set('pageSize', 5000); if (search) p = p.set('search', search); if (isActive) p = p.set('isActive', isActive); return this.http.get<WorkforcePage<BrigadeSummary>>(`${this.api}brigades`, { params: p }); }
  brigade(id: string) { return this.http.get<BrigadeDetails>(`${this.api}brigades/${id}`); }
  saveBrigade(id: string | undefined, body: object) { return id ? this.http.put<BrigadeDetails>(`${this.api}brigades/${id}`, body) : this.http.post<BrigadeDetails>(`${this.api}brigades`, body); }
  setBrigadeActive(id: string, active: boolean) { return this.http.post<boolean>(`${this.api}brigades/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  memberships(id: string, history: boolean) { return this.http.get<BrigadeMembership[]>(`${this.api}brigades/${id}/memberships`, { params: { history } }); }
  addMembership(id: string, body: object) { return this.http.post<BrigadeMembership>(`${this.api}brigades/${id}/memberships`, body); }
  closeMembership(brigadeId: string, membershipId: string, body: object) { return this.http.post<BrigadeMembership>(`${this.api}brigades/${brigadeId}/memberships/${membershipId}/close`, body); }
  changeCoefficient(brigadeId: string, membershipId: string, body: object) { return this.http.post<BrigadeMembership>(`${this.api}brigades/${brigadeId}/memberships/${membershipId}/change-coefficient`, body); }
}
