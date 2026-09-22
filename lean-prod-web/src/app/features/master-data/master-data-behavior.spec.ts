import { TestBed } from '@angular/core/testing';
import { BehaviorSubject, of, Subject } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../core/auth/auth.service';
import { LanguageService } from '../../core/i18n/language.service';
import { MasterDataService } from './master-data.service';
import { DepartmentsComponent } from './departments/departments.component';
import { UnitsOfMeasureComponent } from './units-of-measure/units-of-measure.component';
import { TechnologiesComponent } from './technologies/technologies.component';
import { CatalogTechnologyDetails, DepartmentSummary, Page } from './master-data.models';

describe('Master-data lists, languages and viewing', () => {
  it('loads records beyond the first 5000 without losing the first page', () => {
    const rows: DepartmentSummary[] = Array.from({ length: 5000 }, (_, i) => ({
      id: String(i), code: String(i), name: 'Department', parentDepartmentId: null, parentName: null, isActive: true
    }));
    const api = { saving: () => false, departments: jasmine.createSpy().and.callFake((page: number) =>
      of({ items: page === 1 ? rows : [{ ...rows[0], id: '5000' }], page, pageSize: 5000, totalCount: 5001 })) };
    TestBed.configureTestingModule({ providers: [
      { provide: MasterDataService, useValue: api }, { provide: AuthService, useValue: { hasPermission: () => true } },
      { provide: TranslocoService, useValue: {} }
    ] });
    const component = TestBed.runInInjectionContext(() => new DepartmentsComponent());
    component.load(); component.load(true);
    expect(api.departments.calls.mostRecent().args[0]).toBe(2);
    expect(component.items.length).toBe(5001);
    expect(component.total).toBe(5001);
  });

  it('ignores a stale list response after filters change', () => {
    const first = new Subject<Page<DepartmentSummary>>(), second = new Subject<Page<DepartmentSummary>>();
    const api = { saving: () => false, departments: jasmine.createSpy().and.returnValues(first, second) };
    TestBed.configureTestingModule({ providers: [
      { provide: MasterDataService, useValue: api }, { provide: AuthService, useValue: { hasPermission: () => true } },
      { provide: TranslocoService, useValue: {} }
    ] });
    const component = TestBed.runInInjectionContext(() => new DepartmentsComponent());
    component.load(); component.filters.controls.search.setValue('new'); component.load();
    second.next({ items: [], page: 1, pageSize: 5000, totalCount: 0 });
    first.next({ items: [], page: 1, pageSize: 5000, totalCount: 50 });
    expect(component.total).toBe(0);
  });

  it('reloads unit labels on language change and preserves the pending local name', () => {
    const languages = new BehaviorSubject('be');
    const api = {
      saving: () => false,
      units: jasmine.createSpy().and.returnValue(of({ items: [], totalCount: 0 })),
      unitOptions: jasmine.createSpy().and.returnValue(of([])),
      unitConversions: () => of([])
    };
    TestBed.configureTestingModule({ providers: [
      { provide: MasterDataService, useValue: api }, { provide: AuthService, useValue: { hasPermission: () => true } },
      { provide: TranslocoService, useValue: { langChanges$: languages } },
      { provide: LanguageService, useValue: { current: () => languages.value } }
    ] });
    const component = TestBed.runInInjectionContext(() => new UnitsOfMeasureComponent());
    component.ngOnInit();
    component.createForm.controls.localizedName.setValue('Мая адзінка');
    languages.next('en');
    expect(api.units.calls.mostRecent().args[2]).toBe('en');
    expect(api.unitOptions.calls.mostRecent().args[0]).toBe('en');
    languages.next('be');
    expect(component.createForm.controls.localizedName.value).toBe('Мая адзінка');
  });

  it('allows a viewer to open a technology but never save its sections', () => {
    const details: CatalogTechnologyDetails = {
      id: 'technology', code: 'T', name: 'Technology', catalogItemId: 'product', catalogItemName: 'Product',
      catalogItemClassId: null, catalogItemClassCode: null, catalogItemClassName: null, versionNo: 1,
      validFrom: null, validTo: null, isDefault: false, status: 'InDevelopment', description: null,
      isActive: true, stages: [], stageTransitions: [], rowVersion: 'v1'
    };
    const api = { saving: () => false, technology: () => of(details),
      saveTechnologyStages: jasmine.createSpy(), saveTechnologyMaterials: jasmine.createSpy(),
      saveTechnologyOutputs: jasmine.createSpy(), saveTechnologyOperations: jasmine.createSpy() };
    TestBed.configureTestingModule({ providers: [
      { provide: MasterDataService, useValue: api }, { provide: AuthService, useValue: { hasPermission: () => false } },
      { provide: TranslocoService, useValue: { translate: (key: string) => key } }
    ] });
    const component = TestBed.runInInjectionContext(() => new TechnologiesComponent());
    component.selectedTechnologyId = details.id;
    component.editSelected();
    expect(component.draft?.id).toBe(details.id);
    component.saveStages(); component.saveMaterials(); component.saveOutputs(); component.saveOperations();
    expect(api.saveTechnologyStages).not.toHaveBeenCalled();
    expect(api.saveTechnologyMaterials).not.toHaveBeenCalled();
    expect(api.saveTechnologyOutputs).not.toHaveBeenCalled();
    expect(api.saveTechnologyOperations).not.toHaveBeenCalled();
  });
});
