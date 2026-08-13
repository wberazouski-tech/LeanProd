import { TestBed } from '@angular/core/testing';
import { ErrorService } from './error.service';

describe('ErrorService', () => {
  it('shows and clears a global message', () => {
    const service = TestBed.inject(ErrorService);
    service.show('Problem');
    expect(service.message()).toBe('Problem');
    service.clear();
    expect(service.message()).toBeNull();
  });
});
