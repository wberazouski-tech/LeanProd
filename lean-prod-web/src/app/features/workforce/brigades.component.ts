import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../core/auth/auth.service';
import { Permissions } from '../../core/auth/permissions';
import { OptionItem } from '../master-data/master-data.models';
import { MasterDataService } from '../master-data/master-data.service';
import { BrigadeDetails, BrigadeMembership, BrigadeSummary, EmployeeOption } from './workforce.models';
import { WorkforceService } from './workforce.service';
import { PageState, PageStateComponent } from '../../core/ui/page-state.component';
import { FormFieldErrorComponent } from '../../core/ui/form-field-error.component';
import { ModalShellComponent } from '../../core/ui/modal-shell.component';
import { ColumnResizeDirective } from '../../core/ui/column-resize.directive';
import { TableActionsComponent } from '../../core/ui/table-actions.component';

@Component({ selector: 'app-brigades', standalone: true, imports: [CommonModule, FormsModule, ReactiveFormsModule, TranslocoPipe, PageStateComponent, FormFieldErrorComponent, ModalShellComponent, ColumnResizeDirective, TableActionsComponent], templateUrl: './brigades.component.html', styleUrls: ['../master-data/master-data.css'] })
export class BrigadesComponent implements OnInit {
  private readonly api = inject(WorkforceService); private readonly refs = inject(MasterDataService); private readonly fb = inject(FormBuilder); private readonly auth = inject(AuthService); private readonly t = inject(TranslocoService);
  readonly canManage = this.auth.hasPermission(Permissions.workforceManage); items: BrigadeSummary[] = []; departments: OptionItem[] = []; employees: EmployeeOption[] = []; selected?: BrigadeDetails; selectedMembership?: BrigadeMembership; memberships: BrigadeMembership[] = []; history = false; total = 0; error = ''; editorOpen = false; ktuEditorOpen = false; saving = false; membershipSaving = false; ktuSaving = false; state: PageState = 'loading';
  sortKey: 'code' | 'name' | 'departmentName' | 'activeMemberCount' | 'isActive' = 'code'; sortDirection: 'asc' | 'desc' = 'asc';
  columnWidths: Record<string, number> = { code: 140, name: 280, departmentName: 220, activeMemberCount: 160, isActive: 130, actions: 58 };
  readonly filters = this.fb.nonNullable.group({ search: '', isActive: '' }); readonly form = this.fb.nonNullable.group({ code: '', name: ['', Validators.required], description: '', departmentId: '' });
  readonly memberForm = this.fb.nonNullable.group({ employeeId: ['', Validators.required], startedAtLocal: [this.localNow(), Validators.required], endedAtLocal: '', coefficient: [1, [Validators.required, Validators.min(0.0001), Validators.max(10)]] });
  readonly ktuForm = this.fb.nonNullable.group({ effectiveFromLocal: [this.localNow(), Validators.required], coefficient: [1, [Validators.required, Validators.min(0.0001), Validators.max(10)]] });
  ngOnInit(): void { this.load(); this.refs.departmentOptions().subscribe(x => this.departments = x); this.api.employeeOptions().subscribe(x => this.employees = x); }
  load(): void { this.state = 'loading'; const f = this.filters.getRawValue(); this.api.brigades(f.search, f.isActive).subscribe({ next: x => { this.items = x.items; this.total = x.totalCount; this.state = x.items.length ? 'ready' : 'empty'; }, error: () => { this.error = this.t.translate('workforce.errors.loadBrigades'); this.state = 'error'; } }); }
  get sortedItems(): BrigadeSummary[] { return [...this.items].sort((a, b) => { const left = a[this.sortKey] ?? ''; const right = b[this.sortKey] ?? ''; const result = typeof left === 'string' ? left.localeCompare(String(right)) : Number(left) - Number(right); return this.sortDirection === 'asc' ? result : -result; }); }
  sortBy(key: typeof this.sortKey): void { if (this.sortKey === key) this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc'; else { this.sortKey = key; this.sortDirection = 'asc'; } }
  select(id: string, openEditor = false): void { this.api.brigade(id).subscribe(x => { this.selected = x; this.form.reset({ code: x.code, name: x.name, description: x.description ?? '', departmentId: x.departmentId ?? '' }); this.loadMemberships(); this.editorOpen = openEditor && this.canManage; }); }
  edit(id: string, event: Event): void { event.stopPropagation(); this.select(id, true); }
  editSelected(): void { if (this.selected) this.select(this.selected.id, true); }
  copySelected(): void { if (!this.selected) return; const x = this.selected; this.selected = undefined; this.memberships = []; this.form.reset({ code: '', name: x.name, description: x.description ?? '', departmentId: x.departmentId ?? '' }); this.editorOpen = true; }
  create(): void { this.selected = undefined; this.memberships = []; this.form.reset({ code: '', name: '', description: '', departmentId: '' }); this.editorOpen = true; }
  closeEditor(): void { this.editorOpen = false; }
  save(): void { if (this.form.invalid) { this.form.markAllAsTouched(); return; } this.saving = true; this.state = 'saving'; const v = this.form.getRawValue(); const body = { ...v, description: v.description.trim() || null, departmentId: v.departmentId || null, rowVersion: this.selected?.rowVersion ?? null }; this.api.saveBrigade(this.selected?.id, body).subscribe({ next: x => { this.saving = false; this.editorOpen = false; this.state = 'success'; this.select(x.id); this.load(); }, error: () => { this.saving = false; this.error = this.t.translate('workforce.errors.saveBrigade'); this.state = 'error'; } }); }
  setActive(active: boolean): void { if (!this.selected) return; this.api.setBrigadeActive(this.selected.id, active).subscribe(() => { this.select(this.selected!.id); this.load(); }); }
  loadMemberships(): void { if (this.selected) this.api.memberships(this.selected.id, this.history).subscribe(x => this.memberships = x); }
  addMember(): void { if (!this.selected || this.memberForm.invalid) { this.memberForm.markAllAsTouched(); return; } this.membershipSaving = true; const v = this.memberForm.getRawValue(); this.api.addMembership(this.selected.id, { employeeId: v.employeeId, startedAtUtc: new Date(v.startedAtLocal).toISOString(), endedAtUtc: v.endedAtLocal ? new Date(v.endedAtLocal).toISOString() : null, laborParticipationCoefficient: v.coefficient }).subscribe({ next: () => { this.membershipSaving = false; this.loadMemberships(); this.memberForm.reset({ employeeId: '', startedAtLocal: this.localNow(), endedAtLocal: '', coefficient: 1 }); this.load(); }, error: () => { this.membershipSaving = false; this.error = this.t.translate('workforce.errors.invalidMembership'); } }); }
  close(x: BrigadeMembership): void { if (!this.selected) return; this.api.closeMembership(this.selected.id, x.id, { endedAtUtc: new Date().toISOString(), rowVersion: x.rowVersion }).subscribe(() => { this.loadMemberships(); this.load(); }); }
  openKtuEditor(x: BrigadeMembership): void { this.selectedMembership = x; this.ktuForm.reset({ effectiveFromLocal: this.localNow(), coefficient: x.laborParticipationCoefficient }); this.ktuEditorOpen = true; }
  closeKtuEditor(): void { this.ktuEditorOpen = false; this.selectedMembership = undefined; }
  changeKtu(): void { if (!this.selected || !this.selectedMembership || this.ktuForm.invalid) { this.ktuForm.markAllAsTouched(); return; } this.ktuSaving = true; const v = this.ktuForm.getRawValue(); this.api.changeCoefficient(this.selected.id, this.selectedMembership.id, { effectiveFromUtc: new Date(v.effectiveFromLocal).toISOString(), laborParticipationCoefficient: v.coefficient, rowVersion: this.selectedMembership.rowVersion }).subscribe({ next: () => { this.ktuSaving = false; this.closeKtuEditor(); this.loadMemberships(); }, error: () => { this.ktuSaving = false; this.error = this.t.translate('workforce.errors.changeKtu'); } }); }
  private localNow(): string { const d = new Date(); d.setSeconds(0, 0); return new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString().slice(0, 16); }
}
