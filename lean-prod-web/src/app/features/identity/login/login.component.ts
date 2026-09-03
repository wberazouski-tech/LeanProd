import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { ErrorService } from '../../../core/errors/error.service';
import { DatabaseSetupService } from '../../setup/database-setup.service';

@Component({ selector: 'app-login', standalone: true, imports: [ReactiveFormsModule, RouterLink, TranslocoPipe], templateUrl: './login.component.html' })
export class LoginComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslocoService);
  private readonly globalErrors = inject(ErrorService);
  private readonly setup = inject(DatabaseSetupService);
  readonly error = signal(''); readonly submitting = signal(false); readonly passwordVisible = signal(false);
  readonly setupAvailable = signal(false);
  readonly form = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] })
  });
  ngOnInit(): void {
    this.setup.status().subscribe({
      next: status => {
        this.setupAvailable.set(status.canConfigureAnonymously);
      },
      error: () => undefined
    });
  }
  submit(): void {
    if (this.form.invalid || this.submitting()) return;
    this.error.set(''); this.globalErrors.clear(); this.submitting.set(true);
    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password).subscribe({
      next: () => void this.router.navigateByUrl('/'),
      error: () => {
        this.setup.status().subscribe({
          next: status => {
            if (status.canConfigureAnonymously) {
              this.setupAvailable.set(true);
              this.error.set(this.translate.translate('setup.databaseUnavailable'));
              this.submitting.set(false);
              return;
            }
            this.error.set(this.translate.translate('auth.invalidCredentials'));
            this.submitting.set(false);
          },
          error: () => {
            this.error.set(this.translate.translate('auth.invalidCredentials'));
            this.submitting.set(false);
          }
        });
      }
    });
  }
}
