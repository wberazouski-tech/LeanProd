import { FormControl } from '@angular/forms';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { MasterDataService } from '../master-data.service';
import { ItemPropertiesService } from './item-properties.service';
import { ItemPropertiesComponent, normalizeDecimal, decimalValidator, formatPropertyNumber } from './item-properties.component';
import { PropertyValues } from './item-properties.models';

describe('Item properties workflow', () => {
  let component: ItemPropertiesComponent;
  let api: jasmine.SpyObj<ItemPropertiesService>;
  const data: PropertyValues = { rowVersion: 'version', definitions: [
    { id: 'number', catalogItemClassId: 'class', name: 'Number', type: 'Number', decimalPlaces: 6, maxLength: null,
      minimum: null, maximum: null, isBatchProperty: true, isActive: true, options: [], rowVersion: 'v' },
    { id: 'bool', catalogItemClassId: 'class', name: 'Boolean', type: 'Boolean', decimalPlaces: null, maxLength: null,
      minimum: null, maximum: null, isBatchProperty: false, isActive: true, options: [], rowVersion: 'v' }
  ], values: [] };
  beforeEach(() => {
    api = jasmine.createSpyObj('ItemPropertiesService', ['values', 'saveValues', 'saveBatch', 'batches', 'saveDefinition']);
    api.values.and.returnValue(of(structuredClone(data)));
    api.saveValues.and.returnValue(of(structuredClone(data)));
    TestBed.configureTestingModule({ providers: [
      { provide: ItemPropertiesService, useValue: api }, { provide: MasterDataService, useValue: {} },
      { provide: ActivatedRoute, useValue: {} }, { provide: AuthService, useValue: { hasPermission: () => true } },
      { provide: TranslocoService, useValue: { translate: (key: string) => key } }
    ] });
    component = TestBed.runInInjectionContext(() => new ItemPropertiesComponent());
    component.itemId = 'item';
  });
  it('creates a definition using its name without a code', async () => {
    component.classId = 'class';
    component.editDefinition();
    component.definitionForm.controls.name.setValue('Density');
    api.saveDefinition.and.returnValue(of({ ...data.definitions[0], name: 'Density' }));
    await component.saveDefinition();
    expect(api.saveDefinition).toHaveBeenCalled();
    const body = api.saveDefinition.calls.mostRecent().args[2] as Record<string, unknown>;
    expect(body['name']).toBe('Density');
    expect('code' in body).toBeFalse();
    expect(component.definitionOpen).toBeFalse();
  });
  it('rejects invalid range settings before sending a request', async () => {
    component.editDefinition();
    component.definitionForm.patchValue({ name: 'Range', type: 'Range', decimalPlaces: 2, minimum: '0', maximum: '1,001' });
    expect(component.definitionForm.hasError('definitionRange')).toBeTrue();
    await component.saveDefinition();
    expect(api.saveDefinition).not.toHaveBeenCalled();
    component.definitionForm.patchValue({ maximum: '15000,00' });
    expect(component.definitionForm.valid).toBeTrue();
    component.definitionForm.patchValue({ minimum: '15001' });
    expect(component.definitionForm.hasError('definitionRange')).toBeTrue();
  });
  it('locks used settings but allows adding choices without replacing existing identifiers', async () => {
    const p = { ...data.definitions[0], type: 'Choice' as const, isInUse: true, decimalPlaces: null,
      options: [{ id: 'existing', label: 'A' }] };
    component.editDefinition(p);
    expect(component.definitionForm.controls.type.disabled).toBeTrue();
    expect(component.definitionForm.controls.isBatchProperty.disabled).toBeTrue();
    component.definitionForm.controls.optionsText.setValue('a');
    expect(component.definitionForm.hasError('definitionChoices')).toBeTrue();
    component.definitionForm.controls.optionsText.setValue('B');
    api.saveDefinition.and.returnValue(of(p));
    await component.saveDefinition();
    const body = api.saveDefinition.calls.mostRecent().args[2] as { options: { id: string | null; label: string }[] };
    expect(body.options).toEqual([{ id: 'existing', label: 'A' }, { id: null, label: 'B' }]);
  });
  it('makes inactive item values read-only and prevents batch creation', async () => {
    api.values.and.returnValue(of({ ...structuredClone(data), isEditable: false }));
    await component.selectBatch();
    expect(component.valuesForm.controls['number'].disabled).toBeTrue();
    await component.saveValues();
    component.editBatch();
    expect(api.saveValues).not.toHaveBeenCalled();
    expect(component.batchOpen).toBeFalse();
  });
  it('validates scale and formats exact values without rounding', () => {
    expect(decimalValidator(2)(new FormControl('12,345'))).toEqual({ decimal: true });
    expect(decimalValidator(2)(new FormControl('12,3400'))).toBeNull();
    expect(decimalValidator(6)(new FormControl('1000000000000000000'))).toEqual({ decimal: true });
    expect(formatPropertyNumber('999999999999999999.120000', 2)).toBe('999999999999999999.12');
  });
  it('rejects partial and reversed ranges in the form', async () => {
    const range = structuredClone(data);
    range.definitions = [{ ...range.definitions[0], type: 'Range', decimalPlaces: 2, minimum: '0', maximum: '15000' }];
    api.values.and.returnValue(of(range));
    await component.selectBatch();
    component.valuesForm.controls['number'].setValue('100');
    expect(component.valuesForm.hasError('range')).toBeTrue();
    component.valuesForm.controls['number_upper'].setValue('15000');
    expect(component.valuesForm.valid).toBeTrue();
    component.valuesForm.controls['number_upper'].setValue('99');
    expect(component.valuesForm.hasError('range')).toBeTrue();
  });
  it('normalizes decimal separators without floating point conversion', () => {
    expect(normalizeDecimal(' 999999999999999999,999999 ')).toBe('999999999999999999.999999');
    expect(normalizeDecimal(' ')).toBeNull();
  });
  it('sends exact numeric strings, false and owner concurrency token', async () => {
    await component.selectBatch();
    component.valuesForm.controls['number'].setValue('999999999999999999,999999');
    component.valuesForm.controls['bool'].setValue('false');
    await component.saveValues();
    const body = api.saveValues.calls.mostRecent().args[2] as { rowVersion: string; values: { number: string; boolean: boolean }[] };
    expect(body.rowVersion).toBe('version');
    expect(body.values[0].number).toBe('999999999999999999.999999');
    expect(body.values[1].boolean).toBeFalse();
  });
  it('keeps entered values after a server concurrency conflict', async () => {
    await component.selectBatch();
    component.valuesForm.controls['number'].setValue('123'); component.valuesForm.markAsDirty();
    api.saveValues.and.returnValue(throwError(() => ({ status: 409, error: { detail: 'Changed' } })));
    await component.saveValues();
    expect(component.valuesForm.controls['number'].value).toBe('123');
    expect(component.dirty).toBeTrue();
    expect(component.message).toContain('itemProperties.conflict');
    expect(component.busy).toBeFalse();
  });
  it('does not navigate away or switch batches if discard is refused', async () => {
    await component.selectBatch(); component.valuesForm.markAsDirty();
    spyOn(window, 'confirm').and.returnValue(false);
    api.values.calls.reset();
    await component.selectBatch();
    expect(api.values).not.toHaveBeenCalled();
    expect(component.requestLeave()).toBeFalse();
  });
});
