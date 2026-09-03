import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { Observable, finalize } from 'rxjs';
import {
  DatabaseAdminConnectionRequest,
  DatabaseSetupResult,
  DatabaseSetupService,
  DatabaseSetupStatus
} from './database-setup.service';

type SetupAction = 'test' | 'connect' | 'create';

@Component({
  selector: 'app-database-setup',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, TranslocoPipe],
  templateUrl: './database-setup.component.html',
  styleUrl: './database-setup.component.css'
})
export class DatabaseSetupComponent implements OnInit {
  private readonly setup = inject(DatabaseSetupService);
  private readonly translate = inject(TranslocoService);
  readonly status = signal<DatabaseSetupStatus | null>(null);
  readonly result = signal<DatabaseSetupResult | null>(null);
  readonly error = signal('');
  readonly busy = signal<SetupAction | null>(null);

  readonly form = new FormGroup({
    server: new FormControl('localhost\\OPTIMA', { nonNullable: true, validators: [Validators.required] }),
    database: new FormControl('LeanProd', { nonNullable: true, validators: [Validators.required] }),
    adminLogin: new FormControl('sa', { nonNullable: true, validators: [Validators.required] }),
    adminPassword: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    appLogin: new FormControl('leanprod_app', { nonNullable: true, validators: [Validators.required] }),
    encrypt: new FormControl(true, { nonNullable: true }),
    trustServerCertificate: new FormControl(true, { nonNullable: true }),
    applyMigrations: new FormControl(false, { nonNullable: true })
  });

  ngOnInit(): void {
    this.loadStatus();
  }

  loadStatus(): void {
    this.setup.status().subscribe({
      next: status => {
        this.status.set(status);
        this.form.patchValue({
          server: status.server ?? this.form.controls.server.value,
          database: status.database ?? this.form.controls.database.value,
          appLogin: status.appLogin && status.appLogin.toLowerCase() !== 'sa'
            ? status.appLogin
            : this.form.controls.appLogin.value
        });
      },
      error: error => this.error.set(this.errorMessage(error))
    });
  }

  test(): void {
    if (this.form.invalid || this.busy()) return;
    this.run('test', () => this.setup.testAdminConnection(this.connection()));
  }

  connectExisting(): void {
    if (this.form.invalid || this.busy()) return;
    this.run('connect', () => this.setup.connectExisting(this.connection(), this.form.controls.applyMigrations.value));
  }

  createNew(): void {
    if (this.form.invalid || this.busy()) return;
    this.run('create', () => this.setup.createNew(this.connection()));
  }

  private run(action: SetupAction, request: () => Observable<DatabaseSetupResult | void>): void {
    this.error.set('');
    this.result.set(null);
    this.busy.set(action);
    request().pipe(finalize(() => this.busy.set(null))).subscribe({
      next: result => {
        if (result) {
          this.result.set(result);
          if (result.status !== 'RequiresMigration') this.loadStatus();
          return;
        }
        this.result.set({
          status: 'Connected',
          message: this.translate.translate('setup.testOk'),
          server: this.form.controls.server.value,
          database: this.form.controls.database.value,
          appLogin: this.form.controls.appLogin.value,
          appliedMigrations: [],
          pendingMigrations: []
        });
      },
      error: error => {
        const payload = error instanceof HttpErrorResponse ? error.error as DatabaseSetupResult | undefined : undefined;
        if (payload?.status === 'RequiresMigration' || payload?.status === 'NotLeanProd') {
          this.result.set(payload);
          this.error.set(payload.message);
          return;
        }
        this.error.set(this.errorMessage(error));
      }
    });
  }

  private connection(): DatabaseAdminConnectionRequest {
    const value = this.form.getRawValue();
    return {
      server: value.server.trim(),
      database: value.database.trim(),
      adminLogin: value.adminLogin.trim(),
      adminPassword: value.adminPassword,
      appLogin: value.appLogin.trim(),
      encrypt: value.encrypt,
      trustServerCertificate: value.trustServerCertificate
    };
  }

  private errorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (typeof error.error?.message === 'string') return error.error.message;
      if (typeof error.error?.title === 'string') return error.error.title;
      if (error.status === 403) return this.translate.translate('setup.forbidden');
    }
    return this.translate.translate('common.requestFailed');
  }
}
