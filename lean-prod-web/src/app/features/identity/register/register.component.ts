import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

function matchingPasswords(control: AbstractControl): ValidationErrors | null {
  return control.get('password')?.value === control.get('confirmPassword')?.value
    ? null : { passwordsDoNotMatch: true };
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly error = signal('');
  readonly submitting = signal(false);
  readonly form = new FormGroup({
    displayName: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(2), Validators.maxLength(100)] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(12)] }),
    confirmPassword: new FormControl('', { nonNullable: true, validators: [Validators.required] })
  }, { validators: matchingPasswords });

  submit(): void {
    if (this.form.invalid || this.submitting()) return;
    this.error.set('');
    this.submitting.set(true);
    const { email, displayName, password } = this.form.getRawValue();
    this.auth.register(email, displayName, password).subscribe({
      next: () => void this.router.navigateByUrl('/'),
      error: error => {
        const details = error.error?.errors;
        this.error.set(Array.isArray(details) ? details.join(' ') : (error.error?.message ?? 'Не ўдалося стварыць уліковы запіс.'));
        this.submitting.set(false);
      }
    });
  }
}
