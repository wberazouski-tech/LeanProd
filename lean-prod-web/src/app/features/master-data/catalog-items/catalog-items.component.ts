import { MasterDataUiModule } from '../shared/master-data-ui.module';
import { CommonModule } from '@angular/common';
import { Component, HostListener, OnDestroy, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { firstValueFrom, Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { CatalogItemClassDetails, CatalogItemClassOption, CatalogItemClassSummary, CatalogItemDetails, CatalogItemSummary, CatalogItemType, UnitSummary } from '../master-data.models';
import { MasterDataService } from '../master-data.service';
import { PageState, PageStateComponent } from '../../../core/ui/page-state.component';
import { MoneyFormatPipe } from '../../../core/ui/number-format.pipe';
import { buildCatalogClassTree, descendantLeafIds } from './catalog-class-tree';
import { TableActionsComponent } from '../../../core/ui/table-actions.component';

@Component({
  selector: 'app-catalog-items', standalone: true,
  imports: [MasterDataUiModule, CommonModule, ReactiveFormsModule, TranslocoPipe, RouterLink, RouterLinkActive, PageStateComponent, MoneyFormatPipe, TableActionsComponent],
  templateUrl: './catalog-items.component.html',
  styleUrls: ['../master-data.css', './catalog-items.component.css']
})
export class CatalogItemsComponent implements OnInit, OnDestroy {
  leaveDialogOpen = false;
  isSaving = false;
  private formBaseline = '';
  private itemRequest = 0;
  private leavePromise?: Promise<boolean>;
  private resolveLeave?: (leave: boolean) => void;

  get hasUnsavedChanges(): boolean {
    return this.editorOpen && JSON.stringify(this.form.getRawValue()) !== this.formBaseline;
  }

  requestLeave(): boolean | Promise<boolean> {
    if (this.isSaving || this.requests.saving()) return false;
    if (this.leavePromise) return this.leavePromise;
    if (!this.hasUnsavedChanges) return true;
    this.leaveDialogOpen = true;
    this.leavePromise = new Promise<boolean>(resolve => this.resolveLeave = resolve);
    return this.leavePromise;
  }

  finishLeave(leave: boolean): void {
    if (this.isSaving || this.requests.saving()) return;
    this.leaveDialogOpen = false;
    const resolve = this.resolveLeave;
    this.leavePromise = undefined;
    this.resolveLeave = undefined;
    resolve?.(leave);
  }

  onCatalogTab(event: MouseEvent): void {
    if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
    const target = event.currentTarget as HTMLAnchorElement;
    if (target.pathname !== this.router.url.split(/[?#]/)[0]) return;
    event.preventDefault();
    void this.closeEditor();
  }

  get propertiesBackUrl(): string { return this.router.url.split('?')[0]; }
  page = 1;
  private listRequest = 0;

  readonly requests = inject(MasterDataService);
  private readonly api = this.requests;
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly t = inject(TranslocoService);
  private readonly auth = inject(AuthService);
  private readonly destroyed = new Subject<void>();

  type: CatalogItemType = 'Product';
  items: CatalogItemSummary[] = [];
  classes: (CatalogItemClassSummary & { depth: number })[] = [];
  classSelectionOptions: CatalogItemClassOption[] = [];
  units: UnitSummary[] = [];
  selected?: CatalogItemDetails;
  selectedClass?: CatalogItemClassDetails;
  total = 0;
  classTotal = 0;
  classPage = 1;
  private classRequest = 0;
  editorOpen = false;
  classEditorOpen = false;
  message = ''; state: PageState = 'loading';
  classMessage = '';
  classState: PageState = 'loading';
  draggedItem?: CatalogItemSummary;
  dropTargetClassId?: string;
  readonly expandedClassIds = new Set<string>();
  sortKey: 'articleNumber' | 'workingName' | 'fullName' | 'baseUnitName' | 'cost' | 'isActive' = 'articleNumber'; 
  sortDirection: 'asc' | 'desc' = 'asc';
  classSortKey: 'code' | 'name' | 'isGroup' | 'isActive' = 'code'; classSortDirection: 'asc' | 'desc' = 'asc';
  resizingColumn?: 'workingName' | 'articleNumber' | 'fullName' | 'class' | 'baseUnit' | 'cost' | 'status' | 'actions';
  resizeStartX = 0;
  resizeStartWidth = 0;
  columnWidths: Record<'workingName' | 'articleNumber' | 'fullName' | 'class' | 'baseUnit' | 'cost' | 'status' | 'actions', number> = {
    workingName: 290,
    articleNumber: 145,
    fullName: 290,
    class: 215,
    baseUnit: 130,
    cost: 120,
    status: 82,
    actions: 58
  };
  get catalogTableWidth(): number {
    const visibleColumns: (keyof typeof this.columnWidths)[] = ['workingName', 'articleNumber', 'fullName', 'class', 'baseUnit', 'cost', 'status'];
    if (this.canManage) visibleColumns.push('actions');
    return visibleColumns.reduce((total, column) => total + this.columnWidths[column], 0);
  }
  readonly canManage = this.auth.hasPermission(Permissions.masterDataManage);
  readonly filters = this.fb.nonNullable.group({ search: '', isActive: '' });
  readonly classFilters = this.fb.nonNullable.group({ search: '', isActive: '', isGroup: '' });
  readonly form = this.fb.nonNullable.group({
    workingName: ['', [Validators.required, Validators.maxLength(200)]],
    fullName: ['', Validators.maxLength(500)],
    articleNumber: ['', Validators.maxLength(100)],
    baseUnitOfMeasureId: ['', Validators.required],
    catalogItemClassId: ['', Validators.required],
    cost: ['0.00', [Validators.required, Validators.pattern(/^(0|[1-9]\d{0,15})(\.\d{1,2})?$/)]],
    description: ['', Validators.maxLength(1000)]
  });
  readonly classForm = this.fb.nonNullable.group({
    code: ['', Validators.maxLength(50)],
    name: ['', [Validators.required, Validators.maxLength(200)]],
    isGroup: false,
    parentId: ''
  });

  ngOnInit(): void {
    this.route.data.pipe(takeUntil(this.destroyed)).subscribe(data => {
      this.type = data['type'] as CatalogItemType;
      this.selected = undefined;
      this.selectedClass = undefined;
      this.editorOpen = false;
      this.classEditorOpen = false;
      this.filters.reset({ search: '', isActive: '' });
      this.classFilters.reset({ search: '', isActive: '', isGroup: '' });
      this.load();
      this.loadClasses();
      this.loadClassSelectionOptions();
    });
    this.loadUnits();
  }

  ngOnDestroy(): void { this.destroyed.next(); this.destroyed.complete(); }
  load(more = false): void {
    if (more && this.state === 'loading') return;
    const requestId = ++this.listRequest;
    const requestedPage = more ? this.page + 1 : 1;
    this.state = 'loading';
    const f = this.filters.getRawValue();
    this.api.catalogItems(this.type, f.search.trim(), f.isActive, requestedPage).subscribe({
      next: x => {
        if (requestId !== this.listRequest) return;
        this.items = more ? [...this.items, ...x.items] : x.items;
        this.page = requestedPage; this.total = x.totalCount;
        this.state = this.items.length ? 'ready' : 'empty';
      },
      error: () => { if (requestId === this.listRequest) this.state = 'error'; }
    });
  }
  loadClasses(more = false): void {
    if (more && this.classState === 'loading') return;
    const requestId = ++this.classRequest;
    const page = more ? this.classPage + 1 : 1;
    this.classState = 'loading';
    const f = this.classFilters.getRawValue();
    this.api.catalogItemClasses(this.type, f.search.trim(), f.isActive, f.isGroup, page).subscribe({
      next: x => { if (requestId !== this.classRequest) return; this.classes = buildCatalogClassTree(more ? [...this.classes, ...x.items] : x.items); this.classPage = page; this.classTotal = x.totalCount; this.classState = this.classes.length ? 'ready' : 'empty'; },
      error: () => { if (requestId === this.classRequest) this.classState = 'error'; }
    });
  }
  get sortedItems(): CatalogItemSummary[] { return this.filteredItems.sort((a, b) => this.compareItems(a, b)); }
  get sortedClasses(): (CatalogItemClassSummary & { depth: number })[] {
    const ids = new Set(this.classes.map(item => item.id));
    const children = new Map<string | null, (CatalogItemClassSummary & { depth: number })[]>();
    for (const item of this.classes) {
      if (!item.isGroup && !item.parentId && item.code.toUpperCase() === 'GENERAL') continue;
      const parentId = item.parentId && ids.has(item.parentId) ? item.parentId : null;
      children.set(parentId, [...(children.get(parentId) ?? []), item]);
    }
    const result: (CatalogItemClassSummary & { depth: number })[] = [];
    const addBranch = (parentId: string | null): void => {
      const siblings = [...(children.get(parentId) ?? [])].sort((a, b) => this.compareClasses(a, b));
      for (const item of siblings) {
        result.push(item);
        if (this.expandedClassIds.has(item.id)) addBranch(item.id);
      }
    };
    addBranch(null);
    return result;
  }
  get classOptions(): CatalogItemClassOption[] { return this.classSelectionOptions.filter(x => !x.isGroup); }
  get parentClassOptions(): CatalogItemClassOption[] { const blocked = this.selectedClass ? this.descendantClassIds(this.selectedClass.id).add(this.selectedClass.id) : new Set<string>(); return this.classSelectionOptions.filter(x => x.isGroup && !blocked.has(x.id)); }
  
  sortBy(key: 'articleNumber' | 'workingName' | 'fullName' | 'baseUnitName' | 'cost' | 'isActive'): void {
    if (this.sortKey === key) 
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc'; 
    else { 
      this.sortKey = key; this.sortDirection = 'asc'; } }
  
  sortClassesBy(key: 'code' | 'name' | 'isGroup' | 'isActive'): void { if (this.classSortKey === key) this.classSortDirection = this.classSortDirection === 'asc' ? 'desc' : 'asc'; else { this.classSortKey = key; this.classSortDirection = 'asc'; } }
  startResize(column: 'workingName' | 'articleNumber' | 'fullName' | 'class' | 'baseUnit' | 'cost' | 'status' | 'actions', event: MouseEvent): void { event.preventDefault(); event.stopPropagation(); this.resizingColumn = column; this.resizeStartX = event.clientX; this.resizeStartWidth = this.columnWidths[column]; }
  @HostListener('document:mousemove', ['$event']) resize(event: MouseEvent): void { if (!this.resizingColumn) return; const minWidth = this.resizingColumn === 'status' ? 78 : this.resizingColumn === 'actions' ? 46 : 80; this.columnWidths[this.resizingColumn] = Math.max(minWidth, this.resizeStartWidth + event.clientX - this.resizeStartX); }
  @HostListener('document:mouseup') stopResize(): void { this.resizingColumn = undefined; }
  hasClassChildren(item: CatalogItemClassSummary): boolean { return this.classes.some(child => child.parentId === item.id); }
  isClassExpanded(item: CatalogItemClassSummary): boolean { return this.expandedClassIds.has(item.id); }
  toggleClassExpanded(item: CatalogItemClassSummary, event: Event): void { event.stopPropagation(); if (this.expandedClassIds.has(item.id)) this.expandedClassIds.delete(item.id); else this.expandedClassIds.add(item.id); }
  selectClass(item: CatalogItemClassSummary): void { if (this.requests.saving()) return; this.api.catalogItemClass(item.id).subscribe(x => { this.selectedClass = x; this.classForm.reset({ code: x.code, name: x.name, isGroup: x.isGroup, parentId: x.parentId ?? '' }); this.classEditorOpen = false; }); }
  editClass(item: CatalogItemClassSummary, event: Event): void { if (this.requests.saving()) return; event.stopPropagation(); this.api.catalogItemClass(item.id).subscribe(x => { this.selectedClass = x; this.classForm.reset({ code: x.code, name: x.name, isGroup: x.isGroup, parentId: x.parentId ?? '' }); this.classEditorOpen = true; }); }
  createClass(isGroup: boolean): void { if (this.requests.saving()) return; const parentId = this.selectedClass?.isGroup ? this.selectedClass.id : ''; this.selectedClass = undefined; this.classMessage = ''; this.classForm.reset({ code: '', name: '', isGroup, parentId }); this.classEditorOpen = true; }
  closeClassEditor(): void { if (this.requests.saving()) return; this.classEditorOpen = false; }
  clearClassSelection(): void { this.selectedClass = undefined; this.classEditorOpen = false; }
  saveClass(): void { if (!this.canManage || this.requests.saving()) return;
    if (this.classForm.invalid) return;
    this.classState = 'saving';
    const value = this.classForm.getRawValue();
    const body = { ...value, type: this.type, parentId: value.parentId || null, rowVersion: this.selectedClass?.rowVersion ?? null };
    this.api.saveCatalogItemClass(this.selectedClass?.id, body).subscribe({ next: x => { this.selectedClass = x; this.classMessage = this.t.translate('masterData.saved'); this.classEditorOpen = false; this.classState = 'success'; this.loadClasses(); this.loadClassSelectionOptions(); this.load(); }, error: () => this.classState = 'error' });
  }
  setClassActive(active: boolean): void { if (!this.canManage || this.requests.saving()) return; if (!this.selectedClass) return; this.api.setCatalogItemClassActive(this.selectedClass.id, active).subscribe(() => { this.selectedClass = { ...this.selectedClass!, isActive: active }; this.classEditorOpen = false; this.loadClasses(); this.loadClassSelectionOptions(); }); }
  select(item: CatalogItemSummary): void { if (this.requests.saving()) return; this.loadItem(item.id, false); }
  edit(item: CatalogItemSummary, event: Event): void { if (this.requests.saving()) return; event.stopPropagation(); this.loadItem(item.id, true); }
  editSelected(): void { if (this.requests.saving()) return; if (this.selected) this.loadItem(this.selected.id, true); }
  copySelected(): void { if (this.requests.saving()) return; ++this.itemRequest; if (!this.selected) return; const x = this.selected; this.selected = undefined; this.message = ''; this.form.reset({ workingName: x.workingName, fullName: x.fullName ?? '', articleNumber: '', baseUnitOfMeasureId: x.baseUnitOfMeasureId, catalogItemClassId: x.catalogItemClassId, cost: x.cost, description: x.description ?? '' }); this.formBaseline = ''; this.editorOpen = true; }
  startItemDrag(item: CatalogItemSummary, event: DragEvent): void { if (!this.canManage) return; this.draggedItem = item; event.dataTransfer?.setData('text/plain', item.id); if (event.dataTransfer) event.dataTransfer.effectAllowed = 'move'; }
  endItemDrag(): void { this.draggedItem = undefined; this.dropTargetClassId = undefined; }
  canDropOnClass(item: CatalogItemClassSummary): boolean { return !!this.draggedItem && !item.isGroup && item.isActive && this.draggedItem.catalogItemClassId !== item.id; }
  dragOverClass(item: CatalogItemClassSummary, event: DragEvent): void {
    if (!this.canDropOnClass(item)) return;
    event.preventDefault();
    if (event.dataTransfer) event.dataTransfer.dropEffect = 'move';
    this.dropTargetClassId = item.id;
  }
  leaveClassDropTarget(item: CatalogItemClassSummary): void { if (this.dropTargetClassId === item.id) this.dropTargetClassId = undefined; }
  dropItemOnClass(item: CatalogItemClassSummary, event: DragEvent): void { if (!this.canManage || this.requests.saving()) return;
    event.preventDefault();
    event.stopPropagation();
    if (!this.canDropOnClass(item) || !this.draggedItem) return;
    const dragged = this.draggedItem;
    this.dropTargetClassId = undefined;
    this.state = 'saving';
    this.api.changeCatalogItemClass(dragged.id, item.id).subscribe({
      next: updated => {
        this.items = this.items.map(x => x.id === updated.id ? updated : x);
        if (this.selected?.id === updated.id) this.selected = updated;
        this.message = this.t.translate('masterData.saved');
        this.draggedItem = undefined;
        this.state = this.filteredItems.length ? 'ready' : 'empty';
      },
      error: () => { this.draggedItem = undefined; this.state = 'error'; }
    });
  }
  create(): void { if (this.requests.saving()) return; ++this.itemRequest; this.selected = undefined; this.message = ''; this.form.reset({ workingName: '', fullName: '', articleNumber: '', baseUnitOfMeasureId: '', catalogItemClassId: this.selectedClass && !this.selectedClass.isGroup ? this.selectedClass.id : '', cost: '0.00', description: '' }); this.formBaseline = JSON.stringify(this.form.getRawValue()); this.editorOpen = true; }
  async closeEditor(): Promise<void> { if (!await this.requestLeave()) return; ++this.itemRequest; this.editorOpen = false; this.message = ''; }
  limitCostScale(event: Event): void {
    const input = event.target as HTMLInputElement;
    const [integerPart, decimalPart] = input.value.split('.');
    if (decimalPart === undefined || decimalPart.length <= 2) return;
    const value = `${integerPart}.${decimalPart.slice(0, 2)}`;
    input.value = value;
    this.form.controls.cost.setValue(value, { emitEvent: false });
  }
  async save(leave = false): Promise<void> {
    if (!this.canManage || this.requests.saving() || this.isSaving) return;
    if (this.form.invalid) { this.form.markAllAsTouched(); this.message = this.t.translate('itemLeave.invalid'); return; }
    const value = this.form.getRawValue();
    const body = { ...value, type: this.type, fullName: value.fullName.trim() || null, articleNumber: value.articleNumber.trim() || null, description: value.description.trim() || null, rowVersion: this.selected?.rowVersion ?? null };
    this.isSaving = true;
    this.message = '';
    try {
      const item = await firstValueFrom(this.api.saveCatalogItem(this.selected?.id, body));
      this.selected = item;
      this.resetItemForm(item);
      this.message = this.t.translate('masterData.saved');
      this.isSaving = false;
      this.load();
      if (leave) this.finishLeave(true);
    } catch {
      this.isSaving = false;
      this.message = this.t.translate('pageState.error');
    }
  }
  private resetItemForm(x: CatalogItemDetails): void {
    this.form.reset({ workingName: x.workingName, fullName: x.fullName ?? '', articleNumber: x.articleNumber ?? '', baseUnitOfMeasureId: x.baseUnitOfMeasureId, catalogItemClassId: x.catalogItemClassId, cost: x.cost, description: x.description ?? '' });
    this.formBaseline = JSON.stringify(this.form.getRawValue());
  }
  setActive(active: boolean): void { if (!this.canManage || this.requests.saving()) return; if (!this.selected || this.hasUnsavedChanges) return; this.api.setCatalogItemActive(this.selected.id, active).subscribe(() => { this.selected = { ...this.selected!, isActive: active }; this.editorOpen = false; this.load(); }); }
  displayUnit(item: CatalogItemSummary): string { return item.baseUnitSymbol ? `${item.baseUnitName} (${item.baseUnitSymbol})` : item.baseUnitName; }
  displayClass(item: { code: string; name: string } | CatalogItemSummary): string { return 'catalogItemClassCode' in item ? `${item.catalogItemClassCode} — ${item.catalogItemClassName}` : `${item.code} — ${item.name}`; }

  private get filteredItems(): CatalogItemSummary[] { if (!this.selectedClass || (!this.selectedClass.parentId && !this.selectedClass.isGroup)) return [...this.items]; const ids = this.selectedClass.isGroup ? this.descendantClassIds(this.selectedClass.id) : new Set([this.selectedClass.id]); return this.items.filter(x => ids.has(x.catalogItemClassId)); }
  private compareItems(a: CatalogItemSummary, b: CatalogItemSummary): number { const left = this.sortValue(a); const right = this.sortValue(b); const result = typeof left === 'number' && typeof right === 'number' ? left - right : String(left).localeCompare(String(right)); return (this.sortDirection === 'asc' ? result : -result) || (a.articleNumber ?? '').localeCompare(b.articleNumber ?? '') || a.workingName.localeCompare(b.workingName); }
  private sortValue(item: CatalogItemSummary): string | number { if (this.sortKey === 'cost') return item.cost.split('.')[0].padStart(16, '0') + '.' + (item.cost.split('.')[1] ?? '').padEnd(2, '0'); if (this.sortKey === 'isActive') return Number(item.isActive); if (this.sortKey === 'baseUnitName') return this.displayUnit(item).toLocaleLowerCase(); return (item[this.sortKey] ?? '').toLocaleLowerCase(); }
  private compareClasses(a: CatalogItemClassSummary, b: CatalogItemClassSummary): number { const left = this.classSortValue(a); const right = this.classSortValue(b); const result = typeof left === 'number' && typeof right === 'number' ? left - right : String(left).localeCompare(String(right)); return (this.classSortDirection === 'asc' ? result : -result) || a.code.localeCompare(b.code); }
  private classSortValue(item: CatalogItemClassSummary): string | number { if (this.classSortKey === 'isActive') return Number(item.isActive); if (this.classSortKey === 'isGroup') return Number(item.isGroup); return item[this.classSortKey].toLocaleLowerCase(); }
  private descendantClassIds(id: string): Set<string> { return descendantLeafIds(this.classes, id); }
  private loadClassSelectionOptions(): void { this.api.catalogItemClassOptions(this.type).subscribe(x => this.classSelectionOptions = x.sort((a, b) => a.code.localeCompare(b.code))); }
  private loadUnits(): void { this.api.unitOptions(this.t.getActiveLang()).subscribe(x => this.units = x.filter(unit => unit.isActive)); }
  private loadItem(id: string, openEditor: boolean): void {
    const request = ++this.itemRequest;
    this.api.catalogItem(id).pipe(takeUntil(this.destroyed)).subscribe({
      next: x => {
        if (request !== this.itemRequest) return;
        this.selected = x;
        this.resetItemForm(x);
        this.message = '';
        this.editorOpen = openEditor && this.canManage;
      },
      error: () => this.message = this.t.translate('pageState.error')
    });
  }
}
