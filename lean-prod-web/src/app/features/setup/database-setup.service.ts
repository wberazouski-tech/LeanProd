import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DatabaseSetupStatus {
  isConfigured: boolean;
  canConfigureAnonymously: boolean;
  server: string | null;
  database: string | null;
  appLogin: string | null;
}

export interface DatabaseAdminConnectionRequest {
  server: string;
  database: string;
  adminLogin: string;
  adminPassword: string;
  encrypt: boolean;
  trustServerCertificate: boolean;
  appLogin: string;
}

export interface DatabaseSetupResult {
  status: 'Connected' | 'Created' | 'RequiresMigration' | 'NotLeanProd';
  message: string;
  server: string | null;
  database: string | null;
  appLogin: string | null;
  appliedMigrations: string[];
  pendingMigrations: string[];
}

@Injectable({ providedIn: 'root' })
export class DatabaseSetupService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}setup`;

  status(): Observable<DatabaseSetupStatus> {
    return this.http.get<DatabaseSetupStatus>(`${this.baseUrl}/status`);
  }

  testAdminConnection(connection: DatabaseAdminConnectionRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/test-admin-connection`, connection);
  }

  connectExisting(connection: DatabaseAdminConnectionRequest, applyMigrations: boolean): Observable<DatabaseSetupResult> {
    return this.http.post<DatabaseSetupResult>(`${this.baseUrl}/connect-existing`, { connection, applyMigrations });
  }

  createNew(connection: DatabaseAdminConnectionRequest): Observable<DatabaseSetupResult> {
    return this.http.post<DatabaseSetupResult>(`${this.baseUrl}/create-new`, { connection });
  }
}
