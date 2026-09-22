import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MasterDataService } from './master-data.service';

describe('MasterDataService request safety', () => {
  let service: MasterDataService;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(MasterDataService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());
  it('prevents duplicate creation and permits retry after failure', () => {
    const body = { name: 'Department' };
    service.saveDepartment(undefined, body).subscribe({ error: () => {} });
    service.saveDepartment(undefined, body).subscribe();
    expect(service.saving()).toBeTrue();
    http.expectOne(req => req.url.endsWith('/departments')).flush({}, { status: 409, statusText: 'Conflict' });
    expect(service.saving()).toBeFalse();
    service.saveDepartment(undefined, body).subscribe();
    http.expectOne(req => req.url.endsWith('/departments')).flush({});
    expect(service.saving()).toBeFalse();
  });
  it('preserves every cost digit and row version in update requests', () => {
    const body = { cost: '9999999999999999.99', rowVersion: 'original-version' };
    service.saveCatalogItem('item-1', body).subscribe();
    const request = http.expectOne(req => req.url.endsWith('/catalog-items/item-1'));
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(body);
    request.flush({});
  });
  it('requests additional pages without dropping filters or language', () => {
    service.units('metre', 'true', 'be', 2).subscribe();
    const request = http.expectOne(req => req.url.endsWith('/unit-of-measures'));
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('search')).toBe('metre');
    expect(request.request.params.get('language')).toBe('be');
    expect(request.request.params.get('isActive')).toBe('true');
    request.flush({ items: [], totalCount: 0 });
  });
});
