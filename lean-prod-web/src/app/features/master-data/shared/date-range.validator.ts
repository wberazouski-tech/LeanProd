import { AbstractControl, ValidationErrors } from '@angular/forms';

export function equipmentDateRange(control: AbstractControl): ValidationErrors | null {
  const start = control.get('startedAtLocal')?.value as string;
  const end = control.get('endedAtLocal')?.value as string;
  if (!start) return null;
  const startTime = new Date(start).getTime();
  if (!Number.isFinite(startTime)) return { dateRange: true };
  return end && (!Number.isFinite(new Date(end).getTime()) || new Date(end).getTime() <= startTime)
    ? { dateRange: true } : null;
}
