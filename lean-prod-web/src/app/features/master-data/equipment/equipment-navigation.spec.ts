import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { ShellComponent } from '../../../shell/shell.component';
import { MasterDataService } from '../master-data.service';
import { EquipmentDetails } from '../master-data.models';
import { EquipmentComponent } from './equipment.component';

describe('Equipment page navigation', () => {
  let component: EquipmentComponent;
  let item: EquipmentDetails;
  let api: { saving: () => boolean; saveEquipment: jasmine.Spy };
  beforeEach(() => {
    item = { id: 'e', name: 'Tank', inventoryNumber: 'INV', equipmentTypeId: null, departmentId: 'd',
      parentEquipmentId: null, serialNumber: null, manufacturer: null, model: null, commissionedOn: null,
      description: null, currentState: 'Operational', isActive: true, rowVersion: 'v1' };
    api = { saving: () => false, saveEquipment: jasmine.createSpy().and.callFake((_id: string, body: object) => of({ ...item, ...body, rowVersion: 'v2' })) };
    TestBed.configureTestingModule({ providers: [
      { provide: MasterDataService, useValue: api }, { provide: AuthService, useValue: { hasPermission: () => true } },
      { provide: TranslocoService, useValue: { translate: (key: string) => key } },
      { provide: Router, useValue: { url: '/master-data/equipment' } }
    ] });
    component = TestBed.runInInjectionContext(() => new EquipmentComponent());
    spyOn(component, 'load'); spyOn(component, 'loadReferences'); spyOn(component, 'loadHistory');
    component.selected = item;
    component.openEdit();
  });
  it('closes an unchanged card through the Equipment menu', async () => {
    const shell = TestBed.runInInjectionContext(() => new ShellComponent());
    Object.assign(shell, { outlet: { component } });
    shell.openEquipment(new MouseEvent('click'));
    await Promise.resolve();
    expect(component.editorOpen).toBeFalse();
    expect(component.selected).toBeUndefined();
  });
  it('keeps edits on cancel and closes on discard', async () => {
    component.form.controls.name.setValue('Changed');
    let close = component.closeEditor();
    component.finishLeave(false);
    await close;
    expect(component.form.controls.name.value).toBe('Changed');
    expect(component.editorOpen).toBeTrue();
    close = component.closeEditor();
    component.finishLeave(true);
    await close;
    expect(component.editorOpen).toBeFalse();
    expect(api.saveEquipment).not.toHaveBeenCalled();
  });
  it('saves and resets the baseline while keeping the card open', async () => {
    component.form.controls.name.setValue('Changed');
    await component.save();
    expect(component.editorOpen).toBeTrue();
    expect(component.hasUnsavedChanges).toBeFalse();
    expect(component.selected?.rowVersion).toBe('v2');
  });
  it('saves before allowing navigation', async () => {
    component.form.controls.name.setValue('Changed');
    const decision = component.requestLeave();
    await component.save(true);
    expect(await decision).toBeTrue();
    expect(api.saveEquipment).toHaveBeenCalledTimes(1);
  });
  it('retains invalid and failed edits', async () => {
    component.form.controls.name.setValue('');
    const decision = component.requestLeave();
    await component.save(true);
    expect(api.saveEquipment).not.toHaveBeenCalled();
    component.form.controls.name.setValue('Changed');
    api.saveEquipment.and.returnValue(throwError(() => new Error('failure')));
    await component.save(true);
    expect(component.hasUnsavedChanges).toBeTrue();
    expect(component.leaveDialogOpen).toBeTrue();
    component.finishLeave(false);
    expect(await decision).toBeFalse();
  });
  it('tracks copies and new drafts, and blocks leaving during save', () => {
    component.copySelected();
    expect(component.hasUnsavedChanges).toBeTrue();
    component.openCreate();
    expect(component.hasUnsavedChanges).toBeFalse();
    component.form.controls.name.setValue('New');
    expect(component.hasUnsavedChanges).toBeTrue();
    component.isSaving = true;
    expect(component.requestLeave()).toBeFalse();
  });
});
