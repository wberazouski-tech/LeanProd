import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { DepartmentDetails, DepartmentSummary, OptionItem } from '../master-data.models';
import { MasterDataService } from '../master-data.service';
import { AddressListComponent } from '../addresses/address-list.component';

@Component({ selector: 'app-departments', standalone: true, imports: [CommonModule, ReactiveFormsModule, TranslocoPipe, AddressListComponent], templateUrl: './departments.component.html', styleUrl: '../master-data.css' })
export class DepartmentsComponent implements OnInit {
  private readonly api = inject(MasterDataService); private readonly fb = inject(FormBuilder); private readonly t = inject(TranslocoService); private readonly auth = inject(AuthService);
  items: (DepartmentSummary & { depth: number })[] = []; options: OptionItem[] = []; selected?: DepartmentDetails; total = 0; message = ''; editorOpen = false;
  readonly canManage = this.auth.hasPermission(Permissions.masterDataManage);
  readonly filters = this.fb.nonNullable.group({ search: '', isActive: '' });
  readonly form = this.fb.nonNullable.group({ code: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(4)]], name: ['', Validators.required], description: '', parentDepartmentId: '' });
  ngOnInit(): void { this.load(); this.loadOptions(); }
  load(): void { const f = this.filters.getRawValue(); this.api.departments(1, f.search.trim(), f.isActive, 5000).subscribe(x => { this.items = this.asTree(x.items); this.total = x.totalCount; }); }
  loadOptions(): void { this.api.departmentOptions().subscribe(x => this.options = x); }
  select(item: DepartmentSummary): void { this.loadDepartment(item.id, false); }
  edit(item: DepartmentSummary, event: Event): void { event.stopPropagation(); this.loadDepartment(item.id, true); }
  create(): void { this.selected = undefined; this.form.reset({ code: '', name: '', description: '', parentDepartmentId: '' }); this.editorOpen = true; }
  closeEditor(): void { this.editorOpen = false; }
  save(): void { if (this.form.invalid) return; const v = this.form.getRawValue(); const body = { ...v, parentDepartmentId: v.parentDepartmentId || null, rowVersion: this.selected?.rowVersion ?? null }; this.api.saveDepartment(this.selected?.id, body).subscribe(x => { this.selected = x; this.message = this.t.translate('masterData.saved'); this.editorOpen = false; this.load(); this.loadOptions(); }); }
  setActive(active: boolean): void { if (!this.selected) return; this.api.setDepartmentActive(this.selected.id, active).subscribe(() => { this.message = this.t.translate(active ? 'masterData.activated' : 'masterData.deactivated'); this.load(); this.loadOptions(); this.selected = { ...this.selected!, isActive: active }; }); }
  private asTree(source: DepartmentSummary[]): (DepartmentSummary & { depth: number })[] { const result: (DepartmentSummary & { depth: number })[] = []; const ids = new Set(source.map(x => x.id)); const children = new Map<string | null, DepartmentSummary[]>(); for (const item of source) { const parent = item.parentDepartmentId && ids.has(item.parentDepartmentId) ? item.parentDepartmentId : null; children.set(parent, [...(children.get(parent) ?? []), item]); } const add = (parent: string | null, depth: number): void => { for (const item of (children.get(parent) ?? []).sort((a, b) => a.code.localeCompare(b.code))) { result.push({ ...item, depth }); add(item.id, depth + 1); } }; add(null, 0); return result; }
  private loadDepartment(id: string, openEditor: boolean): void { this.api.department(id).subscribe(x => { this.selected = x; this.form.reset({ code: x.code, name: x.name, description: x.description ?? '', parentDepartmentId: x.parentDepartmentId ?? '' }); this.editorOpen = openEditor && this.canManage; }); }
}
