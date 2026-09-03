import { FormControl, Validators } from '@angular/forms';
import { FormFieldErrorComponent } from './form-field-error.component';

describe('FormFieldErrorComponent', () => {
  it('maps required and range validation errors to translation keys', () => {
    const component = new FormFieldErrorComponent();
    component.control = new FormControl('', Validators.required);
    expect(component.messageKey).toBe('validation.required');

    component.control = new FormControl(11, Validators.max(10));
    expect(component.messageKey).toBe('validation.range');
  });
});
