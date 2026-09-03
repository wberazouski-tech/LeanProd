import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { EquipmentDetails, EquipmentOption, EquipmentStateEvent, EquipmentSummary, EquipmentType, OptionItem } from '../master-data.models';
import { MasterDataService } from '../master-data.service';
import { PageState, PageStateComponent } from '../../../core/ui/page-state.component';
import { TableActionsComponent } from '../../../core/ui/table-actions.component';

@Component({ selector: 'app-equipment', standalone: true, imports: [CommonModule, ReactiveFormsModule, TranslocoPipe, PageStateComponent, TableActionsComponent], templateUrl: './equipment.component.html', styleUrls: ['../master-data.css', './equipment.component.css'] })
export class EquipmentComponent implements OnInit {
  private readonly api = inject(MasterDataService); private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService); private readonly t = inject(TranslocoService);
  readonly canManage = this.auth.hasPermission(Permissions.masterDataManage);
  readonly states = ['Operational', 'Maintenance', 'Repair', 'OutOfService', 'Decommissioned'];
  @ViewChild('equipmentDialog') equipmentDialog?: ElementRef<HTMLDialogElement>;
  @ViewChild('typeDialog') typeDialog?: ElementRef<HTMLDialogElement>;
  @ViewChild('stateDialog') stateDialog?: ElementRef<HTMLDialogElement>;
  items: EquipmentSummary[] = []; options: EquipmentOption[] = []; departments: OptionItem[] = [];
  types: EquipmentType[] = []; history: EquipmentStateEvent[] = []; selected?: EquipmentDetails;
  selectedRowId?: string;
  editingType?: EquipmentType; editingState?: EquipmentStateEvent; total = 0; message = ''; state: PageState = 'loading';
  sortKey: 'name' | 'inventoryNumber' | 'equipmentType' | 'isActive' = 'name'; sortDirection: 'asc' | 'desc' = 'asc';
  readonly filters = this.fb.nonNullable.group({ search: '', isActive: '', departmentId: '', equipmentTypeId: '', state: '' });
  appliedFilters = this.filters.getRawValue();
  readonly form = this.fb.nonNullable.group({ name: ['', Validators.required], inventoryNumber: '', equipmentTypeId: '', departmentId: ['', Validators.required], parentEquipmentId: '', serialNumber: '', manufacturer: '', model: '', commissionedOn: '', description: '' });
  readonly typeForm = this.fb.nonNullable.group({ name: ['', Validators.required], description: '', isActive: true });
  readonly stateForm = this.fb.nonNullable.group({ state: ['', Validators.required], startedAtLocal: [this.localNow(), Validators.required], endedAtLocal: '', comment: '' });

  ngOnInit(): void { this.load(); this.loadReferences(); }
  load(): void { this.state = 'loading'; const f = this.filters.getRawValue(); this.appliedFilters = { ...f, search: f.search.trim() }; this.api.equipment(f).subscribe({ next: x => { this.items = x.items; this.total = x.totalCount; this.state = x.items.length ? 'ready' : 'empty'; }, error: () => this.state = 'error' }); }
  loadReferences(): void { this.api.departmentOptions().subscribe(x => this.departments = x); this.api.equipmentOptions().subscribe(x => this.options = x); this.api.equipmentTypes().subscribe(x => this.types = x); }
  get filterSummary(): string {
    const f = this.appliedFilters;
    const parts: string[] = [];
    const type = this.types.find(x => x.id === f.equipmentTypeId);
    const department = this.departments.find(x => x.id === f.departmentId);
    if (type) parts.push(`${this.t.translate('equipment.type')}: ${type.name}`);
    if (f.search) parts.push(`${this.t.translate('admin.name')}: ${f.search}`);
    if (department) parts.push(`${this.t.translate('masterData.department')}: ${department.name}`);
    if (f.isActive) parts.push(`${this.t.translate('admin.status')}: ${this.t.translate(f.isActive === 'true' ? 'admin.active' : 'admin.inactive')}`);
    return parts.join(' · ');
  }
  get sortedItems(): EquipmentSummary[] { return [...this.items].sort((a, b) => this.compareItems(a, b)); }
  sortBy(key: 'name' | 'inventoryNumber' | 'equipmentType' | 'isActive'): void {
    if (this.sortKey === key) 
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc'; 
    else { 
      this.sortKey = key; this.sortDirection = 'asc'; } }
  filterByType(equipmentTypeId = ''): void { this.filters.controls.equipmentTypeId.setValue(equipmentTypeId); this.selected = undefined; this.selectedRowId = undefined; this.history = []; this.load(); }
  select(item: EquipmentSummary): void { this.selectedRowId = item.id; this.api.equipmentDetails(item.id).subscribe(x => { if (this.selectedRowId !== x.id) return; this.selected = x; this.loadHistory(); }); }
  openCreate(): void { this.selected = undefined; this.selectedRowId = undefined; this.form.reset({ name: '', inventoryNumber: '', equipmentTypeId: '', departmentId: '', parentEquipmentId: '', serialNumber: '', manufacturer: '', model: '', commissionedOn: '', description: '' }); this.equipmentDialog?.nativeElement.showModal(); }
  openEdit(): void { if (!this.selected) return; const x = this.selected; this.form.reset({ name: x.name, inventoryNumber: x.inventoryNumber ?? '', equipmentTypeId: x.equipmentTypeId ?? '', departmentId: x.departmentId, parentEquipmentId: x.parentEquipmentId ?? '', serialNumber: x.serialNumber ?? '', manufacturer: x.manufacturer ?? '', model: x.model ?? '', commissionedOn: x.commissionedOn ?? '', description: x.description ?? '' }); this.equipmentDialog?.nativeElement.showModal(); }
  copySelected(): void { if (!this.selected) return; const x = this.selected; this.selected = undefined; this.selectedRowId = undefined; this.history = []; this.form.reset({ name: x.name, inventoryNumber: '', equipmentTypeId: x.equipmentTypeId ?? '', departmentId: x.departmentId, parentEquipmentId: x.parentEquipmentId ?? '', serialNumber: '', manufacturer: x.manufacturer ?? '', model: x.model ?? '', commissionedOn: x.commissionedOn ?? '', description: x.description ?? '' }); this.equipmentDialog?.nativeElement.showModal(); }
  save(): void { if (this.form.invalid) return; const v = this.form.getRawValue(); const body = { ...v, inventoryNumber: v.inventoryNumber.trim() || null, equipmentTypeId: v.equipmentTypeId || null, parentEquipmentId: v.parentEquipmentId || null, serialNumber: v.serialNumber.trim() || null, manufacturer: v.manufacturer.trim() || null, model: v.model.trim() || null, commissionedOn: v.commissionedOn || null, description: v.description.trim() || null, rowVersion: this.selected?.rowVersion ?? null }; this.api.saveEquipment(this.selected?.id, body).subscribe(x => { this.selected = x; this.selectedRowId = x.id; this.equipmentDialog?.nativeElement.close(); this.message = this.t.translate('masterData.saved'); this.load(); this.loadReferences(); this.loadHistory(); }); }
  setActive(active: boolean): void { if (!this.selected) return; this.api.setEquipmentActive(this.selected.id, active).subscribe(() => { this.selected = { ...this.selected!, isActive: active }; this.load(); this.loadReferences(); }); }
  openType(type?: EquipmentType): void { this.editingType = type; this.typeForm.reset({ name: type?.name ?? '', description: type?.description ?? '', isActive: type?.isActive ?? true }); this.typeDialog?.nativeElement.showModal(); }
  saveType(): void { if (this.typeForm.invalid) return; const v = this.typeForm.getRawValue(); this.api.saveEquipmentType(this.editingType?.id, { name: v.name.trim(), description: v.description.trim() || null, isActive: v.isActive, rowVersion: this.editingType?.rowVersion ?? null }).subscribe(() => { this.typeDialog?.nativeElement.close(); this.loadReferences(); }); }
  setTypeActive(type: EquipmentType, active: boolean): void { this.api.setEquipmentTypeActive(type.id, active).subscribe(() => this.loadReferences()); }
  openState(event?: EquipmentStateEvent): void { if (!this.selected) return; this.editingState = event; this.stateForm.reset({ state: event?.state ?? '', startedAtLocal: event ? this.toLocalInput(event.startedAtUtc) : this.localNow(), endedAtLocal: event?.endedAtUtc ? this.toLocalInput(event.endedAtUtc) : '', comment: event?.comment ?? '' }); this.stateDialog?.nativeElement.showModal(); }
  changeState(): void { if (!this.selected || this.stateForm.invalid) return; const v = this.stateForm.getRawValue(); const body = { state: v.state, startedAtUtc: new Date(v.startedAtLocal).toISOString(), endedAtUtc: v.endedAtLocal ? new Date(v.endedAtLocal).toISOString() : null, comment: v.comment.trim() || null, rowVersion: this.editingState?.rowVersion ?? null }; const request = this.editingState ? this.api.updateEquipmentState(this.selected.id, this.editingState.id, body) : this.api.changeEquipmentState(this.selected.id, body); request.subscribe(() => { this.stateDialog?.nativeElement.close(); this.editingState = undefined; this.api.equipmentDetails(this.selected!.id).subscribe(x => this.selected = x); this.loadHistory(); this.load(); }); }
  loadHistory(): void { if (!this.selected) { this.history = []; return; } this.api.equipmentStates(this.selected.id).subscribe(x => this.history = x); }
  label(name: string, inventoryNumber: string | null): string { return inventoryNumber ? `${name}, ${this.t.translate('equipment.inventoryShort')} ${inventoryNumber}` : name; }
  isPlanned(event: EquipmentStateEvent): boolean { return new Date(event.startedAtUtc).getTime() > Date.now(); }
  private compareItems(a: EquipmentSummary, b: EquipmentSummary): number { const result = String(this.sortValue(a)).localeCompare(String(this.sortValue(b)), undefined, { numeric: true, sensitivity: 'base' }); return (this.sortDirection === 'asc' ? result : -result) || a.name.localeCompare(b.name); }
  private sortValue(item: EquipmentSummary): string | number { if (this.sortKey === 'equipmentType') return item.typeName ?? ''; if (this.sortKey === 'isActive') return Number(item.isActive); return item[this.sortKey] ?? ''; }
  private localNow(): string { const d = new Date(); d.setSeconds(0, 0); return new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString().slice(0, 16); }
  private toLocalInput(value: string): string { const d = new Date(value); return new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString().slice(0, 16); }
}
