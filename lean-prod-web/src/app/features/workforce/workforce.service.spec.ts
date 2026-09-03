import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { WorkforceService } from './workforce.service';

describe('WorkforceService', () => {
  let service: WorkforceService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(WorkforceService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('sends employee filters and paging to the API', () => {
    service.employees('Smith', 'true').subscribe();
    const request = http.expectOne(req => req.url.endsWith('/employees'));
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('page')).toBe('1');
    expect(request.request.params.get('pageSize')).toBe('5000');
    expect(request.request.params.get('search')).toBe('Smith');
    expect(request.request.params.get('isActive')).toBe('true');
    request.flush({ items: [], page: 1, pageSize: 5000, totalCount: 0 });
  });

  it('uses the dedicated KTU history endpoint', () => {
    const body = { effectiveFromUtc: '2026-08-27T08:00:00.000Z', laborParticipationCoefficient: 1.25, rowVersion: 'v1' };
    service.changeCoefficient('brigade-1', 'membership-1', body).subscribe();
    const request = http.expectOne(req => req.url.endsWith('/brigades/brigade-1/memberships/membership-1/change-coefficient'));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush({});
  });
});
