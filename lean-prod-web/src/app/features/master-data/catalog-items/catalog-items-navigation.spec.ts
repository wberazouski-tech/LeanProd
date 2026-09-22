import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { MasterDataService } from '../master-data.service';
import { CatalogItemDetails } from '../master-data.models';
import { CatalogItemsComponent } from './catalog-items.component';
import { ShellComponent } from '../../../shell/shell.component';

describe('Items page editor', () => {
  let component: CatalogItemsComponent;
  let item: CatalogItemDetails;
  let api: { saving: () => boolean; catalogItem: jasmine.Spy; saveCatalogItem: jasmine.Spy; catalogItems: jasmine.Spy };
  beforeEach(() => {
    item = { id: 'item', type: 'Product', workingName: 'Oil', fullName: null, articleNumber: 'OIL',
      baseUnitOfMeasureId: 'unit', baseUnitName: 'Piece', baseUnitSymbol: 'pc',
      catalogItemClassId: 'class', catalogItemClassCode: 'C', catalogItemClassName: 'Class',
      cost: '9999999999999999.99', description: null, isActive: true, rowVersion: 'v1' };
    api = { saving: () => false, catalogItem: jasmine.createSpy().and.returnValue(of(item)),
      saveCatalogItem: jasmine.createSpy().and.callFake((_id: string, body: Partial<CatalogItemDetails>) => of({ ...item, ...body, rowVersion: 'v2' })),
      catalogItems: jasmine.createSpy().and.returnValue(of({ items: [item], totalCount: 1 })) };
    TestBed.configureTestingModule({ providers: [
      { provide: MasterDataService, useValue: api }, { provide: AuthService, useValue: { hasPermission: () => true } },
      { provide: TranslocoService, useValue: { translate: (key: string) => key } },
      { provide: ActivatedRoute, useValue: { data: of({ type: 'Product' }) } },
      { provide: Router, useValue: { url: '/catalog/products' } }
    ] });
    component = TestBed.runInInjectionContext(() => new CatalogItemsComponent());
    component.edit(item, new Event('click'));
  });

  it('returns to the list from the Items menu when unchanged', async () => {
    const shell = TestBed.runInInjectionContext(() => new ShellComponent());
    Object.assign(shell, { outlet: { component } });
    shell.openItems(new MouseEvent('click'));
    await Promise.resolve();
    expect(component.editorOpen).toBeFalse();
    expect(component.leaveDialogOpen).toBeFalse();
  });

  it('can cancel or discard changes without saving', async () => {
    component.form.controls.workingName.setValue('Changed');
    let closing = component.closeEditor();
    component.finishLeave(false);
    await closing;
    expect(component.editorOpen).toBeTrue();
    expect(component.form.controls.workingName.value).toBe('Changed');
    closing = component.closeEditor();
    component.finishLeave(true);
    await closing;
    expect(component.editorOpen).toBeFalse();
    expect(api.saveCatalogItem).not.toHaveBeenCalled();
  });

  it('saves exact decimal text and stays in the editor on normal Save', async () => {
    component.form.controls.workingName.setValue('Changed');
    await component.save();
    expect(api.saveCatalogItem.calls.mostRecent().args[1].cost).toBe('9999999999999999.99');
    expect(component.editorOpen).toBeTrue();
    expect(component.hasUnsavedChanges).toBeFalse();
    expect(component.selected?.rowVersion).toBe('v2');
  });

  it('saves before closing and prevents duplicate saves', async () => {
    const response = new Subject<CatalogItemDetails>();
    api.saveCatalogItem.and.returnValue(response);
    component.form.controls.workingName.setValue('Changed');
    const closing = component.closeEditor();
    const saving = component.save(true);
    await component.save(true);
    expect(component.requestLeave()).toBeFalse();
    expect(api.saveCatalogItem).toHaveBeenCalledTimes(1);
    response.next({ ...item, workingName: 'Changed' });
    await saving;
    await closing;
    expect(component.editorOpen).toBeFalse();
  });

  it('keeps invalid or failed edits open', async () => {
    component.form.controls.workingName.setValue('');
    const closing = component.closeEditor();
    await component.save(true);
    expect(api.saveCatalogItem).not.toHaveBeenCalled();
    expect(component.form.controls.workingName.touched).toBeTrue();
    component.form.controls.workingName.setValue('Changed');
    api.saveCatalogItem.and.returnValue(throwError(() => new Error('conflict')));
    await component.save(true);
    expect(component.leaveDialogOpen).toBeTrue();
    expect(component.hasUnsavedChanges).toBeTrue();
    component.finishLeave(false);
    await closing;
  });

  it('guards new and copied items and recognises reverted edits', () => {
    component.form.controls.workingName.setValue('Changed');
    component.form.controls.workingName.setValue(item.workingName);
    expect(component.hasUnsavedChanges).toBeFalse();
    component.copySelected();
    expect(component.hasUnsavedChanges).toBeTrue();
    component.create();
    expect(component.hasUnsavedChanges).toBeFalse();
    component.form.controls.workingName.setValue('New');
    expect(component.hasUnsavedChanges).toBeTrue();
  });

  it('uses the same leave decision for route and active-tab navigation', async () => {
    component.form.controls.workingName.setValue('Changed');
    const decision = component.requestLeave();
    expect(component.requestLeave()).toBe(decision);
    component.finishLeave(false);
    expect(await decision).toBeFalse();
    const anchor = document.createElement('a');
    anchor.href = '/catalog/products';
    anchor.addEventListener('click', event => component.onCatalogTab(event));
    anchor.dispatchEvent(new MouseEvent('click', { cancelable: true }));
    expect(component.leaveDialogOpen).toBeTrue();
    component.finishLeave(false);
  });
});
