import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { EquipmentDetails, EquipmentOption, EquipmentStateEvent, EquipmentSummary, EquipmentType, OptionItem } from '../master-data.models';
import { MasterDataService } from '../master-data.service';

@Component({ selector: 'app-equipment', standalone: true, imports: [CommonModule, ReactiveFormsModule, TranslocoPipe], templateUrl: './equipment.component.html', styleUrls: ['../master-data.css', './equipment.component.css'] })
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
  editingType?: EquipmentType; editingState?: EquipmentStateEvent; page = 1; total = 0; message = '';
  readonly filters = this.fb.nonNullable.group({ search: '', isActive: '', departmentId: '', equipmentTypeId: '', state: '' });
  readonly form = this.fb.nonNullable.group({ name: ['', Validators.required], inventoryNumber: '', equipmentTypeId: '', departmentId: ['', Validators.required], parentEquipmentId: '', serialNumber: '', manufacturer: '', model: '', commissionedOn: '', description: '' });
  readonly typeForm = this.fb.nonNullable.group({ name: ['', Validators.required], description: '' });
  readonly stateForm = this.fb.nonNullable.group({ state: ['', Validators.required], startedAtLocal: [this.localNow(), Validators.required], endedAtLocal: '', comment: '' });

  ngOnInit(): void { this.load(); this.loadReferences(); }
  load(page = 1): void { const f = this.filters.getRawValue(); this.api.equipment(page, f).subscribe(x => { this.items = x.items; this.page = x.page; this.total = x.totalCount; }); }
  loadReferences(): void { this.api.departmentOptions().subscribe(x => this.departments = x); this.api.equipmentOptions().subscribe(x => this.options = x); this.api.equipmentTypes().subscribe(x => this.types = x); }
  select(item: EquipmentSummary): void { this.api.equipmentDetails(item.id).subscribe(x => { this.selected = x; this.loadHistory(); }); }
  openCreate(): void { this.selected = undefined; this.form.reset({ name: '', inventoryNumber: '', equipmentTypeId: '', departmentId: '', parentEquipmentId: '', serialNumber: '', manufacturer: '', model: '', commissionedOn: '', description: '' }); this.equipmentDialog?.nativeElement.showModal(); }
  openEdit(): void { if (!this.selected) return; const x = this.selected; this.form.reset({ name: x.name, inventoryNumber: x.inventoryNumber ?? '', equipmentTypeId: x.equipmentTypeId ?? '', departmentId: x.departmentId, parentEquipmentId: x.parentEquipmentId ?? '', serialNumber: x.serialNumber ?? '', manufacturer: x.manufacturer ?? '', model: x.model ?? '', commissionedOn: x.commissionedOn ?? '', description: x.description ?? '' }); this.equipmentDialog?.nativeElement.showModal(); }
  save(): void { if (this.form.invalid) return; const v = this.form.getRawValue(); const body = { ...v, inventoryNumber: v.inventoryNumber.trim() || null, equipmentTypeId: v.equipmentTypeId || null, parentEquipmentId: v.parentEquipmentId || null, serialNumber: v.serialNumber.trim() || null, manufacturer: v.manufacturer.trim() || null, model: v.model.trim() || null, commissionedOn: v.commissionedOn || null, description: v.description.trim() || null, rowVersion: this.selected?.rowVersion ?? null }; this.api.saveEquipment(this.selected?.id, body).subscribe(x => { this.selected = x; this.equipmentDialog?.nativeElement.close(); this.message = this.t.translate('masterData.saved'); this.load(this.page); this.loadReferences(); this.loadHistory(); }); }
  setActive(active: boolean): void { if (!this.selected) return; this.api.setEquipmentActive(this.selected.id, active).subscribe(() => { this.selected = { ...this.selected!, isActive: active }; this.load(this.page); this.loadReferences(); }); }
  openType(type?: EquipmentType): void { this.editingType = type; this.typeForm.reset({ name: type?.name ?? '', description: type?.description ?? '' }); this.typeDialog?.nativeElement.showModal(); }
  saveType(): void { if (this.typeForm.invalid) return; const v = this.typeForm.getRawValue(); this.api.saveEquipmentType(this.editingType?.id, { name: v.name.trim(), description: v.description.trim() || null, rowVersion: this.editingType?.rowVersion ?? null }).subscribe(() => { this.typeDialog?.nativeElement.close(); this.loadReferences(); }); }
  setTypeActive(type: EquipmentType, active: boolean): void { this.api.setEquipmentTypeActive(type.id, active).subscribe(() => this.loadReferences()); }
  openState(event?: EquipmentStateEvent): void { if (!this.selected) return; this.editingState = event; this.stateForm.reset({ state: event?.state ?? '', startedAtLocal: event ? this.toLocalInput(event.startedAtUtc) : this.localNow(), endedAtLocal: event?.endedAtUtc ? this.toLocalInput(event.endedAtUtc) : '', comment: event?.comment ?? '' }); this.stateDialog?.nativeElement.showModal(); }
  changeState(): void { if (!this.selected || this.stateForm.invalid) return; const v = this.stateForm.getRawValue(); const body = { state: v.state, startedAtUtc: new Date(v.startedAtLocal).toISOString(), endedAtUtc: v.endedAtLocal ? new Date(v.endedAtLocal).toISOString() : null, comment: v.comment.trim() || null, rowVersion: this.editingState?.rowVersion ?? null }; const request = this.editingState ? this.api.updateEquipmentState(this.selected.id, this.editingState.id, body) : this.api.changeEquipmentState(this.selected.id, body); request.subscribe(() => { this.stateDialog?.nativeElement.close(); this.editingState = undefined; this.api.equipmentDetails(this.selected!.id).subscribe(x => this.selected = x); this.loadHistory(); this.load(this.page); }); }
  loadHistory(): void { if (!this.selected) { this.history = []; return; } this.api.equipmentStates(this.selected.id).subscribe(x => this.history = x); }
  label(name: string, inventoryNumber: string | null): string { return inventoryNumber ? `${name}, ${this.t.translate('equipment.inventoryShort')} ${inventoryNumber}` : name; }
  isPlanned(event: EquipmentStateEvent): boolean { return new Date(event.startedAtUtc).getTime() > Date.now(); }
  private localNow(): string { const d = new Date(); d.setSeconds(0, 0); return new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString().slice(0, 16); }
  private toLocalInput(value: string): string { const d = new Date(value); return new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString().slice(0, 16); }
}
