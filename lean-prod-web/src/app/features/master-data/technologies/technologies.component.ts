import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { PageState, PageStateComponent } from '../../../core/ui/page-state.component';
import { TableActionsComponent } from '../../../core/ui/table-actions.component';
import { CatalogItemClassOption, CatalogItemSummary, CatalogItemType, CatalogTechnologyDetails, CatalogTechnologyMaterial, CatalogTechnologyStage, CatalogTechnologyStatus, CatalogTechnologySummary, DepartmentSummary, EquipmentOption, OptionItem, StorageOption, TechnologyMaterialConsumptionTrackingMode, TechnologyMaterialMovementKind, TechnologyStageLinkType, TechnologyStageTemplate, UnitSummary } from '../master-data.models';
import { MasterDataService } from '../master-data.service';

type DraftTechnology = CatalogTechnologyDetails;

@Component({
  selector: 'app-technologies',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslocoPipe, PageStateComponent, TableActionsComponent],
  templateUrl: './technologies.component.html',
  styleUrls: ['../master-data.css', './technologies.component.css']
})
export class TechnologiesComponent implements OnInit {
  private readonly api = inject(MasterDataService);
  private readonly auth = inject(AuthService);
  private readonly t = inject(TranslocoService);

  readonly canManage = this.auth.hasPermission(Permissions.masterDataManage);
  readonly linkTypes: TechnologyStageLinkType[] = ['FinishToStart', 'StartToStart', 'FinishToFinish', 'StartToFinish'];
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
  templates: TechnologyStageTemplate[] = [];
  selected?: CatalogTechnologyDetails;
  selectedTechnologyId = '';
  draft?: DraftTechnology;
  selectedStageId = '';
  selectedMaterialId = '';
  targetItemText = '';
  search = '';
  isActive = '';
  state: PageState = 'loading';
  refsState: PageState = 'loading';
  message = '';
  sortKey: 'code' | 'name' | 'target' | 'status' = 'code';
  sortDirection: 'asc' | 'desc' = 'asc';

  ngOnInit(): void {
    this.loadRefs();
    this.load();
  }

  load(): void {
    this.state = 'loading';
    this.api.technologies({ search: this.search.trim(), isActive: this.isActive }).subscribe({
      next: x => { this.items = x.items; this.state = x.items.length ? 'ready' : 'empty'; },
      error: error => {
        this.state = 'error';
        this.message = this.errorMessage(error);
      }
    });
  }

  get sortedItems(): CatalogTechnologySummary[] {
    return [...this.items].sort((a, b) => this.compareItems(a, b));
  }

  get hasUnsavedChanges(): boolean {
    return !!this.selected && !!this.draft && JSON.stringify(this.draft) !== JSON.stringify(this.selected);
  }

  sortBy(key: 'code' | 'name' | 'target' | 'status'): void {
    if (this.sortKey === key) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortKey = key;
      this.sortDirection = 'asc';
    }
  }

  loadRefs(): void {
    this.refsState = 'loading';
    forkJoin({
      products: this.api.catalogItems('Product', '', 'true'),
      primary: this.api.catalogItems('PrimaryMaterial', '', 'true'),
      auxiliary: this.api.catalogItems('AuxiliaryMaterial', '', 'true'),
      semi: this.api.catalogItems('SemiFinishedProduct', '', 'true'),
      packaging: this.api.catalogItems('Packaging', '', 'true'),
      classes: this.api.catalogItemClassOptions('Product', true),
      units: this.api.unitOptions(this.t.getActiveLang()),
      storages: this.api.storageOptions(),
      departments: this.api.departmentOptions(),
      equipment: this.api.equipmentOptions(),
      templates: this.api.technologyStageTemplates(true)
    }).subscribe({
      next: refs => {
        this.productItems = refs.products.items;
        this.catalogItems = [...refs.products.items, ...refs.primary.items, ...refs.auxiliary.items, ...refs.semi.items, ...refs.packaging.items].sort((a, b) => a.workingName.localeCompare(b.workingName));
        this.productClasses = refs.classes.filter(x => !x.isGroup);
        this.units = refs.units.filter(x => x.isActive);
        this.storages = refs.storages;
        this.departments = refs.departments;
        this.equipment = refs.equipment;
        this.templates = refs.templates;
        if (this.draft?.catalogItemId) this.setTargetItemText(this.draft.catalogItemId);
        this.refsState = 'ready';
      },
      error: () => this.refsState = 'error'
    });
  }

  select(item: CatalogTechnologySummary): void {
    this.selectedTechnologyId = item.id;
  }

  private openEditor(id: string): void {
    this.api.technology(id).subscribe(x => {
      this.selected = x;
      this.draft = this.clone(x);
      this.targetItemText = x.catalogItemName ?? '';
      this.selectedStageId = x.stages[0]?.id ?? '';
      this.selectedMaterialId = this.selectedStage?.materials[0]?.id ?? '';
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

  create(): void {
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
      stageLinks: [],
      rowVersion: ''
    };
    this.selectedStageId = '';
    this.selectedMaterialId = '';
    this.setTargetItemText(this.draft.catalogItemId);
  }

  copy(): void {
    if (!this.selected) return;
    this.draft = this.clone(this.selected);
    this.draft.id = '';
    this.draft.code = '';
    this.draft.rowVersion = '';
    this.draft.status = 'InDevelopment';
    this.draft.isActive = true;
    this.remapDraftIds(this.draft);
    this.selected = undefined;
    this.selectedStageId = this.draft.stages[0]?.id ?? '';
    this.setTargetItemText(this.draft.catalogItemId);
  }

  editSelected(): void {
    if (this.selectedTechnologyId) this.openEditor(this.selectedTechnologyId);
  }

  closeEditor(): void {
    this.selected = undefined;
    this.selectedTechnologyId = '';
    this.draft = undefined;
    this.selectedStageId = '';
    this.selectedMaterialId = '';
    this.targetItemText = '';
    this.message = '';
  }

  save(): void {
    if (!this.draft || !this.canManage) return;
    if (!this.canSaveDraft()) return;
    this.state = 'saving';
    this.api.saveTechnology(this.draft.id || undefined, this.toPayload(this.draft)).subscribe({
      next: x => {
        this.selected = x;
        this.selectedTechnologyId = x.id;
        this.draft = this.clone(x);
        this.targetItemText = x.catalogItemName ?? '';
        this.selectedStageId = x.stages[0]?.id ?? '';
        this.selectedMaterialId = this.selectedStage?.materials[0]?.id ?? '';
        this.message = this.t.translate('masterData.saved');
        this.load();
      },
      error: error => {
        this.state = 'error';
        this.message = this.errorMessage(error);
      }
    });
  }

  setActive(active: boolean): void {
    if (!this.selected || !this.canManage) return;
    this.api.setTechnologyActive(this.selected.id, active).subscribe(() => {
      this.selected = { ...this.selected!, isActive: active };
      if (this.draft) this.draft.isActive = active;
      this.load();
    });
  }

  canSaveDraft(): boolean {
    if (!this.draft) return false;
    if (!this.draft.name.trim()) return false;
    if (!this.technologyStatuses.includes(this.draft.status)) return false;
    if (!this.draft.catalogItemId && !this.draft.catalogItemClassId) return false;
    if (this.draft.versionNo < 1) return false;
    if (this.draft.status !== 'InDevelopment' && this.draft.stages.length === 0) return false;
    if (this.hasDuplicateStageCodes()) return false;
    return this.draft.stages.every(stage =>
      !!stage.id && !!stage.name.trim() && stage.lineNo > 0
      && stage.materials.every(material => material.catalogItemId && material.unitOfMeasureId && material.quantity > 0)
      && stage.outputs.every(output => output.catalogItemId && output.unitOfMeasureId && output.receiptStorageLocationId && output.quantity > 0)
      && stage.operations.every(operation => !!operation.name.trim() && operation.workers > 0)
      && stage.materials.every(material => material.routeSteps.every(route =>
        route.fromStorageLocationId && route.leadTimeMinutes >= 0
        && (route.isConsumptionPoint || !!route.toStorageLocationId))));
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

  addStage(): void {
    if (!this.draft) return;
    const stage = this.newStage();
    stage.lineNo = this.draft.stages.length + 1;
    stage.code = this.nextStageCode();
    this.draft.stages = [...this.draft.stages, stage];
    this.selectedStageId = stage.id;
    this.selectedMaterialId = '';
  }

  selectStage(stage: CatalogTechnologyStage): void {
    this.selectedStageId = stage.id;
    this.selectedMaterialId = stage.materials.length ? stage.materials[0].id : '';
  }

  removeStage(stage: CatalogTechnologyStage): void {
    if (!this.draft || (this.draft.stages.length <= 1 && this.draft.status !== 'InDevelopment')) return;
    const removedIndex = this.draft.stages.findIndex(x => x.id === stage.id);
    this.draft.stages = this.draft.stages.filter(x => x.id !== stage.id).map((x, index) => ({ ...x, lineNo: index + 1 }));
    this.draft.stageLinks = this.draft.stageLinks.filter(x => x.fromStageId !== stage.id && x.toStageId !== stage.id);
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
    this.draft.stages = stages.map((item, lineIndex) => ({ ...item, lineNo: lineIndex + 1 }));
  }

  isFirstStage(stage: CatalogTechnologyStage): boolean {
    return this.draft?.stages[0]?.id === stage.id;
  }

  isLastStage(stage: CatalogTechnologyStage): boolean {
    return this.draft?.stages[this.draft.stages.length - 1]?.id === stage.id;
  }

  isStageIncomplete(stage: CatalogTechnologyStage): boolean {
    return !stage.name.trim() || stage.lineNo < 1;
  }

  isStageCodeDuplicate(stage: CatalogTechnologyStage): boolean {
    if (!this.draft || !stage.code.trim()) return false;
    const code = stage.code.trim().toLocaleUpperCase();
    return this.draft.stages.filter(item => item.code.trim().toLocaleUpperCase() === code).length > 1;
  }

  applyTemplate(stage: CatalogTechnologyStage): void {
    const template = this.templates.find(x => x.id === stage.stageTemplateId);
    if (!template) return;
    stage.code = template.code;
    stage.name = template.name;
    stage.description = template.description;
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
      routeSteps: []
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
      note: null
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
      note: null
    }];
  }

  removeOutput(stage: CatalogTechnologyStage, id: string): void {
    stage.outputs = stage.outputs.filter(x => x.id !== id);
  }

  addOperation(stage: CatalogTechnologyStage): void {
    stage.operations = [...stage.operations, {
      id: this.newId(),
      code: '',
      name: '',
      departmentId: stage.departmentId,
      departmentName: null,
      equipmentId: stage.equipmentId,
      equipmentName: null,
      setupMinutes: 0,
      runMinutes: 0,
      laborMinutes: 0,
      workers: 1,
      note: null
    }];
  }

  removeOperation(stage: CatalogTechnologyStage, id: string): void {
    stage.operations = stage.operations.filter(x => x.id !== id);
  }

  addLink(): void {
    if (!this.draft || this.draft.stages.length < 2) return;
    this.draft.stageLinks = [...this.draft.stageLinks, {
      id: this.newId(),
      fromStageId: this.draft.stages[0].id,
      toStageId: this.draft.stages[1].id,
      linkType: 'FinishToStart',
      lagMinutes: 0
    }];
  }

  removeLink(id: string): void {
    if (!this.draft) return;
    this.draft.stageLinks = this.draft.stageLinks.filter(x => x.id !== id);
  }

  itemName(id: string): string {
    return this.catalogItems.find(x => x.id === id)?.workingName ?? this.productItems.find(x => x.id === id)?.workingName ?? '';
  }

  unitName(id: string): string { return this.units.find(x => x.id === id)?.displayName ?? ''; }
  storageName(id: string | null): string { return id ? this.storages.find(x => x.id === id)?.name ?? '' : ''; }
  stageName(id: string): string { return this.draft?.stages.find(x => x.id === id)?.name ?? ''; }
  targetLabel(item: CatalogTechnologySummary): string { return item.catalogItemName ?? `${item.catalogItemClassCode} - ${item.catalogItemClassName}`; }

  private newStage(): CatalogTechnologyStage {
    return {
      id: this.newId(),
      stageTemplateId: null,
      stageTemplateName: null,
      code: 'STAGE-1',
      name: '',
      lineNo: 1,
      plannedDurationMinutes: null,
      departmentId: null,
      departmentName: null,
      equipmentId: null,
      equipmentName: null,
      description: null,
      materials: [],
      outputs: [],
      operations: []
    };
  }

  private hasDuplicateStageCodes(): boolean {
    if (!this.draft) return false;
    const codes = this.draft.stages.map(stage => stage.code.trim().toLocaleUpperCase()).filter(Boolean);
    return new Set(codes).size !== codes.length;
  }

  private nextStageCode(): string {
    const existing = new Set(this.draft?.stages.map(stage => stage.code.trim().toLocaleUpperCase()).filter(Boolean) ?? []);
    let number = (this.draft?.stages.length ?? 0) + 1;
    while (existing.has(`STAGE-${number}`)) number += 1;
    return `STAGE-${number}`;
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
        plannedDurationMinutes: stage.plannedDurationMinutes ?? null,
        departmentId: stage.departmentId || null,
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
      stageMap.set(oldId, stage.id);
      stage.materials.forEach(material => {
        material.id = this.newId();
        material.routeSteps.forEach(route => route.id = this.newId());
      });
      stage.outputs.forEach(output => output.id = this.newId());
      stage.operations.forEach(operation => operation.id = this.newId());
    }
    draft.stageLinks.forEach(link => {
      link.id = this.newId();
      link.fromStageId = stageMap.get(link.fromStageId) ?? link.fromStageId;
      link.toStageId = stageMap.get(link.toStageId) ?? link.toStageId;
    });
  }

  private newId(): string {
    return crypto.randomUUID();
  }
}
