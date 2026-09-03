import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({ selector: 'app-form-field-error', standalone: true, imports: [TranslocoPipe], changeDetection: ChangeDetectionStrategy.OnPush, template: `@if (control && control.touched && control.invalid) { <small class="field-error" role="alert">{{ messageKey | transloco }}</small> }` })
export class FormFieldErrorComponent {
  @Input({ required: true }) control!: AbstractControl;
  get messageKey(): string { if (this.control.hasError('required')) return 'validation.required'; if (this.control.hasError('min') || this.control.hasError('max')) return 'validation.range'; return 'validation.invalid'; }
}
