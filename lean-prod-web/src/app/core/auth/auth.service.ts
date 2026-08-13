import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, of, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CurrentUser } from './current-user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private refreshRequest?: Observable<CurrentUser>;
  readonly currentUser = signal<CurrentUser | null>(null);

  get accessToken(): string | null { return this.currentUser()?.accessToken ?? null; }
  hasPermission(permission: string): boolean {
    return this.currentUser()?.permissions.includes(permission) ?? false;
  }

  restoreSession(): Observable<CurrentUser | null> {
    return this.refresh().pipe(catchError(() => { this.currentUser.set(null); return of(null); }));
  }

  login(email: string, password: string): Observable<CurrentUser> {
    return this.http.post<CurrentUser>(`${environment.apiUrl}identity/login`, {
      email, password, rememberMe: false
    }).pipe(tap(user => this.currentUser.set(user)));
  }

  register(email: string, displayName: string, password: string): Observable<CurrentUser> {
    return this.http.post<CurrentUser>(`${environment.apiUrl}identity/register`, {
      email, displayName, password
    }).pipe(tap(user => this.currentUser.set(user)));
  }

  refresh(): Observable<CurrentUser> {
    if (!this.refreshRequest) {
      this.refreshRequest = this.http.post<CurrentUser>(`${environment.apiUrl}identity/refresh`, {}).pipe(
        tap(user => this.currentUser.set(user)),
        shareReplay(1),
        finalize(() => this.refreshRequest = undefined)
      );
    }
    return this.refreshRequest;
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}identity/logout`, {}).pipe(
      tap(() => this.currentUser.set(null))
    );
  }
}
