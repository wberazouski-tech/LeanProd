import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, of, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CurrentUser } from './current-user';
import { LanguageService, SupportedLanguage } from '../i18n/language.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly language = inject(LanguageService);
  private refreshRequest?: Observable<CurrentUser>;
  readonly currentUser = signal<CurrentUser | null>(null);

  get accessToken(): string | null { return this.currentUser()?.accessToken ?? null; }
  hasPermission(permission: string): boolean {
    return this.currentUser()?.permissions.includes(permission) ?? false;
  }
  hasRole(role: string): boolean {
    return this.currentUser()?.roles.includes(role) ?? false;
  }

  restoreSession(): Observable<CurrentUser | null> {
    return this.refresh().pipe(catchError(() => { this.currentUser.set(null); return of(null); }));
  }

  login(email: string, password: string): Observable<CurrentUser> {
    return this.http.post<CurrentUser>(`${environment.apiUrl}identity/login`, {
      email, password, rememberMe: false
    }).pipe(tap(user => this.acceptUser(user)));
  }

  register(email: string, displayName: string, password: string): Observable<CurrentUser> {
    return this.http.post<CurrentUser>(`${environment.apiUrl}identity/register`, {
      email, displayName, password, preferredLanguage: this.language.current()
    }).pipe(tap(user => this.acceptUser(user)));
  }

  refresh(): Observable<CurrentUser> {
    if (!this.refreshRequest) {
      this.refreshRequest = this.http.post<CurrentUser>(`${environment.apiUrl}identity/refresh`, {}).pipe(
        tap(user => this.acceptUser(user)),
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

  updateLanguage(language: SupportedLanguage): Observable<void> {
    this.language.set(language);
    return this.http.put<void>(`${environment.apiUrl}identity/preferences`, { preferredLanguage: language }).pipe(
      tap(() => this.currentUser.update(user => user ? { ...user, preferredLanguage: language } : user))
    );
  }

  private acceptUser(user: CurrentUser): void {
    this.currentUser.set(user);
    this.language.set(user.preferredLanguage);
  }
}
