import { FormBuilder } from '@angular/forms';
import { MoneyFormatPipe } from '../../../core/ui/number-format.pipe';
import { equipmentDateRange } from './date-range.validator';

describe('Master-data precision and validation', () => {
  it('formats decimal(18,2) without converting it to a floating point number', () => {
    const pipe = new MoneyFormatPipe();
    expect(pipe.transform('9999999999999999.99')).toBe('9 999 999 999 999 999.99');
    expect(pipe.transform('99999999999999.99')).toBe('99 999 999 999 999.99');
    expect(pipe.transform('0.1')).toBe('0.10');
  });
  it('rejects reversed and invalid equipment state dates and permits open intervals', () => {
    const form = new FormBuilder().group({ startedAtLocal: '2026-09-15T10:00', endedAtLocal: '2026-09-15T09:00' }, { validators: equipmentDateRange });
    expect(form.hasError('dateRange')).toBeTrue();
    form.controls.endedAtLocal.setValue('2026-09-15T11:00');
    expect(form.valid).toBeTrue();
    form.controls.endedAtLocal.setValue('');
    expect(form.valid).toBeTrue();
    form.controls.startedAtLocal.setValue('invalid');
    expect(form.hasError('dateRange')).toBeTrue();
  });
});
