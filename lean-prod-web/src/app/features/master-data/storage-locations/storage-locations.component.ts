import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { CatalogItem, OptionItem, StorageDetails, StorageSummary } from '../master-data.models';
import { MasterDataService } from '../master-data.service';
import { AddressListComponent } from '../addresses/address-list.component';

@Component({ selector: 'app-storage-locations', standalone: true, imports: [CommonModule, ReactiveFormsModule, TranslocoPipe, AddressListComponent], templateUrl: './storage-locations.component.html', styleUrl: '../master-data.css' })
export class StorageLocationsComponent implements OnInit {
  private readonly api = inject(MasterDataService); private readonly fb = inject(FormBuilder); private readonly t = inject(TranslocoService); private readonly auth = inject(AuthService);
  items: (StorageSummary & { depth: number })[] = []; departments: OptionItem[] = []; parents: OptionItem[] = []; kinds: CatalogItem[] = []; types: CatalogItem[] = []; selected?: StorageDetails; total = 0; message = ''; editorOpen = false;
  readonly canManage = this.auth.hasPermission(Permissions.masterDataManage);
  readonly filters = this.fb.nonNullable.group({ search: '', isActive: '' });
  readonly form = this.fb.nonNullable.group({ code: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(4)]], name: ['', Validators.required], description: '', departmentId: ['', Validators.required], kindId: ['', Validators.required], parentStorageLocationId: '', typeIds: this.fb.nonNullable.control<string[]>([]) });
  ngOnInit(): void { this.load(); this.loadOptions(); }
  load(): void { const f = this.filters.getRawValue(); this.api.storages(1, f.search.trim(), f.isActive, 5000).subscribe(x => { this.items = this.asTree(x.items); this.total = x.totalCount; }); }
  loadOptions(): void { forkJoin({ departments: this.api.departmentOptions(), parents: this.api.storageOptions(), kinds: this.api.kinds(), types: this.api.types() }).subscribe(x => { this.departments = x.departments; this.parents = x.parents; this.kinds = x.kinds; this.types = x.types; }); }
  select(item: StorageSummary): void { this.loadStorage(item.id, false); }
  edit(item: StorageSummary, event: Event): void { event.stopPropagation(); this.loadStorage(item.id, true); }
  create(): void { this.selected = undefined; this.form.reset({ code: '', name: '', description: '', departmentId: '', kindId: '', parentStorageLocationId: '', typeIds: [] }); this.editorOpen = true; }
  closeEditor(): void { this.editorOpen = false; }
  toggleType(id: string, checked: boolean): void { const ids = this.form.controls.typeIds.value.filter(x => x !== id); this.form.controls.typeIds.setValue(checked ? [...ids, id] : ids); }
  save(): void { if (this.form.invalid || this.form.controls.typeIds.value.length === 0) return; const v = this.form.getRawValue(); const body = { ...v, parentStorageLocationId: v.parentStorageLocationId || null, rowVersion: this.selected?.rowVersion ?? null }; this.api.saveStorage(this.selected?.id, body).subscribe(x => { this.selected = x; this.message = this.t.translate('masterData.saved'); this.editorOpen = false; this.load(); this.loadOptions(); }); }
  setActive(active: boolean): void { if (!this.selected) return; this.api.setStorageActive(this.selected.id, active).subscribe(() => { this.message = this.t.translate(active ? 'masterData.activated' : 'masterData.deactivated'); this.load(); this.loadOptions(); this.selected = { ...this.selected!, isActive: active }; }); }
  kind(code: string): string { return this.t.translate(`storageKinds.${code}`); }
  type(code: string): string { return this.t.translate(`storageTypes.${code}`); }
  typeList(codes: string[]): string { return codes.map(x => this.type(x)).join(', '); }
  private asTree(source: StorageSummary[]): (StorageSummary & { depth: number })[] { const result: (StorageSummary & { depth: number })[] = []; const ids = new Set(source.map(x => x.id)); const children = new Map<string | null, StorageSummary[]>(); for (const item of source) { const parent = item.parentStorageLocationId && ids.has(item.parentStorageLocationId) ? item.parentStorageLocationId : null; children.set(parent, [...(children.get(parent) ?? []), item]); } const add = (parent: string | null, depth: number): void => { for (const item of (children.get(parent) ?? []).sort((a, b) => a.code.localeCompare(b.code))) { result.push({ ...item, depth }); add(item.id, depth + 1); } }; add(null, 0); return result; }
  private loadStorage(id: string, openEditor: boolean): void { this.api.storage(id).subscribe(x => { this.selected = x; this.form.reset({ code: x.code, name: x.name, description: x.description ?? '', departmentId: x.departmentId, kindId: x.kindId, parentStorageLocationId: x.parentStorageLocationId ?? '', typeIds: [...x.typeIds] }); this.editorOpen = openEditor && this.canManage; }); }
}
