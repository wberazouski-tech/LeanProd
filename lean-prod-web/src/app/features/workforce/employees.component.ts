import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../core/auth/auth.service';
import { Permissions } from '../../core/auth/permissions';
import { OptionItem } from '../master-data/master-data.models';
import { MasterDataService } from '../master-data/master-data.service';
import { BrigadeMembership, EmployeeDetails, EmployeeSummary } from './workforce.models';
import { WorkforceService } from './workforce.service';
import { PageState, PageStateComponent } from '../../core/ui/page-state.component';
import { FormFieldErrorComponent } from '../../core/ui/form-field-error.component';
import { ModalShellComponent } from '../../core/ui/modal-shell.component';
import { ColumnResizeDirective } from '../../core/ui/column-resize.directive';
import { TableActionsComponent } from '../../core/ui/table-actions.component';

@Component({ selector: 'app-employees', standalone: true, imports: [CommonModule, ReactiveFormsModule, TranslocoPipe, PageStateComponent, FormFieldErrorComponent, ModalShellComponent, ColumnResizeDirective, TableActionsComponent], templateUrl: './employees.component.html', styleUrls: ['../master-data/master-data.css'] })
export class EmployeesComponent implements OnInit {
  private readonly api = inject(WorkforceService); private readonly refs = inject(MasterDataService); private readonly fb = inject(FormBuilder); private readonly auth = inject(AuthService); private readonly t = inject(TranslocoService);
  readonly canManage = this.auth.hasPermission(Permissions.workforceManage); items: EmployeeSummary[] = []; departments: OptionItem[] = []; selected?: EmployeeDetails; history: BrigadeMembership[] = []; total = 0; error = ''; editorOpen = false; saving = false; state: PageState = 'loading';
  sortKey: 'personnelNumber' | 'fullName' | 'position' | 'departmentName' | 'activeBrigadeCount' | 'isActive' = 'personnelNumber'; sortDirection: 'asc' | 'desc' = 'asc';
  columnWidths: Record<string, number> = { personnelNumber: 150, fullName: 260, position: 190, departmentName: 210, activeBrigadeCount: 140, isActive: 130, actions: 58 };
  readonly filters = this.fb.nonNullable.group({ search: '', isActive: '' });
  readonly form = this.fb.nonNullable.group({ personnelNumber: ['', Validators.required], lastName: ['', Validators.required], firstName: ['', Validators.required], middleName: '', position: '', departmentId: '' });
  ngOnInit(): void { this.load(); this.refs.departmentOptions().subscribe(x => this.departments = x); }
  load(): void { this.state = 'loading'; const f = this.filters.getRawValue(); this.api.employees(f.search, f.isActive).subscribe({ next: x => { this.items = x.items; this.total = x.totalCount; this.state = x.items.length ? 'ready' : 'empty'; }, error: () => { this.error = this.t.translate('workforce.errors.loadEmployees'); this.state = 'error'; } }); }
  get sortedItems(): EmployeeSummary[] { return [...this.items].sort((a, b) => { const left = a[this.sortKey] ?? ''; const right = b[this.sortKey] ?? ''; const result = typeof left === 'string' ? left.localeCompare(String(right)) : Number(left) - Number(right); return this.sortDirection === 'asc' ? result : -result; }); }
  sortBy(key: typeof this.sortKey): void { if (this.sortKey === key) this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc'; else { this.sortKey = key; this.sortDirection = 'asc'; } }
  select(id: string, openEditor = false): void { this.api.employee(id).subscribe(x => { this.selected = x; this.form.reset({ personnelNumber: x.personnelNumber, lastName: x.lastName, firstName: x.firstName, middleName: x.middleName ?? '', position: x.position ?? '', departmentId: x.departmentId ?? '' }); this.api.employeeHistory(id).subscribe(h => this.history = h); this.editorOpen = openEditor && this.canManage; }); }
  edit(id: string, event: Event): void { event.stopPropagation(); this.select(id, true); }
  editSelected(): void { if (this.selected) this.select(this.selected.id, true); }
  copySelected(): void { if (!this.selected) return; const x = this.selected; this.selected = undefined; this.history = []; this.form.reset({ personnelNumber: '', lastName: x.lastName, firstName: x.firstName, middleName: x.middleName ?? '', position: x.position ?? '', departmentId: x.departmentId ?? '' }); this.editorOpen = true; }
  create(): void { this.selected = undefined; this.history = []; this.form.reset({ personnelNumber: '', lastName: '', firstName: '', middleName: '', position: '', departmentId: '' }); this.editorOpen = true; }
  closeEditor(): void { this.editorOpen = false; }
  save(): void { if (this.form.invalid) { this.form.markAllAsTouched(); return; } this.saving = true; this.state = 'saving'; const v = this.form.getRawValue(); const body = { ...v, middleName: v.middleName.trim() || null, position: v.position.trim() || null, departmentId: v.departmentId || null, rowVersion: this.selected?.rowVersion ?? null }; this.api.saveEmployee(this.selected?.id, body).subscribe({ next: x => { this.saving = false; this.editorOpen = false; this.state = 'success'; this.select(x.id); this.load(); }, error: () => { this.saving = false; this.error = this.t.translate('workforce.errors.saveEmployee'); this.state = 'error'; } }); }
  setActive(active: boolean): void { if (!this.selected) return; this.api.setEmployeeActive(this.selected.id, active).subscribe(() => { this.select(this.selected!.id); this.load(); }); }
}
