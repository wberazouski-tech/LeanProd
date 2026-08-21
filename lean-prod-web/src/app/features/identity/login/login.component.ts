import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { ErrorService } from '../../../core/errors/error.service';

@Component({ selector: 'app-login', standalone: true, imports: [ReactiveFormsModule, RouterLink, TranslocoPipe], templateUrl: './login.component.html' })
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslocoService);
  private readonly globalErrors = inject(ErrorService);
  readonly error = signal(''); readonly submitting = signal(false); readonly passwordVisible = signal(false);
  readonly form = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] })
  });
  submit(): void {
    if (this.form.invalid || this.submitting()) return;
    this.error.set(''); this.globalErrors.clear(); this.submitting.set(true);
    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password).subscribe({
      next: () => void this.router.navigateByUrl('/'),
      error: () => { this.error.set(this.translate.translate('auth.invalidCredentials')); this.submitting.set(false); }
    });
  }
}
