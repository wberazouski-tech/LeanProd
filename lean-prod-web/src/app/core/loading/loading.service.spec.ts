import { fakeAsync, tick } from '@angular/core/testing';
import { LoadingService } from './loading.service';

describe('LoadingService', () => {
  it('shows the overlay only after one second', fakeAsync(() => {
    const service = new LoadingService();

    service.begin();
    tick(999);
    expect(service.visible()).toBeFalse();

    tick(1);
    expect(service.visible()).toBeTrue();

    service.end();
    expect(service.visible()).toBeFalse();
  }));

  it('stays visible until all concurrent requests finish', fakeAsync(() => {
    const service = new LoadingService();

    service.begin();
    service.begin();
    tick(1000);
    service.end();
    expect(service.visible()).toBeTrue();

    service.end();
    expect(service.visible()).toBeFalse();
  }));
});
