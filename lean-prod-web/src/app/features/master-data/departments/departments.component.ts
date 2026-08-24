import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { DepartmentDetails, DepartmentSummary, OptionItem } from '../master-data.models';
import { MasterDataService } from '../master-data.service';
import { AddressListComponent } from '../addresses/address-list.component';
import { PageState, PageStateComponent } from '../../../core/ui/page-state.component';

@Component({ selector: 'app-departments', standalone: true, imports: [CommonModule, ReactiveFormsModule, TranslocoPipe, AddressListComponent, PageStateComponent], templateUrl: './departments.component.html', styleUrl: '../master-data.css' })
export class DepartmentsComponent implements OnInit {
  private readonly api = inject(MasterDataService); private readonly fb = inject(FormBuilder); private readonly t = inject(TranslocoService); private readonly auth = inject(AuthService);
  items: (DepartmentSummary & { depth: number })[] = []; options: OptionItem[] = []; selected?: DepartmentDetails; total = 0; message = ''; editorOpen = false; state: PageState = 'loading';
  readonly expandedIds = new Set<string>();
  sortKey: 'code' | 'name' | 'isActive' = 'code'; sortDirection: 'asc' | 'desc' = 'asc'; resizingColumn?: 'code' | 'name' | 'status' | 'actions'; resizeStartX = 0; resizeStartWidth = 0;
  columnWidths: Record<'code' | 'name' | 'status' | 'actions', number> = { code: 150, name: 360, status: 150, actions: 58 };
  readonly canManage = this.auth.hasPermission(Permissions.masterDataManage);
  readonly filters = this.fb.nonNullable.group({ search: '', isActive: '' });
  readonly form = this.fb.nonNullable.group({ code: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(4)]], name: ['', Validators.required], description: '', parentDepartmentId: '' });
  ngOnInit(): void { this.load(); this.loadOptions(); }
  load(): void { this.state = 'loading'; const f = this.filters.getRawValue(); this.api.departments(1, f.search.trim(), f.isActive, 5000).subscribe({ next: x => { this.items = this.asTree(x.items); this.total = x.totalCount; this.state = x.items.length ? 'ready' : 'empty'; }, error: () => this.state = 'error' }); }
  loadOptions(): void { this.api.departmentOptions().subscribe(x => this.options = x); }
  get sortedItems(): (DepartmentSummary & { depth: number })[] {
    const ids = new Set(this.items.map(item => item.id));
    const children = new Map<string | null, (DepartmentSummary & { depth: number })[]>();
    for (const item of this.items) {
      const parentId = item.parentDepartmentId && ids.has(item.parentDepartmentId) ? item.parentDepartmentId : null;
      children.set(parentId, [...(children.get(parentId) ?? []), item]);
    }
    const result: (DepartmentSummary & { depth: number })[] = [];
    const addBranch = (parentId: string | null): void => {
      const siblings = [...(children.get(parentId) ?? [])].sort((a, b) => this.compareItems(a, b));
      for (const item of siblings) {
        result.push(item);
        if (this.expandedIds.has(item.id)) addBranch(item.id);
      }
    };
    addBranch(null);
    return result;
  }
  hasChildren(item: DepartmentSummary): boolean { return this.items.some(child => child.parentDepartmentId === item.id); }
  isExpanded(item: DepartmentSummary): boolean { return this.expandedIds.has(item.id); }
  toggleExpanded(item: DepartmentSummary, event: Event): void { event.stopPropagation(); if (this.expandedIds.has(item.id)) this.expandedIds.delete(item.id); else this.expandedIds.add(item.id); }
  sortBy(key: 'code' | 'name' | 'isActive'): void { if (this.sortKey === key) this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc'; else { this.sortKey = key; this.sortDirection = 'asc'; } }
  private compareItems(a: DepartmentSummary, b: DepartmentSummary): number { const left = this.sortKey === 'isActive' ? Number(a.isActive) : a[this.sortKey].toLocaleLowerCase(); const right = this.sortKey === 'isActive' ? Number(b.isActive) : b[this.sortKey].toLocaleLowerCase(); const result = left < right ? -1 : left > right ? 1 : 0; return (this.sortDirection === 'asc' ? result : -result) || a.code.localeCompare(b.code); }
  startResize(column: 'code' | 'name' | 'status' | 'actions', event: MouseEvent): void { event.preventDefault(); event.stopPropagation(); this.resizingColumn = column; this.resizeStartX = event.clientX; this.resizeStartWidth = this.columnWidths[column]; }
  @HostListener('document:mousemove', ['$event']) resize(event: MouseEvent): void { if (!this.resizingColumn) return; this.columnWidths[this.resizingColumn] = Math.max(58, this.resizeStartWidth + event.clientX - this.resizeStartX); }
  @HostListener('document:mouseup') stopResize(): void { this.resizingColumn = undefined; }
  select(item: DepartmentSummary): void { this.loadDepartment(item.id, false); }
  edit(item: DepartmentSummary, event: Event): void { event.stopPropagation(); this.loadDepartment(item.id, true); }
  create(): void { this.selected = undefined; this.form.reset({ code: '', name: '', description: '', parentDepartmentId: '' }); this.editorOpen = true; }
  closeEditor(): void { this.editorOpen = false; }
  save(): void { if (this.form.invalid) return; this.state = 'saving'; const v = this.form.getRawValue(); const body = { ...v, parentDepartmentId: v.parentDepartmentId || null, rowVersion: this.selected?.rowVersion ?? null }; this.api.saveDepartment(this.selected?.id, body).subscribe({ next: x => { this.selected = x; this.message = this.t.translate('masterData.saved'); this.editorOpen = false; this.state = 'success'; this.load(); this.loadOptions(); }, error: () => this.state = 'error' }); }
  setActive(active: boolean): void { if (!this.selected) return; this.api.setDepartmentActive(this.selected.id, active).subscribe(() => { this.message = this.t.translate(active ? 'masterData.activated' : 'masterData.deactivated'); this.load(); this.loadOptions(); this.selected = { ...this.selected!, isActive: active }; }); }
  private asTree(source: DepartmentSummary[]): (DepartmentSummary & { depth: number })[] { const result: (DepartmentSummary & { depth: number })[] = []; const ids = new Set(source.map(x => x.id)); const children = new Map<string | null, DepartmentSummary[]>(); for (const item of source) { const parent = item.parentDepartmentId && ids.has(item.parentDepartmentId) ? item.parentDepartmentId : null; children.set(parent, [...(children.get(parent) ?? []), item]); } const add = (parent: string | null, depth: number): void => { for (const item of (children.get(parent) ?? []).sort((a, b) => a.code.localeCompare(b.code))) { result.push({ ...item, depth }); add(item.id, depth + 1); } }; add(null, 0); return result; }
  private loadDepartment(id: string, openEditor: boolean): void { this.api.department(id).subscribe(x => { this.selected = x; this.form.reset({ code: x.code, name: x.name, description: x.description ?? '', parentDepartmentId: x.parentDepartmentId ?? '' }); this.editorOpen = openEditor && this.canManage; }); }
}
