import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { ShellComponent } from '../../../shell/shell.component';
import { MasterDataService } from '../master-data.service';
import { CatalogTechnologyDetails, CatalogTechnologyStage } from '../master-data.models';
import { TechnologiesComponent } from './technologies.component';

describe('Technology editor navigation', () => {
  const clone = <T>(value: T): T => JSON.parse(JSON.stringify(value)) as T;
  let component: TechnologiesComponent;
  let server: CatalogTechnologyDetails;
  let api: { saving: () => boolean; technologies: jasmine.Spy; saveTechnologyHeader: jasmine.Spy; saveTechnologyMaterials: jasmine.Spy };

  beforeEach(() => {
    server = {
      id: 'tech', code: 'T', name: 'Technology', catalogItemId: 'product', catalogItemName: 'Product',
      catalogItemClassId: null, catalogItemClassCode: null, catalogItemClassName: null, versionNo: 1,
      validFrom: null, validTo: null, isDefault: false, status: 'InDevelopment', description: null,
      isActive: true, stages: [], stageTransitions: [], rowVersion: 'v1'
    };
    api = {
      saving: () => false,
      technologies: jasmine.createSpy().and.returnValue(of({ items: [], totalCount: 0 })),
      saveTechnologyHeader: jasmine.createSpy().and.callFake((_id: string, payload: Partial<CatalogTechnologyDetails>) => {
        server = { ...server, ...payload, rowVersion: 'v2' };
        return of(clone(server));
      }),
      saveTechnologyMaterials: jasmine.createSpy().and.callFake((_id: string, id: string, payload: Pick<CatalogTechnologyStage, 'materials'>) => {
        server.stages.find(stage => stage.id === id)!.materials = clone(payload.materials);
        return of(clone(server));
      })
    };
    TestBed.configureTestingModule({ providers: [
      { provide: MasterDataService, useValue: api },
      { provide: AuthService, useValue: { hasPermission: () => true } },
      { provide: TranslocoService, useValue: { translate: (key: string) => key } },
      { provide: Router, useValue: { url: '/master-data/technologies' } }
    ] });
    component = TestBed.runInInjectionContext(() => new TechnologiesComponent());
    component.selected = clone(server);
    component.draft = clone(server);
  });

  it('returns to the list when the same menu item is clicked', async () => {
    const shell = TestBed.runInInjectionContext(() => new ShellComponent());
    Object.assign(shell, { outlet: { component } });
    const event = new MouseEvent('click', { cancelable: true });
    shell.openTechnologies(event);
    await Promise.resolve();
    expect(event.defaultPrevented).toBeTrue();
    expect(component.draft).toBeUndefined();
    expect(component.leaveDialogOpen).toBeFalse();
  });

  it('preserves modified drafts on cancel and closes only after discard', async () => {
    component.draft!.name = 'Changed';
    const closing = component.closeEditor();
    expect(component.leaveDialogOpen).toBeTrue();
    component.finishLeave(false);
    await closing;
    expect(component.draft!.name).toBe('Changed');
    const discarding = component.closeEditor();
    component.finishLeave(true);
    await discarding;
    expect(component.draft).toBeUndefined();
    expect(api.saveTechnologyHeader).not.toHaveBeenCalled();
  });

  it('guards route changes with the same pending decision', async () => {
    component.draft!.name = 'Changed';
    const decision = component.requestLeave();
    expect(component.requestLeave()).toBe(decision);
    component.finishLeave(false);
    expect(await decision).toBeFalse();
  });

  it('tracks changes to new records and copied records', () => {
    component.create();
    expect(component.hasUnsavedChanges).toBeFalse();
    component.draft!.name = 'New technology';
    expect(component.hasUnsavedChanges).toBeTrue();
    component.selected = clone(server);
    component.copy();
    expect(component.hasUnsavedChanges).toBeTrue();
  });

  it('saves header and material changes from every stage before closing', async () => {
    server.stages = ['one', 'two'].map((id, index) => ({
      id, technologyStageId: id, technologyStageCode: id, technologyStageName: id, stageNumber: (index + 1) * 10,
      plannedDurationMinutes: null, technologyStageDepartmentId: 'department', technologyStageDepartmentName: 'Department',
      equipmentId: null, equipmentName: null, description: null, rowVersion: 's1', outputs: [], operations: [],
      materials: [{
        id: 'material-' + id, catalogItemId: 'item', catalogItemName: 'Item', unitOfMeasureId: 'unit', unitOfMeasureName: 'Unit',
        quantity: 1, consumptionTrackingMode: 'NormOnly', defaultSourceStorageLocationId: null,
        defaultSourceStorageLocationName: null, scrapPercent: 0, isOptional: false, note: null, routeSteps: [], rowVersion: 'm1'
      }]
    }));
    component.selected = clone(server);
    component.draft = clone(server);
    component.draft.name = 'Changed';
    component.draft.stages.forEach(stage => stage.materials[0].quantity = 2);
    const closing = component.closeEditor();
    await component.saveAndLeave();
    await closing;
    expect(server.name).toBe('Changed');
    expect(api.saveTechnologyMaterials).toHaveBeenCalledTimes(2);
    expect(server.stages.every(stage => stage.materials[0].quantity === 2)).toBeTrue();
    expect(component.draft).toBeUndefined();
  });

  it('keeps the draft and dialog open when saving fails', async () => {
    component.draft!.name = 'Changed';
    api.saveTechnologyHeader.and.returnValue(throwError(() => new Error('failure')));
    const closing = component.closeEditor();
    await component.saveAndLeave();
    expect(component.draft!.name).toBe('Changed');
    expect(component.leaveDialogOpen).toBeTrue();
    expect(component.message).toBe('technologies.saveError');
    component.finishLeave(false);
    await closing;
  });

  it('blocks navigation during a save and leaves invalid drafts open', async () => {
    component.isSaving = true;
    expect(component.requestLeave()).toBeFalse();
    component.isSaving = false;
    component.draft!.name = '';
    const closing = component.closeEditor();
    await component.saveAndLeave();
    expect(api.saveTechnologyHeader).not.toHaveBeenCalled();
    expect(component.leaveDialogOpen).toBeTrue();
    component.finishLeave(false);
    await closing;
  });
});
