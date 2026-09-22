import { MasterDataUiModule } from '../shared/master-data-ui.module';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { firstValueFrom, forkJoin, Observable } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { PageState, PageStateComponent } from '../../../core/ui/page-state.component';
import { TableActionsComponent } from '../../../core/ui/table-actions.component';
import { CatalogItemClassOption, CatalogItemDetails, CatalogItemSummary, CatalogItemType, CatalogTechnologyDetails, CatalogTechnologyMaterial, CatalogTechnologyStage, CatalogTechnologyStageOutput, CatalogTechnologyStatus, CatalogTechnologySummary, EquipmentOption, OptionItem, StorageOption, TechnologyMaterialConsumptionTrackingMode, TechnologyMaterialMovementKind, TechnologyStage, UnitSummary } from '../master-data.models';
import { MasterDataService } from '../master-data.service';

type DraftTechnology = CatalogTechnologyDetails;

@Component({
  selector: 'app-technologies',
  standalone: true,
  imports: [MasterDataUiModule, CommonModule, FormsModule, TranslocoPipe, PageStateComponent, TableActionsComponent],
  templateUrl: './technologies.component.html',
  styleUrls: ['../master-data.css', './technologies.component.css', './technology-materials.css']
})
export class TechnologiesComponent implements OnInit {
  materialItemName(id: string): string {
    return this.catalogItems.find(item => item.id === id)?.workingName ?? id;
  }

  leaveDialogOpen = false;
  private leavePromise?: Promise<boolean>;
  private resolveLeave?: (leave: boolean) => void;
  private newDraftBaseline = '';

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
    this.resolveLeave = undefined;
    this.leavePromise = undefined;
    resolve?.(leave);
  }

  async saveAndLeave(): Promise<void> {
    if (!this.canManage || !this.draft || this.isSaving || this.requests.saving()) return;
    if (!this.canSaveHeader()) {
      this.message = this.t.translate('masterDataReview.invalidTechnology');
      return;
    }
    this.isSaving = true;
    this.message = '';
    try {
      if (!this.draft.id) {
        this.acceptAll(await firstValueFrom(this.api.createTechnology(this.toPayload(this.draft))));
      } else {
        const id = this.draft.id;
        if (JSON.stringify(this.headerPayload(this.draft)) !== JSON.stringify(this.headerPayload(this.selected!))) {
          this.acceptHeader(await firstValueFrom(this.api.saveTechnologyHeader(id, this.headerPayload(this.draft))));
        }
        const stageShape = (value: CatalogTechnologyDetails) => ({
          stages: value.stages.map(stage => this.stageRowPayload(stage)), transitions: value.stageTransitions
        });
        if (JSON.stringify(stageShape(this.draft!)) !== JSON.stringify(stageShape(this.selected!))) {
          if (!this.canSaveStages()) throw new Error('Invalid stages');
          this.acceptStages(await firstValueFrom(this.api.saveTechnologyStages(id, this.stagesPayload())));
        }
        for (const stageId of this.draft!.stages.map(stage => stage.id)) {
          for (const section of ['materials', 'outputs', 'operations'] as const) {
            const stage = this.draft!.stages.find(item => item.id === stageId)!;
            const saved = this.selected!.stages.find(item => item.id === stageId);
            if (JSON.stringify(stage[section]) === JSON.stringify(saved?.[section] ?? [])) continue;
            const request = section === 'materials'
              ? this.api.saveTechnologyMaterials(id, stageId, this.materialsPayload(stage))
              : section === 'outputs'
                ? this.api.saveTechnologyOutputs(id, stageId, this.outputsPayload(stage))
                : this.api.saveTechnologyOperations(id, stageId, this.operationsPayload(stage));
            this.acceptStageSection(await firstValueFrom(request), stageId, section);
          }
        }
      }
      this.isSaving = false;
      this.load();
      this.finishLeave(true);
    } catch (error) {
      this.isSaving = false;
      this.message = this.errorMessage(error);
    }
  }

  page = 1;
  private listRequest = 0;
  total = 0;

  readonly requests = inject(MasterDataService);
  private readonly api = this.requests;
  private readonly auth = inject(AuthService);
  private readonly t = inject(TranslocoService);

  readonly canManage = this.auth.hasPermission(Permissions.masterDataManage);
  readonly trackingModes: TechnologyMaterialConsumptionTrackingMode[] = ['NormOnly', 'ActualConsumptionTracked'];
  readonly movementKinds: TechnologyMaterialMovementKind[] = ['Transfer', 'IssueToProduction', 'InternalMove'];
  readonly technologyStatuses: CatalogTechnologyStatus[] = ['InDevelopment', 'Active', 'NotUsed'];
  readonly itemTypes: CatalogItemType[] = ['Product', 'PrimaryMaterial', 'AuxiliaryMaterial', 'SemiFinishedProduct', 'Packaging'];

  items: CatalogTechnologySummary[] = [];
  catalogItems: CatalogItemSummary[] = [];
  productItems: CatalogItemSummary[] = [];
  productClasses: CatalogItemClassOption[] = [];
  units: UnitSummary[] = [];
  storages: StorageOption[] = [];
  departments: OptionItem[] = [];
  equipment: EquipmentOption[] = [];
  technologyStages: TechnologyStage[] = [];
  selected?: CatalogTechnologyDetails;
  selectedTechnologyId = '';
  draft?: DraftTechnology;
  selectedStageId = '';
  selectedMaterialId = '';
  nextStageCandidateId = '';
  stageEditorDraft?: CatalogTechnologyStage;
  stageEditorNextStageNumbers = '';
  stageDuplicateConfirmationOpen = false;
  stageSelectionOpen = false;
  selectedTechnologyStageId = '';
  viewedCatalogItem?: CatalogItemDetails;
  activeEditorTab: 'stages' | 'materials' = 'stages';
  targetItemText = '';
  search = '';
  isActive = '';
  state: PageState = 'loading';
  refsPage = 1;
  refsHasMore = false;
  refsState: PageState = 'loading';
  isSaving = false;
  message = '';
  sortKey: 'code' | 'name' | 'target' | 'status' = 'code';
  sortDirection: 'asc' | 'desc' = 'asc';
  stagePanelWidth = 42;
  stageColumnWidths = [145, 225, 185, 260];
  private activeResize?:
    | { kind: 'panels'; pointerId: number; target: HTMLElement; left: number; width: number }
    | { kind: 'column'; pointerId: number; target: HTMLElement; index: number; startX: number; startWidth: number };

  get stageTableWidth(): number {
    return this.stageColumnWidths.reduce((total, width) => total + width, 0);
  }

  startPanelResize(event: PointerEvent, container: HTMLElement): void {
    if (event.button !== 0) return;
    const target = event.currentTarget as HTMLElement;
    const bounds = container.getBoundingClientRect();
    this.activeResize = { kind: 'panels', pointerId: event.pointerId, target, left: bounds.left, width: bounds.width };
    target.setPointerCapture(event.pointerId);
    event.preventDefault();
  }

  startStageColumnResize(event: PointerEvent, index: number): void {
    if (event.button !== 0) return;
    const target = event.currentTarget as HTMLElement;
    this.activeResize = {
      kind: 'column', pointerId: event.pointerId, target, index,
      startX: event.clientX, startWidth: this.stageColumnWidths[index]
    };
    target.setPointerCapture(event.pointerId);
    event.preventDefault();
    event.stopPropagation();
  }

  continueResize(event: PointerEvent): void {
    const resize = this.activeResize;
    if (!resize || resize.pointerId !== event.pointerId) return;
    if (resize.kind === 'panels') {
      const percent = ((event.clientX - resize.left) / resize.width) * 100;
      this.stagePanelWidth = Math.min(70, Math.max(25, percent));
    } else {
      const minimumWidths = [85, 130, 110, 130];
      const widths = [...this.stageColumnWidths];
      widths[resize.index] = Math.max(minimumWidths[resize.index], resize.startWidth + event.clientX - resize.startX);
      this.stageColumnWidths = widths;
    }
  }

  finishResize(event: PointerEvent): void {
    const resize = this.activeResize;
    if (!resize || resize.pointerId !== event.pointerId) return;
    if (resize.target.hasPointerCapture(event.pointerId)) resize.target.releasePointerCapture(event.pointerId);
    this.activeResize = undefined;
  }

  resizePanelsByKeyboard(event: KeyboardEvent): void {
    if (event.key !== 'ArrowLeft' && event.key !== 'ArrowRight') return;
    this.stagePanelWidth = Math.min(70, Math.max(25, this.stagePanelWidth + (event.key === 'ArrowRight' ? 2 : -2)));
    event.preventDefault();
  }

  resizeStageColumnByKeyboard(event: KeyboardEvent, index: number): void {
    if (event.key !== 'ArrowLeft' && event.key !== 'ArrowRight') return;
    const minimumWidths = [85, 130, 110, 130];
    const widths = [...this.stageColumnWidths];
    widths[index] = Math.max(minimumWidths[index], widths[index] + (event.key === 'ArrowRight' ? 10 : -10));
    this.stageColumnWidths = widths;
    event.preventDefault();
    event.stopPropagation();
  }

  ngOnInit(): void {
    this.loadRefs();
    this.load();
  }

  load(more = false): void {
    if (more && this.state === 'loading') return;
    const requestId = ++this.listRequest;
    const requestedPage = more ? this.page + 1 : 1;
    this.state = 'loading';
    this.api.technologies({ search: this.search.trim(), isActive: this.isActive }, requestedPage).subscribe({
      next: x => {
        if (requestId !== this.listRequest) return;
        this.items = more ? [...this.items, ...x.items] : x.items;
        this.page = requestedPage; this.total = x.totalCount;
        this.state = this.items.length ? 'ready' : 'empty';
      },
      error: () => { if (requestId === this.listRequest) this.state = 'error'; }
    });
  }

  get sortedItems(): CatalogTechnologySummary[] {
    return [...this.items].sort((a, b) => this.compareItems(a, b));
  }

  get hasUnsavedChanges(): boolean {
    return !!this.draft && JSON.stringify(this.draft) !== (this.selected ? JSON.stringify(this.selected) : this.newDraftBaseline);
  }

  get editorTitle(): string {
    if (!this.draft?.id) return this.t.translate('technologies.title');
    return this.draft.name + (this.hasUnsavedChanges ? ' *' : '');
  }

  sortBy(key: 'code' | 'name' | 'target' | 'status'): void {
    if (this.sortKey === key) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortKey = key;
      this.sortDirection = 'asc';
    }
  }

  loadRefs(more = false): void {
    if (more && this.refsState === 'loading') return;
    const page = more ? this.refsPage + 1 : 1;
    this.refsState = 'loading';
    forkJoin({
      products: this.api.catalogItems('Product', '', 'true', page),
      primary: this.api.catalogItems('PrimaryMaterial', '', 'true', page),
      auxiliary: this.api.catalogItems('AuxiliaryMaterial', '', 'true', page),
      semi: this.api.catalogItems('SemiFinishedProduct', '', 'true', page),
      packaging: this.api.catalogItems('Packaging', '', 'true', page),
      classes: this.api.catalogItemClassOptions('Product', true),
      units: this.api.unitOptions(this.t.getActiveLang()),
      storages: this.api.storageOptions(),
      departments: this.api.departmentOptions(),
      equipment: this.api.equipmentOptions(),
      technologyStages: this.api.technologyStages(true)
    }).subscribe({
      next: refs => {
        this.refsPage = page;
        this.productItems = more ? [...this.productItems, ...refs.products.items] : refs.products.items;
        this.catalogItems = [...(more ? this.catalogItems : []), ...refs.products.items, ...refs.primary.items, ...refs.auxiliary.items, ...refs.semi.items, ...refs.packaging.items].sort((a, b) => a.workingName.localeCompare(b.workingName));
        this.productClasses = refs.classes.filter(x => !x.isGroup);
        this.units = refs.units.filter(x => x.isActive);
        this.storages = refs.storages;
        this.departments = refs.departments;
        this.equipment = refs.equipment;
        this.technologyStages = refs.technologyStages;
        if (this.draft?.catalogItemId) this.setTargetItemText(this.draft.catalogItemId);
        this.refsHasMore = this.catalogItems.length < refs.products.totalCount + refs.primary.totalCount + refs.auxiliary.totalCount + refs.semi.totalCount + refs.packaging.totalCount;
        this.refsState = 'ready';
      },
      error: () => this.refsState = 'error'
    });
  }

  select(item: CatalogTechnologySummary): void { if (this.requests.saving()) return;
    this.selectedTechnologyId = item.id;
  }

  private openEditor(id: string): void {
    this.api.technology(id).subscribe(x => {
      if (id !== this.selectedTechnologyId) return;
      this.selected = x;
      this.draft = this.clone(x);
      this.targetItemText = x.catalogItemName ?? '';
      this.selectedStageId = x.stages[0]?.id ?? '';
      this.selectedMaterialId = this.selectedStage?.materials[0]?.id ?? '';
      this.activeEditorTab = 'stages';
    });
  }

  private compareItems(a: CatalogTechnologySummary, b: CatalogTechnologySummary): number {
    const result = String(this.sortValue(a)).localeCompare(String(this.sortValue(b)), undefined, { numeric: true, sensitivity: 'base' });
    return (this.sortDirection === 'asc' ? result : -result) || a.code.localeCompare(b.code, undefined, { numeric: true, sensitivity: 'base' });
  }

  private sortValue(item: CatalogTechnologySummary): string | number {
    if (this.sortKey === 'target') return this.targetLabel(item);
    if (this.sortKey === 'status') return item.status;
    return item[this.sortKey];
  }

  create(): void { if (this.requests.saving()) return;
    this.selected = undefined;
    this.selectedTechnologyId = '';
    this.draft = {
      id: '',
      code: '',
      name: '',
      catalogItemId: this.productItems[0]?.id ?? null,
      catalogItemName: null,
      catalogItemClassId: null,
      catalogItemClassCode: null,
      catalogItemClassName: null,
      versionNo: 1,
      validFrom: null,
      validTo: null,
      isDefault: false,
      status: 'InDevelopment',
      description: null,
      isActive: true,
      stages: [],
      stageTransitions: [],
      rowVersion: ''
    };
    this.selectedStageId = '';
    this.selectedMaterialId = '';
    this.activeEditorTab = 'stages';
    this.setTargetItemText(this.draft.catalogItemId);
    this.newDraftBaseline = JSON.stringify(this.draft);
  }

  copy(): void { if (this.requests.saving()) return;
    if (!this.selected) return;
    this.draft = this.clone(this.selected);
    this.draft.id = '';
    this.draft.code = '';
    this.draft.rowVersion = '';
    this.draft.status = 'InDevelopment';
    this.draft.isActive = true;
    this.remapDraftIds(this.draft);
    this.newDraftBaseline = '';
    this.selected = undefined;
    this.selectedStageId = this.draft.stages[0]?.id ?? '';
    this.setTargetItemText(this.draft.catalogItemId);
  }

  copySelectedFromList(): void {
    if (!this.canManage || !this.selectedTechnologyId || this.requests.saving()) return;
    const id = this.selectedTechnologyId;
    this.api.technology(id).subscribe({ next: x => { if (id !== this.selectedTechnologyId) return; this.selected = x; this.copy(); }, error: error => this.message = this.errorMessage(error) });
  }

  editSelected(): void { if (this.requests.saving()) return;
    if (this.selectedTechnologyId) this.openEditor(this.selectedTechnologyId);
  }

  async closeEditor(): Promise<void> {
    if (!await this.requestLeave()) return;
    this.selected = undefined;
    this.selectedTechnologyId = '';
    this.draft = undefined;
    this.selectedStageId = '';
    this.selectedMaterialId = '';
    this.activeEditorTab = 'stages';
    this.targetItemText = '';
    this.message = '';
  }

  save(): void { if (!this.canManage || this.requests.saving()) return;
    if (!this.draft || !this.canManage || this.isSaving) return;
    if (!this.canSaveHeader()) { this.message = this.t.translate('masterDataReview.invalidTechnology'); return; }
    this.isSaving = true;
    this.state = 'saving';
    const request = this.draft.id
      ? this.api.saveTechnologyHeader(this.draft.id, this.headerPayload(this.draft))
      : this.api.createTechnology(this.toPayload(this.draft));
    request.subscribe({
      next: x => {
        if (this.draft?.id) this.acceptHeader(x);
        else this.acceptAll(x);
        this.message = this.t.translate('masterData.saved');
        this.isSaving = false;
        this.load();
      },
      error: error => {
        this.isSaving = false;
        this.state = 'error';
        this.message = this.errorMessage(error);
      }
    });
  }

  setActive(active: boolean): void { if (!this.canManage || this.requests.saving()) return;
    if (!this.selected || !this.canManage || this.hasUnsavedChanges) return;
    this.api.setTechnologyActive(this.selected.id, active).subscribe(() => {
      this.openEditor(this.selected!.id);
      this.load();
    });
  }

  canSaveHeader(): boolean {
    if (!this.draft) return false;
    if (!this.draft.name.trim()) return false;
    if (!this.technologyStatuses.includes(this.draft.status)) return false;
    if (!this.draft.catalogItemId && !this.draft.catalogItemClassId) return false;
    if (!Number.isInteger(this.draft.versionNo) || this.draft.versionNo < 1) return false;
    if (this.draft.validFrom && this.draft.validTo && this.draft.validTo < this.draft.validFrom) return false;
    return this.draft.status === 'InDevelopment' || this.draft.stages.length > 0;
  }

  canSaveStages(): boolean {
    return !!this.draft?.id && this.draft.stages.every(stage => !!stage.id && !!stage.technologyStageName.trim()
      && !!stage.technologyStageDepartmentId && Number.isInteger(stage.stageNumber) && stage.stageNumber > 0)
      && new Set(this.draft.stages.map(stage => stage.stageNumber)).size === this.draft.stages.length;
  }

  saveStages(): void {
    if (!this.canManage || this.isSaving || this.requests.saving()) return;
    if (!this.draft?.id || !this.selected || !this.canSaveStages()) { this.message = this.t.translate('masterDataReview.invalidTechnology'); return; }
    this.runSectionSave(this.api.saveTechnologyStages(this.draft.id, this.stagesPayload()), x => this.acceptStages(x));
  }

  saveMaterials(): void {
    if (!this.canManage || this.isSaving || this.requests.saving()) return;
    const stage = this.selectedStage;
    if (!this.draft?.id || !stage || !stage.materials.every(material =>
      material.catalogItemId && material.unitOfMeasureId && material.quantity > 0
      && material.routeSteps.every(route => route.fromStorageLocationId && route.leadTimeMinutes >= 0
        && (route.isConsumptionPoint || !!route.toStorageLocationId)))) { this.message = this.t.translate('masterDataReview.invalidTechnology'); return; }
    this.runSectionSave(this.api.saveTechnologyMaterials(this.draft.id, stage.id, this.materialsPayload(stage)),
      x => this.acceptStageSection(x, stage.id, 'materials'));
  }

  saveOutputs(): void {
    if (!this.canManage || this.isSaving || this.requests.saving()) return;
    const stage = this.selectedStage;
    if (!this.draft?.id || !stage || this.isSaving || !stage.outputs.every(output =>
      output.catalogItemId && output.unitOfMeasureId && output.receiptStorageLocationId && output.quantity > 0)) { this.message = this.t.translate('masterDataReview.invalidTechnology'); return; }
    this.runSectionSave(this.api.saveTechnologyOutputs(this.draft.id, stage.id, this.outputsPayload(stage)),
      x => this.acceptStageSection(x, stage.id, 'outputs'));
  }

  saveOperations(): void {
    if (!this.canManage || this.isSaving || this.requests.saving()) return;
    const stage = this.selectedStage;
    if (!this.draft?.id || !stage || this.isSaving || !stage.operations.every(operation =>
      !!operation.name.trim() && operation.workers > 0 && [operation.setupMinutes, operation.runMinutes, operation.laborMinutes].every(x => Number.isFinite(x) && x >= 0))) { this.message = this.t.translate('masterDataReview.invalidTechnology'); return; }
    this.runSectionSave(this.api.saveTechnologyOperations(this.draft.id, stage.id, this.operationsPayload(stage)),
      x => this.acceptStageSection(x, stage.id, 'operations'));
  }

  targetKind(): 'item' | 'class' {
    return this.draft?.catalogItemClassId ? 'class' : 'item';
  }

  changeTarget(kind: 'item' | 'class'): void {
    if (!this.draft) return;
    if (kind === 'item') {
      this.draft.catalogItemClassId = null;
      this.draft.catalogItemId = this.draft.catalogItemId ?? this.productItems[0]?.id ?? null;
      this.setTargetItemText(this.draft.catalogItemId);
    } else {
      this.draft.catalogItemId = null;
      this.draft.catalogItemClassId = this.draft.catalogItemClassId ?? this.productClasses[0]?.id ?? null;
      this.targetItemText = '';
    }
  }

  onTargetItemInput(value: string): void {
    this.targetItemText = value;
    if (!this.draft) return;

    const normalizedValue = value.trim().toLocaleLowerCase();
    const item = this.productItems.find(x => x.workingName.trim().toLocaleLowerCase() === normalizedValue);
    this.draft.catalogItemId = item?.id ?? null;
    this.draft.catalogItemName = item?.workingName ?? null;
  }

  get selectedStage(): CatalogTechnologyStage | undefined {
    return this.draft?.stages.find(x => x.id === this.selectedStageId) ?? this.draft?.stages[0];
  }

  get selectedMaterial(): CatalogTechnologyMaterial | undefined {
    return this.selectedStage?.materials.find(x => x.id === this.selectedMaterialId) ?? this.selectedStage?.materials[0];
  }

  openStageEditor(): void {
    if (!this.draft) return;
    const stage = this.newStage();
    stage.stageNumber = this.nextStageNumber();
    this.stageEditorDraft = stage;
    this.stageEditorNextStageNumbers = '';
    this.stageDuplicateConfirmationOpen = false;
  }

  closeStageEditor(): void {
    this.stageEditorDraft = undefined;
    this.stageEditorNextStageNumbers = '';
    this.stageDuplicateConfirmationOpen = false;
  }

  openStageSelection(): void {
    if (!this.draft) return;
    this.selectedTechnologyStageId = '';
    this.stageSelectionOpen = true;
  }

  closeStageSelection(): void {
    this.stageSelectionOpen = false;
    this.selectedTechnologyStageId = '';
  }

  addSelectedTechnologyStage(): void {
    if (!this.draft) return;
    const masterStage = this.technologyStages.find(stage => stage.id === this.selectedTechnologyStageId);
    if (!masterStage) return;

    const stage: CatalogTechnologyStage = {
      id: this.newId(),
      technologyStageId: masterStage.id,
      technologyStageCode: masterStage.code,
      technologyStageName: masterStage.name,
      stageNumber: this.nextStageNumber(),
      plannedDurationMinutes: null,
      technologyStageDepartmentId: masterStage.departmentId,
      technologyStageDepartmentName: masterStage.departmentName,
      equipmentId: null,
      equipmentName: null,
      description: null,
      materials: [],
      outputs: [],
      operations: [],
      rowVersion: ''
    };
    this.draft.stages = [...this.draft.stages, stage];
    this.selectedStageId = this.draft.stages.at(-1)!.id;
    this.selectedMaterialId = '';
    this.closeStageSelection();
    if (this.draft.id) {
      this.runSectionSave(this.api.addExistingTechnologyStage(this.draft.id, this.stageRowPayload(stage)),
        x => this.acceptStages(x));
    }
  }

  saveStageEditor(): void {
    const nextStageNumbers = this.parseStageEditorNextStageNumbers();
    if (!this.canSaveStageEditor || !this.draft || !this.stageEditorDraft || !nextStageNumbers) return;
    this.api.hasTechnologyStageDuplicate(this.stageEditorDraft.technologyStageName, this.stageEditorDraft.technologyStageDepartmentId!).subscribe({
      next: duplicateExists => {
        if (duplicateExists) {
          this.stageDuplicateConfirmationOpen = true;
          return;
        }
        this.commitStageEditor(nextStageNumbers);
      },
      error: error => this.message = this.errorMessage(error)
    });
  }

  confirmStageEditorDuplicate(): void {
    const nextStageNumbers = this.parseStageEditorNextStageNumbers();
    if (!nextStageNumbers) return;
    this.stageDuplicateConfirmationOpen = false;
    this.commitStageEditor(nextStageNumbers);
  }

  cancelStageEditorDuplicate(): void {
    this.stageDuplicateConfirmationOpen = false;
  }

  private commitStageEditor(nextStageNumbers: number[]): void {
    if (!this.draft || !this.stageEditorDraft) return;
    const stage = this.stageEditorDraft;
    this.draft.stages = [...this.draft.stages, stage];
    const nextStages = this.draft.stages.filter(item => item.id !== stage.id && nextStageNumbers.includes(item.stageNumber));
    const newTransitions = nextStages.map(nextStage => ({
      id: this.newId(),
      fromCatalogTechnologyStageId: stage.id,
      toCatalogTechnologyStageId: nextStage.id,
      rowVersion: ''
    }));
    this.draft.stageTransitions = [...this.draft.stageTransitions, ...newTransitions];
    this.selectedStageId = stage.id;
    this.selectedMaterialId = '';
    this.closeStageEditor();
    if (this.draft.id) {
      this.runSectionSave(this.api.addNewTechnologyStage(this.draft.id, {
        stage: this.stageRowPayload(stage),
        stageTransitions: newTransitions
      }), x => this.acceptStages(x));
    }
  }

  onStageEditorNameInput(value: string): void {
    if (!this.stageEditorDraft) return;
    this.stageEditorDraft.technologyStageId = '';
    this.stageEditorDraft.technologyStageCode = '';
    this.stageEditorDraft.technologyStageName = value;
  }

  get canSaveStageEditor(): boolean {
    const stage = this.stageEditorDraft;
    const nextStageNumbers = this.parseStageEditorNextStageNumbers();
    return !!stage && !!stage.technologyStageName.trim() && !!stage.technologyStageDepartmentId && stage.stageNumber > 0 && !!nextStageNumbers
      && !this.draft?.stages.some(item => item.stageNumber === stage.stageNumber)
      && nextStageNumbers.every(number => this.draft?.stages.some(item => item.stageNumber === number));
  }

  private parseStageEditorNextStageNumbers(): number[] | undefined {
    const value = this.stageEditorNextStageNumbers.trim();
    if (!value || value === '0') return [];
    if (!/^\d+(\s*,\s*\d+)*$/.test(value)) return undefined;
    const numbers = value.split(',').map(item => Number(item.trim()));
    return numbers.every(number => number > 0) && new Set(numbers).size === numbers.length ? numbers : undefined;
  }

  selectStage(stage: CatalogTechnologyStage): void {
    this.selectedStageId = stage.id;
    this.selectedMaterialId = stage.materials.length ? stage.materials[0].id : '';
    this.nextStageCandidateId = '';
  }

  nextStages(stage: CatalogTechnologyStage): CatalogTechnologyStage[] {
    if (!this.draft) return [];
    const targetIds = new Set(this.draft.stageTransitions
      .filter(link => link.fromCatalogTechnologyStageId === stage.id)
      .map(link => link.toCatalogTechnologyStageId));
    return this.draft.stages.filter(item => targetIds.has(item.id)).sort((left, right) => left.stageNumber - right.stageNumber);
  }

  availableNextStages(stage: CatalogTechnologyStage): CatalogTechnologyStage[] {
    if (!this.draft) return [];
    const linkedIds = new Set(this.nextStages(stage).map(item => item.id));
    return this.draft.stages.filter(item => item.id !== stage.id && !linkedIds.has(item.id)
      && !this.hasStagePath(item.id, stage.id));
  }

  addNextStage(stage: CatalogTechnologyStage): void {
    if (!this.draft || !this.nextStageCandidateId
      || !this.availableNextStages(stage).some(item => item.id === this.nextStageCandidateId)) return;
    this.draft.stageTransitions = [...this.draft.stageTransitions, {
      id: this.newId(),
      fromCatalogTechnologyStageId: stage.id,
      toCatalogTechnologyStageId: this.nextStageCandidateId,
      rowVersion: ''
    }];
    this.nextStageCandidateId = '';
  }

  removeNextStage(stage: CatalogTechnologyStage, nextStage: CatalogTechnologyStage): void {
    if (!this.draft) return;
    this.draft.stageTransitions = this.draft.stageTransitions.filter(link =>
      link.fromCatalogTechnologyStageId !== stage.id || link.toCatalogTechnologyStageId !== nextStage.id);
  }

  private hasStagePath(fromStageId: string, toStageId: string): boolean {
    if (!this.draft) return false;
    const visited = new Set<string>();
    const visit = (stageId: string): boolean => {
      if (stageId === toStageId) return true;
      if (!visited.add(stageId)) return false;
      return this.draft!.stageTransitions
        .filter(link => link.fromCatalogTechnologyStageId === stageId)
        .some(link => visit(link.toCatalogTechnologyStageId));
    };
    return visit(fromStageId);
  }

  selectStageById(stageId: string): void {
    const stage = this.draft?.stages.find(x => x.id === stageId);
    if (stage) this.selectStage(stage);
  }

  nextStageNumbers(stage: CatalogTechnologyStage): string {
    if (!this.draft) return '';

    return this.draft.stageTransitions
      .filter(x => x.fromCatalogTechnologyStageId === stage.id)
      .map(x => this.draft!.stages.find(stageItem => stageItem.id === x.toCatalogTechnologyStageId))
      .filter((stageItem): stageItem is CatalogTechnologyStage => !!stageItem)
      .map(stageItem => stageItem.stageNumber)
      .sort((left, right) => left - right)
      .join(', ');
  }

  removeStage(stage: CatalogTechnologyStage): void {
    if (!this.draft || (this.draft.stages.length <= 1 && this.draft.status !== 'InDevelopment')) return;
    const removedIndex = this.draft.stages.findIndex(x => x.id === stage.id);
    this.draft.stages = this.draft.stages.filter(x => x.id !== stage.id);
    this.draft.stageTransitions = this.draft.stageTransitions.filter(x => x.fromCatalogTechnologyStageId !== stage.id && x.toCatalogTechnologyStageId !== stage.id);
    const nextIndex = Math.min(Math.max(removedIndex, 0), this.draft.stages.length - 1);
    this.selectedStageId = this.draft.stages[nextIndex]?.id ?? '';
    this.selectedMaterialId = '';
  }

  moveStage(stage: CatalogTechnologyStage, offset: -1 | 1): void {
    if (!this.draft) return;
    const index = this.draft.stages.findIndex(x => x.id === stage.id);
    const targetIndex = index + offset;
    if (index < 0 || targetIndex < 0 || targetIndex >= this.draft.stages.length) return;
    const stages = [...this.draft.stages];
    [stages[index], stages[targetIndex]] = [stages[targetIndex], stages[index]];
    this.draft.stages = stages.map((item, lineIndex) => ({ ...item, stageNumber: (lineIndex + 1) * 10 }));
  }

  isFirstStage(stage: CatalogTechnologyStage): boolean {
    return this.draft?.stages[0]?.id === stage.id;
  }

  isLastStage(stage: CatalogTechnologyStage): boolean {
    return this.draft?.stages[this.draft.stages.length - 1]?.id === stage.id;
  }

  isStageIncomplete(stage: CatalogTechnologyStage): boolean {
    return !stage.technologyStageName.trim() || stage.stageNumber < 1;
  }

  onTechnologyStageChange(stage: CatalogTechnologyStage): void {
    const master = this.technologyStages.find(x => x.id === stage.technologyStageId);
    if (!master) return;
    stage.technologyStageCode = master.code;
    stage.technologyStageName = master.name;
  }

  addMaterial(stage: CatalogTechnologyStage): void {
    const item = this.catalogItems[0];
    const unit = this.units[0];
    if (!item || !unit) return;
    const material: CatalogTechnologyMaterial = {
      id: this.newId(),
      catalogItemId: item.id,
      catalogItemName: item.workingName,
      unitOfMeasureId: unit.id,
      unitOfMeasureName: unit.displayName,
      quantity: 1,
      consumptionTrackingMode: 'NormOnly',
      defaultSourceStorageLocationId: this.storages[0]?.id ?? null,
      defaultSourceStorageLocationName: null,
      scrapPercent: 0,
      isOptional: false,
      note: null,
      routeSteps: [],
      rowVersion: ''
    };
    stage.materials = [...stage.materials, material];
    this.selectedMaterialId = material.id;
  }

  removeMaterial(stage: CatalogTechnologyStage, material: CatalogTechnologyMaterial): void {
    stage.materials = stage.materials.filter(x => x.id !== material.id);
    this.selectedMaterialId = stage.materials[0]?.id ?? '';
  }

  addRouteStep(material: CatalogTechnologyMaterial): void {
    const storage = this.storages[0];
    if (!storage) return;
    material.routeSteps = [...material.routeSteps, {
      id: this.newId(),
      lineNo: material.routeSteps.length + 1,
      fromStorageLocationId: storage.id,
      fromStorageLocationName: storage.name,
      toStorageLocationId: null,
      toStorageLocationName: null,
      isConsumptionPoint: true,
      movementKind: 'IssueToProduction',
      leadTimeMinutes: 0,
      note: null,
      rowVersion: ''
    }];
  }

  removeRouteStep(material: CatalogTechnologyMaterial, id: string): void {
    material.routeSteps = material.routeSteps.filter(x => x.id !== id).map((x, index) => ({ ...x, lineNo: index + 1 }));
  }

  addOutput(stage: CatalogTechnologyStage): void {
    const item = this.productItems[0];
    const unit = this.units[0];
    const storage = this.storages[0];
    if (!item || !unit || !storage) return;
    stage.outputs = [...stage.outputs, {
      id: this.newId(),
      catalogItemId: item.id,
      catalogItemName: item.workingName,
      unitOfMeasureId: unit.id,
      unitOfMeasureName: unit.displayName,
      quantity: 1,
      receiptStorageLocationId: storage.id,
      receiptStorageLocationName: storage.name,
      isPrimary: stage.outputs.length === 0,
      note: null,
      rowVersion: ''
    }];
  }

  removeOutput(stage: CatalogTechnologyStage, id: string): void {
    stage.outputs = stage.outputs.filter(x => x.id !== id);
  }

  outputItemText(output: CatalogTechnologyStageOutput): string {
    return output.catalogItemName || this.productItems.find(item => item.id === output.catalogItemId)?.workingName || '';
  }

  outputItemOptionLabel(item: CatalogItemSummary): string {
    return item.articleNumber ? `${item.workingName} · ${item.articleNumber}` : item.workingName;
  }

  onOutputItemInput(output: CatalogTechnologyStageOutput, value: string): void {
    const normalized = value.trim().toLocaleLowerCase();
    const item = this.productItems.find(candidate =>
      candidate.workingName.trim().toLocaleLowerCase() === normalized
      || this.outputItemOptionLabel(candidate).toLocaleLowerCase() === normalized);
    output.catalogItemId = item?.id ?? '';
    output.catalogItemName = item?.workingName ?? value;
  }

  openCatalogItemViewer(catalogItemId: string): void {
    if (!catalogItemId) return;
    this.api.catalogItem(catalogItemId).subscribe(item => this.viewedCatalogItem = item);
  }

  closeCatalogItemViewer(): void {
    this.viewedCatalogItem = undefined;
  }

  addOperation(stage: CatalogTechnologyStage): void {
    stage.operations = [...stage.operations, {
      id: this.newId(),
      code: '',
      name: '',
      departmentId: stage.technologyStageDepartmentId,
      departmentName: null,
      equipmentId: stage.equipmentId,
      equipmentName: null,
      setupMinutes: 0,
      runMinutes: 0,
      laborMinutes: 0,
      workers: 1,
      note: null,
      rowVersion: ''
    }];
  }

  removeOperation(stage: CatalogTechnologyStage, id: string): void {
    stage.operations = stage.operations.filter(x => x.id !== id);
  }

  addLink(): void {
    if (!this.draft || this.draft.stages.length < 2) return;
    this.draft.stageTransitions = [...this.draft.stageTransitions, {
      id: this.newId(),
      fromCatalogTechnologyStageId: this.draft.stages[0].id,
      toCatalogTechnologyStageId: this.draft.stages[1].id,
      rowVersion: ''
    }];
  }

  removeLink(id: string): void {
    if (!this.draft) return;
    this.draft.stageTransitions = this.draft.stageTransitions.filter(x => x.id !== id);
  }

  itemName(id: string): string {
    return this.catalogItems.find(x => x.id === id)?.workingName ?? this.productItems.find(x => x.id === id)?.workingName ?? '';
  }

  unitName(id: string): string { return this.units.find(x => x.id === id)?.displayName ?? ''; }
  storageName(id: string | null): string { return id ? this.storages.find(x => x.id === id)?.name ?? '' : ''; }
  stageName(id: string): string { return this.draft?.stages.find(x => x.id === id)?.technologyStageName ?? ''; }
  targetLabel(item: CatalogTechnologySummary): string { return item.catalogItemName ?? `${item.catalogItemClassCode} - ${item.catalogItemClassName}`; }

  private newStage(): CatalogTechnologyStage {
    return {
      id: this.newId(),
      technologyStageId: '',
      technologyStageCode: '',
      technologyStageName: '',
      stageNumber: 10,
      plannedDurationMinutes: null,
      technologyStageDepartmentId: null,
      technologyStageDepartmentName: null,
      equipmentId: null,
      equipmentName: null,
      description: null,
      materials: [],
      outputs: [],
      operations: [],
      rowVersion: ''
    };
  }

  private nextStageNumber(): number {
    const numbers = this.draft?.stages.map(x => x.stageNumber) ?? [];
    return numbers.length ? Math.max(...numbers) + 10 : 10;
  }

  private errorMessage(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) return this.t.translate('technologies.saveError');
    const payload = error.error as { detail?: string; title?: string; errors?: Record<string, string[]> } | string | null;
    if (typeof payload === 'string' && payload.trim()) return payload;
    if (payload && typeof payload === 'object') {
      const validationMessage = Object.values(payload.errors ?? {}).flat().find(Boolean);
      return validationMessage ?? payload.detail ?? payload.title ?? this.t.translate('technologies.saveError');
    }
    return this.t.translate('technologies.saveError');
  }

  private toPayload(draft: DraftTechnology): object {
    return {
      ...draft,
      catalogItemId: draft.catalogItemId || null,
      catalogItemClassId: draft.catalogItemClassId || null,
      description: draft.description || null,
      stages: draft.stages.map(stage => ({
        ...stage,
        technologyStageId: stage.technologyStageId || null,
        plannedDurationMinutes: stage.plannedDurationMinutes ?? null,
        technologyStageDepartmentId: stage.technologyStageDepartmentId || null,
        equipmentId: stage.equipmentId || null,
        description: stage.description || null,
        materials: stage.materials.map(material => ({
          ...material,
          defaultSourceStorageLocationId: material.defaultSourceStorageLocationId || null,
          note: material.note || null,
          routeSteps: material.routeSteps.map(route => ({
            ...route,
            toStorageLocationId: route.isConsumptionPoint ? null : route.toStorageLocationId,
            note: route.note || null
          }))
        })),
        outputs: stage.outputs.map(output => ({ ...output, note: output.note || null })),
        operations: stage.operations.map(operation => ({
          ...operation,
          departmentId: operation.departmentId || null,
          equipmentId: operation.equipmentId || null,
          note: operation.note || null
        }))
      }))
    };
  }

  private headerPayload(draft: DraftTechnology): object {
    return {
      code: draft.code,
      name: draft.name,
      catalogItemId: draft.catalogItemId || null,
      catalogItemClassId: draft.catalogItemClassId || null,
      versionNo: draft.versionNo,
      validFrom: draft.validFrom,
      validTo: draft.validTo,
      isDefault: draft.isDefault,
      status: draft.status,
      description: draft.description || null,
      rowVersion: draft.rowVersion
    };
  }

  private stagesPayload(): object {
    const draft = this.draft!;
    const selected = this.selected!;
    return {
      stages: draft.stages.map(stage => this.stageRowPayload(stage)),
      stageTransitions: draft.stageTransitions.map(link => ({ ...link, rowVersion: link.rowVersion || null })),
      deletedStages: this.deletedRows(selected.stages, draft.stages),
      deletedStageTransitions: this.deletedRows(selected.stageTransitions, draft.stageTransitions)
    };
  }

  private stageRowPayload(stage: CatalogTechnologyStage): object {
    return {
      id: stage.id,
      technologyStageId: stage.technologyStageId || null,
      technologyStageCode: stage.technologyStageCode,
      technologyStageName: stage.technologyStageName,
      stageNumber: stage.stageNumber,
      plannedDurationMinutes: stage.plannedDurationMinutes,
      technologyStageDepartmentId: stage.technologyStageDepartmentId,
      equipmentId: stage.equipmentId || null,
      description: stage.description || null,
      rowVersion: stage.rowVersion || null
    };
  }

  private materialsPayload(stage: CatalogTechnologyStage): object {
    const savedStage = this.selected?.stages.find(item => item.id === stage.id);
    const savedMaterials = savedStage?.materials ?? [];
    const currentRoutes = stage.materials.flatMap(material => material.routeSteps);
    const savedRoutes = savedMaterials.flatMap(material => material.routeSteps);
    const deletedMaterialIds = new Set(this.deletedRows(savedMaterials, stage.materials).map(item => item.id));
    return {
      materials: stage.materials.map(material => ({
        ...material,
        defaultSourceStorageLocationId: material.defaultSourceStorageLocationId || null,
        note: material.note || null,
        rowVersion: material.rowVersion || null,
        routeSteps: material.routeSteps.map(route => ({
          ...route,
          toStorageLocationId: route.isConsumptionPoint ? null : route.toStorageLocationId,
          note: route.note || null,
          rowVersion: route.rowVersion || null
        }))
      })),
      deletedMaterials: this.deletedRows(savedMaterials, stage.materials),
      deletedRouteSteps: this.deletedRows(
        savedRoutes.filter(route => !savedMaterials.some(material => deletedMaterialIds.has(material.id) && material.routeSteps.some(item => item.id === route.id))),
        currentRoutes)
    };
  }

  private outputsPayload(stage: CatalogTechnologyStage): object {
    const saved = this.selected?.stages.find(item => item.id === stage.id)?.outputs ?? [];
    return {
      outputs: stage.outputs.map(output => ({ ...output, note: output.note || null, rowVersion: output.rowVersion || null })),
      deletedOutputs: this.deletedRows(saved, stage.outputs)
    };
  }

  private operationsPayload(stage: CatalogTechnologyStage): object {
    const saved = this.selected?.stages.find(item => item.id === stage.id)?.operations ?? [];
    return {
      operations: stage.operations.map(operation => ({
        ...operation,
        departmentId: operation.departmentId || null,
        equipmentId: operation.equipmentId || null,
        note: operation.note || null,
        rowVersion: operation.rowVersion || null
      })),
      deletedOperations: this.deletedRows(saved, stage.operations)
    };
  }

  private deletedRows<T extends { id: string; rowVersion: string }>(saved: T[], current: { id: string }[]): { id: string; rowVersion: string }[] {
    const currentIds = new Set(current.map(item => item.id));
    return saved.filter(item => !currentIds.has(item.id)).map(item => ({ id: item.id, rowVersion: item.rowVersion }));
  }

  private runSectionSave(request: Observable<CatalogTechnologyDetails>, accept: (value: CatalogTechnologyDetails) => void): void {
    this.isSaving = true;
    this.state = 'saving';
    request.subscribe({
      next: value => {
        accept(value);
        this.message = this.t.translate('masterData.saved');
        this.isSaving = false;
        this.state = 'ready';
        this.load();
      },
      error: error => {
        this.isSaving = false;
        this.state = 'error';
        this.message = this.errorMessage(error);
      }
    });
  }

  private acceptAll(value: CatalogTechnologyDetails): void {
    this.selected = value;
    this.selectedTechnologyId = value.id;
    this.draft = this.clone(value);
    this.targetItemText = value.catalogItemName ?? '';
    this.selectedStageId = value.stages[0]?.id ?? '';
    this.selectedMaterialId = this.selectedStage?.materials[0]?.id ?? '';
  }

  private acceptHeader(value: CatalogTechnologyDetails): void {
    if (!this.draft) return;
    const children = { stages: this.draft.stages, stageTransitions: this.draft.stageTransitions };
    const nextDraft = this.clone(value);
    nextDraft.stages = children.stages;
    nextDraft.stageTransitions = children.stageTransitions;
    this.selected = value;
    this.draft = nextDraft;
    this.selectedTechnologyId = value.id;
    this.targetItemText = nextDraft.catalogItemName ?? this.targetItemText;
  }

  private acceptStages(value: CatalogTechnologyDetails): void {
    if (!this.draft) return;
    const oldDraft = this.draft;
    const nextDraft = this.clone(oldDraft);
    nextDraft.rowVersion = value.rowVersion;
    nextDraft.stages = value.stages.map(serverStage => {
      const local = oldDraft.stages.find(item => item.id === serverStage.id);
      return local ? { ...serverStage, materials: local.materials, outputs: local.outputs, operations: local.operations } : serverStage;
    });
    nextDraft.stageTransitions = value.stageTransitions;
    this.selected = value;
    this.draft = nextDraft;
    this.selectedStageId = nextDraft.stages.some(stage => stage.id === this.selectedStageId)
      ? this.selectedStageId : nextDraft.stages[0]?.id ?? '';
  }

  private acceptStageSection(value: CatalogTechnologyDetails, stageId: string,
    section: 'materials' | 'outputs' | 'operations'): void {
    if (!this.draft) return;
    const serverStage = value.stages.find(stage => stage.id === stageId);
    const localStage = this.draft.stages.find(stage => stage.id === stageId);
    if (!serverStage || !localStage) return;
    localStage[section] = this.clone(serverStage[section]) as never;
    localStage.rowVersion = serverStage.rowVersion;
    this.draft.rowVersion = value.rowVersion;
    this.selected = value;
    if (section === 'materials') this.selectedMaterialId = localStage.materials[0]?.id ?? '';
  }

  private setTargetItemText(catalogItemId: string | null): void {
    this.targetItemText = this.productItems.find(x => x.id === catalogItemId)?.workingName ?? this.draft?.catalogItemName ?? '';
  }

  private clone<T>(value: T): T {
    return JSON.parse(JSON.stringify(value)) as T;
  }

  private remapDraftIds(draft: DraftTechnology): void {
    const stageMap = new Map<string, string>();
    for (const stage of draft.stages) {
      const oldId = stage.id;
      stage.id = this.newId();
      stage.rowVersion = '';
      stageMap.set(oldId, stage.id);
      stage.materials.forEach(material => {
        material.id = this.newId();
        material.rowVersion = '';
        material.routeSteps.forEach(route => { route.id = this.newId(); route.rowVersion = ''; });
      });
      stage.outputs.forEach(output => { output.id = this.newId(); output.rowVersion = ''; });
      stage.operations.forEach(operation => { operation.id = this.newId(); operation.rowVersion = ''; });
    }
    draft.stageTransitions.forEach(link => {
      link.id = this.newId();
      link.rowVersion = '';
      link.fromCatalogTechnologyStageId = stageMap.get(link.fromCatalogTechnologyStageId) ?? link.fromCatalogTechnologyStageId;
      link.toCatalogTechnologyStageId = stageMap.get(link.toCatalogTechnologyStageId) ?? link.toCatalogTechnologyStageId;
    });
  }

  private newId(): string {
    return crypto.randomUUID();
  }
}
