import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { PagedUsers, RoleDetails, UserDetails } from './user-administration.models';

@Injectable({ providedIn: 'root' })
export class UserAdministrationService {
  private readonly http = inject(HttpClient);
  private readonly usersUrl = `${environment.apiUrl}users`;
  list(page: number, search: string, isActive: string, role: string) {
    let params = new HttpParams().set('page', page).set('pageSize', 20);
    if (search) params = params.set('search', search);
    if (isActive) params = params.set('isActive', isActive);
    if (role) params = params.set('role', role);
    return this.http.get<PagedUsers>(this.usersUrl, { params });
  }
  get(id: string) { return this.http.get<UserDetails>(`${this.usersUrl}/${id}`); }
  roles() { return this.http.get<RoleDetails[]>(`${environment.apiUrl}roles`); }
  create(value: { email: string; displayName: string; temporaryPassword: string; preferredLanguage: string; defaultDepartmentId: string | null; defaultStorageLocationId: string | null; roles: string[] }) { return this.http.post<UserDetails>(this.usersUrl, value); }
  update(id: string, value: { email: string; displayName: string; preferredLanguage: string; defaultDepartmentId: string | null; defaultStorageLocationId: string | null; concurrencyStamp: string }) { return this.http.put<UserDetails>(`${this.usersUrl}/${id}`, value); }
  setRoles(id: string, roles: string[], concurrencyStamp: string) { return this.http.put<UserDetails>(`${this.usersUrl}/${id}/roles`, { roles, concurrencyStamp }); }
  setActive(id: string, active: boolean) { return this.http.post<boolean>(`${this.usersUrl}/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  resetPassword(id: string, temporaryPassword: string) { return this.http.post<boolean>(`${this.usersUrl}/${id}/reset-password`, { temporaryPassword }); }
}
