import { CommonModule } from '@angular/common';
import { Component, HostListener, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { Subject, firstValueFrom, takeUntil } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { ItemPropertiesComponent } from '../item-properties/item-properties.component';
import { CatalogItemType, CatalogItemDetails, CatalogItemClassOption, UnitSummary } from '../master-data.models';
import { MasterDataService } from '../master-data.service';

@Component({
  selector: 'app-catalog-item-editor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, TranslocoPipe, ItemPropertiesComponent],
  templateUrl: './catalog-item-editor.component.html',
  styleUrls: ['../master-data.css', './catalog-item-editor.component.css']
})
export class CatalogItemEditorComponent implements OnInit, OnDestroy {
  private readonly api = inject(MasterDataService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly t = inject(TranslocoService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyed = new Subject<void>();
  readonly canManage = inject(AuthService).hasPermission(Permissions.masterDataManage);
  readonly form = this.fb.nonNullable.group({
    workingName: ['', [Validators.required, Validators.maxLength(200)]],
    fullName: ['', Validators.maxLength(500)],
    articleNumber: ['', Validators.maxLength(100)],
    baseUnitOfMeasureId: ['', Validators.required],
    catalogItemClassId: ['', Validators.required],
    cost: ['0.00', [Validators.required, Validators.pattern(/^(0|[1-9]\d{0,15})(\.\d{1,2})?$/)]],
    description: ['', Validators.maxLength(1000)]
  });
  readonly catalogTypes: { type: CatalogItemType; path: string }[] = [
    { type: 'Product', path: '/catalog/products' }, { type: 'Work', path: '/catalog/works' },
    { type: 'PrimaryMaterial', path: '/catalog/primary-materials' }, { type: 'AuxiliaryMaterial', path: '/catalog/auxiliary-materials' },
    { type: 'SemiFinishedProduct', path: '/catalog/semi-finished-products' }, { type: 'Packaging', path: '/catalog/packaging' },
    { type: 'ToolingAndTools', path: '/catalog/tooling-and-tools' }, { type: 'SparePart', path: '/catalog/spare-parts' },
    { type: 'PurchasedService', path: '/catalog/purchased-services' }, { type: 'Waste', path: '/catalog/waste' }
  ];
  itemId = '';
  item?: CatalogItemDetails;
  type: CatalogItemType = 'Product';
  tab: 'main' | 'properties' | 'batches' = 'main';
  backUrl = '/catalog/products';
  units: UnitSummary[] = [];
  classOptions: CatalogItemClassOption[] = [];
  @ViewChild(ItemPropertiesComponent) propertiesComponent?: ItemPropertiesComponent;
  loading = true;
  saving = false;
  message = '';
  leaveDialogOpen = false;
  private leavePromise?: Promise<boolean>;
  private resolveLeave?: (leave: boolean) => void;
  private baseline = '';

  get currentCatalogPath(): string { return this.catalogTypes.find(x => x.type === this.type)?.path ?? '/catalog/products'; }
  get hasUnsavedChanges(): boolean { return JSON.stringify(this.form.getRawValue()) !== this.baseline; }
  get dirty(): boolean { return this.hasUnsavedChanges || !!this.propertiesComponent?.dirty; }
  get propertiesBusy(): boolean { return !!this.propertiesComponent?.busy; }
  get propertiesInvalid(): boolean { return !!this.propertiesComponent?.valuesForm.invalid; }
  requestLeave(): boolean | Promise<boolean> {
    if (this.saving || this.propertiesBusy) return false;
    if (!this.dirty) return true;
    if (this.leavePromise) return this.leavePromise;
    this.leaveDialogOpen = true;
    this.leavePromise = new Promise<boolean>(resolve => this.resolveLeave = resolve);
    return this.leavePromise;
  }
  @HostListener('window:beforeunload', ['$event']) beforeUnload(event: BeforeUnloadEvent): void {
    if (this.dirty || this.saving || this.propertiesBusy) { event.preventDefault(); event.returnValue = ''; }
  }

  ngOnInit(): void {
    this.itemId = this.route.snapshot.paramMap.get('itemId') ?? '';
    const back = this.route.snapshot.queryParamMap.get('back');
    if (back && /^\/catalog\/[a-z-]+$/.test(back)) this.backUrl = back;
    const requestedTab = this.route.snapshot.queryParamMap.get('tab');
    this.tab = requestedTab === 'batches' ? 'batches' : requestedTab === 'properties' || this.route.snapshot.url.some(segment => segment.path === 'properties') ? 'properties' : 'main';
    this.api.unitOptions(this.t.getActiveLang()).pipe(takeUntil(this.destroyed)).subscribe(units => this.units = units.filter(unit => unit.isActive));
    this.api.catalogItem(this.itemId).pipe(takeUntil(this.destroyed)).subscribe({
      next: item => { this.item = item; this.type = item.type; this.reset(item); this.loading = false; },
      error: () => { this.message = this.t.translate('pageState.error'); this.loading = false; }
    });
  }

  ngOnDestroy(): void { this.destroyed.next(); this.destroyed.complete(); }

  selectTab(tab: 'main' | 'properties' | 'batches', event: Event): void {
    event.preventDefault();
    this.tab = tab;
    if (tab === 'main') void this.router.navigate(['/catalog/items', this.itemId, 'edit'], { queryParams: { back: this.backUrl, tab: null } });
    else void this.router.navigate(['/catalog/items', this.itemId, 'edit'], { queryParams: { back: this.backUrl, tab } });
  }

  async save(): Promise<void> {
    if (!this.canManage || this.saving) return;
    if (this.form.invalid) { this.form.markAllAsTouched(); this.message = this.t.translate('itemLeave.invalid'); return; }
    this.saving = true; this.message = '';
    const value = this.form.getRawValue();
    try {
      const saved = await firstValueFrom(this.api.saveCatalogItem(this.itemId, {
        ...value, type: this.type, fullName: value.fullName.trim() || null,
        articleNumber: value.articleNumber.trim() || null, description: value.description.trim() || null,
        rowVersion: this.item?.rowVersion ?? null
      }));
      this.item = saved; this.type = saved.type; this.reset(saved); this.message = this.t.translate('masterData.saved');
    } catch { this.message = this.t.translate('pageState.error'); }
    finally { this.saving = false; }
  }

  async saveCurrentTab(): Promise<void> {
    if (this.tab === 'properties') {
      if (this.propertiesComponent) await this.propertiesComponent.saveValues();
      return;
    }
    if (this.tab === 'batches') {
      if (this.propertiesComponent?.batchOpen) await this.propertiesComponent.saveBatch();
      return;
    }
    await this.save();
  }

  async saveAll(): Promise<boolean> {
    if (this.hasUnsavedChanges) await this.save();
    if (this.dirty && this.propertiesComponent?.dirty) {
      if (this.propertiesComponent.batchOpen) await this.propertiesComponent.saveBatch();
      else await this.propertiesComponent.saveValues();
    }
    return !this.dirty;
  }

  async finishLeave(action: 'save' | 'discard' | 'stay'): Promise<void> {
    if (this.saving || this.propertiesBusy) return;
    if (action === 'save') {
      const saved = await this.saveAll();
      if (!saved) return;
    } else if (action === 'discard') {
      this.reset(this.item!);
      this.propertiesComponent?.discardChanges();
    }
    const resolve = this.resolveLeave;
    this.leavePromise = undefined;
    this.resolveLeave = undefined;
    this.leaveDialogOpen = false;
    resolve?.(action !== 'stay');
  }

  setActive(active: boolean): void {
    if (!this.canManage || this.saving || this.hasUnsavedChanges || !this.item) return;
    this.saving = true;
    this.api.setCatalogItemActive(this.item.id, active).pipe(takeUntil(this.destroyed)).subscribe({
      next: () => { this.item = { ...this.item!, isActive: active }; this.message = this.t.translate('masterData.saved'); this.saving = false; },
      error: () => { this.message = this.t.translate('pageState.error'); this.saving = false; }
    });
  }

  close(): void { void this.router.navigateByUrl(this.backUrl); }
  displayClass(item: CatalogItemClassOption): string { return `${item.code} — ${item.name}`; }
  limitCostScale(event: Event): void {
    const input = event.target as HTMLInputElement;
    const [integerPart, decimalPart] = input.value.split('.');
    if (decimalPart === undefined || decimalPart.length <= 2) return;
    const value = `${integerPart}.${decimalPart.slice(0, 2)}`;
    input.value = value;
    this.form.controls.cost.setValue(value, { emitEvent: false });
  }

  private reset(item: CatalogItemDetails): void {
    this.form.reset({ workingName: item.workingName, fullName: item.fullName ?? '', articleNumber: item.articleNumber ?? '', baseUnitOfMeasureId: item.baseUnitOfMeasureId, catalogItemClassId: item.catalogItemClassId, cost: item.cost, description: item.description ?? '' });
    this.baseline = JSON.stringify(this.form.getRawValue());
    this.api.catalogItemClassOptions(item.type).pipe(takeUntil(this.destroyed)).subscribe(options => this.classOptions = options.filter(option => !option.isGroup));
  }
}
