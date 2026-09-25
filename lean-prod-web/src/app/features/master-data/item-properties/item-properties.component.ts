import { Component, HostListener, OnDestroy, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormRecord, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { Subject, firstValueFrom, takeUntil } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { MasterDataService } from '../master-data.service';
import { ItemPropertiesService } from './item-properties.service';
import { ItemBatch, ItemPropertyType, PropertyDefinition, PropertyValue, PropertyValues } from './item-properties.models';

@Component({
  selector: 'app-item-properties', standalone: true,
  imports: [ReactiveFormsModule, RouterLink, TranslocoPipe],
  templateUrl: './item-properties.component.html',
  styleUrls: ['../master-data.css', './item-properties.component.css']
})
export class ItemPropertiesComponent implements OnInit, OnDestroy {
  private readonly api = inject(ItemPropertiesService);
  private readonly master = inject(MasterDataService);
  private readonly route = inject(ActivatedRoute);
  private readonly t = inject(TranslocoService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyed = new Subject<void>();
  readonly canManage = inject(AuthService).hasPermission(Permissions.masterDataManage);
  readonly types: ItemPropertyType[] = ['Number', 'Text', 'Choice', 'Boolean', 'Range'];
  classId = ''; itemId = ''; title = ''; backUrl = '/catalog/products';
  administrationType = 'Product';
  classActive = true;
  itemActive = true;
  get canEditDefinitions(): boolean { return this.canManage && this.classActive; }
  get canEditValues(): boolean { return this.canManage && this.itemActive; }
  definitions: PropertyDefinition[] = [];
  editing?: PropertyDefinition;
  definitionOpen = false;
  batches: ItemBatch[] = [];
  batch?: ItemBatch;
  batchOpen = false;
  batchPage = 1; batchTotal = 0;
  busy = false; loading = false; message = ''; failed = false;
  private requestId = 0;
  values?: PropertyValues;
  valuesForm = new FormRecord<FormControl<string>>({});
  readonly definitionForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)]],
    type: this.fb.nonNullable.control<ItemPropertyType>('Number'),
    decimalPlaces: [2, [Validators.required, Validators.min(0), Validators.max(6), Validators.pattern(/^\d+$/)]],
    maxLength: [100, [Validators.required, Validators.min(1), Validators.max(4000), Validators.pattern(/^\d+$/)]],
    minimum: '0', maximum: '15000', isBatchProperty: false, isActive: true, optionsText: ''
  }, { validators: control => validateDefinitionForm(control, this.editing) });
  readonly batchForm = this.fb.nonNullable.group({
    number: ['', [Validators.required, Validators.maxLength(100)]],
    receiptDate: ['', Validators.required], receiptReference: ['', [Validators.required, Validators.maxLength(300)]]
  });

  get dirty(): boolean { return this.definitionOpen && this.definitionForm.dirty || this.batchOpen && this.batchForm.dirty || this.valuesForm.dirty; }
  requestLeave(): boolean { return !this.busy && (!this.dirty || window.confirm(this.t.translate('itemProperties.discard'))); }
  @HostListener('window:beforeunload', ['$event']) beforeUnload(event: BeforeUnloadEvent): void {
    if (this.dirty || this.busy) { event.preventDefault(); event.returnValue = ''; }
  }
  ngOnInit(): void {
    this.definitionForm.controls.type.valueChanges.pipe(takeUntil(this.destroyed)).subscribe(() => this.configureDefinitionFields());
    this.configureDefinitionFields();
    this.route.paramMap.pipe(takeUntil(this.destroyed)).subscribe(params => {
      this.classId = params.get('classId') ?? ''; this.itemId = params.get('itemId') ?? '';
      this.backUrl = this.itemId ? '/catalog/products' : '/administration/item-properties';
      const back = this.route.snapshot.queryParamMap.get('back');
      if (this.itemId && back && /^\/catalog\/[a-z-]+$/.test(back)) this.backUrl = back;
      this.definitionOpen = false; this.batchOpen = false; this.batch = undefined;
      this.valuesForm = new FormRecord<FormControl<string>>({});
      void this.load();
    });
  }
  ngOnDestroy(): void { ++this.requestId; this.destroyed.next(); this.destroyed.complete(); }
  reload(): void {
    if (!this.requestLeave()) return;
    this.definitionOpen = false; this.batchOpen = false;
    this.definitionForm.markAsPristine(); this.batchForm.markAsPristine();
    this.valuesForm = new FormRecord<FormControl<string>>({});
    void this.load();
  }
  async load(): Promise<void> {
    const request = ++this.requestId;
    this.loading = true; this.message = ''; this.failed = false;
    try {
      if (this.itemId) {
        const [item, values, batches] = await Promise.all([
          firstValueFrom(this.master.catalogItem(this.itemId)), firstValueFrom(this.api.values(this.itemId)), firstValueFrom(this.api.batches(this.itemId))
        ]);
        if (request !== this.requestId) return;
        this.title = item.workingName; this.classId = item.catalogItemClassId; this.itemActive = item.isActive;
        this.batch = undefined; this.batchOpen = false;
        this.setValues(values); this.batches = batches.items; this.batchPage = 1; this.batchTotal = batches.totalCount;
      } else {
        const [itemClass, definitions] = await Promise.all([
          firstValueFrom(this.master.catalogItemClass(this.classId)), firstValueFrom(this.api.definitions(this.classId))
        ]);
        if (request !== this.requestId) return;
        this.title = itemClass.name; this.classActive = itemClass.isActive && !itemClass.isGroup; this.administrationType = itemClass.type; this.definitions = definitions;
      }
    } catch (error) { if (request === this.requestId) { this.fail(error); this.failed = true; } }
    finally { if (request === this.requestId) this.loading = false; }
  }
  editDefinition(p?: PropertyDefinition): void {
    if (!this.canEditDefinitions || !this.requestLeave()) return;
    this.editing = p; this.definitionOpen = true; this.message = '';
    this.definitionForm.reset({ name: p?.name ?? '', type: p?.type ?? 'Number',
      decimalPlaces: p?.decimalPlaces ?? 2, maxLength: p?.maxLength ?? 100,
      minimum: p?.minimum ?? '0', maximum: p?.maximum ?? '15000',
      isBatchProperty: p?.isBatchProperty ?? false, isActive: p?.isActive ?? true,
      optionsText: p?.isInUse ? '' : p?.options.map(x => x.label).join('\n') ?? '' });
    this.configureDefinitionFields();
  }
  private configureDefinitionFields(): void {
    const controls = this.definitionForm.controls;
    const type = controls.type.value;
    const locked = !!this.editing?.isInUse;
    const setEnabled = (control: AbstractControl, enabled: boolean): void => {
      if (enabled) control.enable({ emitEvent: false }); else control.disable({ emitEvent: false });
    };
    setEnabled(controls.type, !locked);
    setEnabled(controls.isBatchProperty, !locked);
    setEnabled(controls.decimalPlaces, !locked && (type === 'Number' || type === 'Range'));
    setEnabled(controls.maxLength, !locked && type === 'Text');
    setEnabled(controls.minimum, !locked && type === 'Range');
    setEnabled(controls.maximum, !locked && type === 'Range');
    setEnabled(controls.optionsText, type === 'Choice');
    this.definitionForm.updateValueAndValidity({ emitEvent: false });
  }
  async saveDefinition(): Promise<void> {
    if (!this.canEditDefinitions || this.busy || this.definitionForm.invalid) { this.definitionForm.markAllAsTouched(); return; }
    const f = this.definitionForm.getRawValue();
    const numeric = f.type === 'Number' || f.type === 'Range';
    const additions = f.type === 'Choice' ? f.optionsText.split(/\r?\n/).map(x => x.trim()).filter(Boolean)
      .map(label => ({ id: this.editing?.options.find(x => x.label === label)?.id ?? null, label })) : [];
    const options = this.editing?.isInUse ? [...this.editing.options, ...additions] : additions;
    this.busy = true; this.message = '';
    try {
      const saved = await firstValueFrom(this.api.saveDefinition(this.classId, this.editing?.id, {
        name: f.name, type: f.type, decimalPlaces: numeric ? f.decimalPlaces : null,
        maxLength: f.type === 'Text' ? f.maxLength : null,
        minimum: f.type === 'Range' ? normalizeDecimal(f.minimum) : null,
        maximum: f.type === 'Range' ? normalizeDecimal(f.maximum) : null,
        isBatchProperty: f.isBatchProperty, isActive: f.isActive, options, rowVersion: this.editing?.rowVersion
      }));
      this.definitions = [...this.definitions.filter(x => x.id !== saved.id), saved].sort((a, b) => a.name.localeCompare(b.name) || a.id.localeCompare(b.id));
      this.definitionForm.markAsPristine(); this.definitionOpen = false; this.message = this.t.translate('itemProperties.saved');
    } catch (error) { this.fail(error); } finally { this.busy = false; }
  }
  closeDefinition(): void { if (this.requestLeave()) { this.definitionOpen = false; this.definitionForm.markAsPristine(); } }
  private setValues(values: PropertyValues): void {
    this.values = values;
    this.itemActive = values.isEditable !== false;
    const controls: Record<string, FormControl<string>> = {};
    for (const p of values.definitions) {
      const v = values.values.find(x => x.propertyId === p.id);
      const value = p.type === 'Boolean' ? (v?.boolean == null ? '' : String(v.boolean)) :
        p.type === 'Choice' ? v?.optionId : p.type === 'Text' ? v?.text : formatPropertyNumber(v?.number, p.decimalPlaces ?? 0);
      controls[p.id] = new FormControl(value ?? '', { nonNullable: true,
        validators: p.type === 'Text' ? [Validators.maxLength(p.maxLength!)] : p.type === 'Number' || p.type === 'Range' ? [decimalValidator(p.decimalPlaces!)] : [] });
      if (!p.isActive || !this.canEditValues) controls[p.id].disable();
      if (p.type === 'Range') {
        controls[p.id + '_upper'] = new FormControl(formatPropertyNumber(v?.upper, p.decimalPlaces!), { nonNullable: true, validators: [decimalValidator(p.decimalPlaces!)] });
        if (!p.isActive || !this.canEditValues) controls[p.id + '_upper'].disable();
      }
    }
    this.valuesForm = new FormRecord(controls, { validators: control => {
      for (const p of values.definitions.filter(x => x.type === 'Range' && x.isActive)) {
        const lower = control.get(p.id); const upper = control.get(p.id + '_upper');
        if (!lower || !upper || lower.invalid || upper.invalid) continue;
        const a = normalizeDecimal(lower.value); const b = normalizeDecimal(upper.value);
        if (a === null && b === null) continue;
        if (a === null || b === null || scaledDecimal(a) > scaledDecimal(b) ||
            scaledDecimal(a) < scaledDecimal(p.minimum!) || scaledDecimal(b) > scaledDecimal(p.maximum!)) return { range: true };
      }
      return null;
    } });
  }
  async selectBatch(batch?: ItemBatch): Promise<void> {
    if (!this.requestLeave()) return;
    this.busy = true; this.message = '';
    try {
      const values = await firstValueFrom(this.api.values(this.itemId, batch?.id));
      this.batch = values.batch ?? batch; this.batchOpen = false; this.batchForm.markAsPristine(); this.setValues(values);
    } catch (error) { this.fail(error); } finally { this.busy = false; }
  }
  async saveValues(): Promise<void> {
    if (!this.canEditValues || this.busy || !this.values || this.valuesForm.invalid) return;
    const raw = this.valuesForm.getRawValue();
    const values: PropertyValue[] = this.values.definitions.filter(x => x.isActive).map(p => {
      const s = raw[p.id];
      return { propertyId: p.id,
        number: p.type === 'Number' || p.type === 'Range' ? normalizeDecimal(s) : null,
        upper: p.type === 'Range' ? normalizeDecimal(raw[p.id + '_upper']) : null,
        text: p.type === 'Text' && s !== '' ? s : null,
        boolean: p.type === 'Boolean' && s !== '' ? s === 'true' : null,
        optionId: p.type === 'Choice' && s !== '' ? s : null };
    });
    this.busy = true; this.message = '';
    try {
      this.setValues(await firstValueFrom(this.api.saveValues(this.itemId, this.batch?.id, { rowVersion: this.values.rowVersion, values })));
      if (this.batch) this.batch.rowVersion = this.values!.rowVersion;
      this.message = this.t.translate('itemProperties.saved');
    } catch (error) { this.fail(error); } finally { this.busy = false; }
  }
  editBatch(existing = false): void {
    if (!this.canEditValues || !this.requestLeave()) return;
    this.valuesForm.markAsPristine();
    if (!existing) { this.batch = undefined; this.values = undefined; }
    this.batchOpen = true; this.message = '';
    this.batchForm.reset({ number: this.batch?.number ?? '', receiptDate: this.batch?.receiptDate ?? localToday(),
      receiptReference: this.batch?.receiptReference ?? '' });
  }
  async saveBatch(): Promise<void> {
    if (!this.canEditValues || this.busy || this.batchForm.invalid) return;
    this.busy = true; this.message = '';
    try {
      const saved = await firstValueFrom(this.api.saveBatch(this.itemId, this.batch?.id,
        { ...this.batchForm.getRawValue(), rowVersion: this.batch?.rowVersion }));
      const isNew = !this.batch;
      this.batch = saved; this.batchOpen = false; this.batchForm.markAsPristine();
      this.batches = [saved, ...this.batches.filter(x => x.id !== saved.id)];
      if (isNew) ++this.batchTotal;
      this.values = undefined;
      this.setValues(await firstValueFrom(this.api.values(this.itemId, saved.id)));
      this.message = this.t.translate('itemProperties.saved');
    } catch (error) { this.fail(error); } finally { this.busy = false; }
  }
  async moreBatches(): Promise<void> {
    if (this.busy) return;
    this.busy = true;
    try {
      const page = await firstValueFrom(this.api.batches(this.itemId, this.batchPage + 1));
      this.batchPage = page.page; this.batchTotal = page.totalCount;
      this.batches = [...this.batches, ...page.items.filter(x => !this.batches.some(b => b.id === x.id))];
    } catch (error) { this.fail(error); } finally { this.busy = false; }
  }
  private fail(error: unknown): void {
    const http = error as HttpErrorResponse;
    const errors = http.error?.errors as Record<string, string[]> | undefined;
    if (http.status === 400 && errors) {
      this.message = this.t.translate('itemProperties.invalidValue') + ' ' + Object.values(errors).flat().join(' ');
      return;
    }
    this.message = http.status === 409 ? this.t.translate('itemProperties.conflict') + ' ' + (http.error?.detail ?? '') :
      http.error?.detail ?? this.t.translate('itemProperties.error');
  }
}

export function normalizeDecimal(value: string): string | null {
  const trimmed = value.trim().replace(',', '.');
  return trimmed === '' ? null : trimmed;
}

function localToday(): string {
  const now = new Date();
  return [now.getFullYear(), String(now.getMonth() + 1).padStart(2, '0'), String(now.getDate()).padStart(2, '0')].join('-');
}
export function formatPropertyNumber(value: string | null | undefined, scale: number): string {
  if (value == null) return '';
  const [whole, fraction = ''] = value.split('.');
  return scale ? whole + '.' + fraction.padEnd(scale, '0').slice(0, scale) : whole;
}

export function decimalValidator(scale: number) {
  return (control: AbstractControl): { decimal: true } | null => {
    const value = normalizeDecimal(String(control.value ?? ''));
    if (value === null) return null;
    if (!/^[+-]?\d+(\.\d+)?$/.test(value)) return { decimal: true };
    const [whole, fraction = ''] = value.replace(/^[+-]/, '').split('.');
    return whole.replace(/^0+/, '').length > 18 || fraction.replace(/0+$/, '').length > scale ? { decimal: true } : null;
  };
}

function scaledDecimal(value: string): bigint {
  const negative = value.startsWith('-');
  const [whole, fraction = ''] = value.replace(/^[+-]/, '').split('.');
  const magnitude = BigInt(whole + fraction.padEnd(6, '0').slice(0, 6));
  return negative ? -magnitude : magnitude;
}
export function validateDefinitionForm(control: AbstractControl, existing?: PropertyDefinition): Record<string, boolean> | null {
  const f = control.getRawValue();
  if (f.type === 'Range') {
    const scale = Number(f.decimalPlaces);
    if (!Number.isInteger(scale) || scale < 0 || scale > 6) return { definitionRange: true };
    const minimum = normalizeDecimal(f.minimum ?? '');
    const maximum = normalizeDecimal(f.maximum ?? '');
    if (minimum === null || maximum === null ||
        decimalValidator(scale)(control.get('minimum')!) || decimalValidator(scale)(control.get('maximum')!) ||
        scaledDecimal(minimum) > scaledDecimal(maximum)) return { definitionRange: true };
  }
  if (f.type === 'Choice') {
    const added = (f.optionsText as string).split(/\r?\n/).map(x => x.trim()).filter(Boolean);
    const labels = existing?.isInUse ? [...existing.options.map(x => x.label), ...added] : added;
    if (!labels.length || labels.length > 500 || labels.some(x => x.length > 200) ||
        new Set(labels.map(x => x.toUpperCase())).size !== labels.length) return { definitionChoices: true };
  }
  return null;
}