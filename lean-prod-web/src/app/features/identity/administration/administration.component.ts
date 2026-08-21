import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { UserDetails, UserSummary } from './user-administration.models';
import { UserAdministrationService } from './user-administration.service';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { MasterDataService } from '../../master-data/master-data.service';
import { OptionItem, StorageOption } from '../../master-data/master-data.models';

@Component({ selector: 'app-administration', standalone: true, imports: [CommonModule, ReactiveFormsModule, TranslocoPipe], templateUrl: './administration.component.html', styleUrl: './administration.component.css' })
export class AdministrationComponent implements OnInit {
  private readonly api = inject(UserAdministrationService);
  private readonly fb = inject(FormBuilder);
  private readonly translate = inject(TranslocoService);
  private readonly masterData = inject(MasterDataService);
  users: UserSummary[] = []; roles: { name: string }[] = []; selected?: UserDetails;
  departments: OptionItem[] = []; storageLocations: StorageOption[] = [];
  page = 1; totalCount = 0; loading = false; message = '';
  readonly filters = this.fb.nonNullable.group({ search: '', isActive: '', role: '' });
  readonly form = this.fb.nonNullable.group({ email: ['', [Validators.required, Validators.email]], displayName: ['', Validators.required], temporaryPassword: ['', Validators.minLength(12)], preferredLanguage: 'be', defaultDepartmentId: '', defaultStorageLocationId: '', roles: this.fb.nonNullable.control<string[]>([]) });
  readonly passwordForm = this.fb.nonNullable.group({ temporaryPassword: ['', [Validators.required, Validators.minLength(12)]] });

  ngOnInit(): void { forkJoin({ roles: this.api.roles(), users: this.api.list(1, '', '', ''), departments: this.masterData.departmentOptions(), storageLocations: this.masterData.storageOptions() }).subscribe(x => { this.roles = x.roles; this.departments = x.departments; this.storageLocations = x.storageLocations; this.applyPage(x.users); }); }
  load(page = 1): void { this.loading = true; const v = this.filters.getRawValue(); this.api.list(page, v.search.trim(), v.isActive, v.role).subscribe({ next: x => { this.applyPage(x); this.loading = false; }, error: () => this.loading = false }); }
  select(user: UserSummary): void { this.api.get(user.id).subscribe(x => { this.selected = x; this.form.reset({ email: x.email, displayName: x.displayName, temporaryPassword: '', preferredLanguage: x.preferredLanguage, defaultDepartmentId: x.defaultDepartmentId ?? '', defaultStorageLocationId: x.defaultStorageLocationId ?? '', roles: [...x.roles] }); }); }
  newUser(): void { this.selected = undefined; this.form.reset({ email: '', displayName: '', temporaryPassword: '', preferredLanguage: 'be', defaultDepartmentId: '', defaultStorageLocationId: '', roles: ['Viewer'] }); }
  toggleRole(role: string, checked: boolean): void { const roles = this.form.controls.roles.value.filter(x => x !== role); this.form.controls.roles.setValue(checked ? [...roles, role] : roles); }
  roleLabel(role: string): string { return this.translate.translate(`roles.${role}`); }
  roleList(roles: string[]): string { return roles.map(role => this.roleLabel(role)).join(', '); }
  save(): void {
    if (this.form.invalid || (!this.selected && this.form.controls.temporaryPassword.value.length < 12)) return;
    const value = this.form.getRawValue();
    const payload = { ...value, defaultDepartmentId: value.defaultDepartmentId || null, defaultStorageLocationId: value.defaultStorageLocationId || null };
    const request = this.selected ? this.api.update(this.selected.id, { email: payload.email, displayName: payload.displayName, preferredLanguage: payload.preferredLanguage, defaultDepartmentId: payload.defaultDepartmentId, defaultStorageLocationId: payload.defaultStorageLocationId, concurrencyStamp: this.selected.concurrencyStamp }) : this.api.create(payload);
    request.subscribe(user => this.selected ? this.api.setRoles(user.id, value.roles, user.concurrencyStamp).subscribe(x => this.saved(x)) : this.saved(user));
  }
  setActive(active: boolean): void { if (!this.selected) return; this.api.setActive(this.selected.id, active).subscribe(() => { this.message = this.translate.translate(active ? 'admin.activated' : 'admin.deactivated'); this.load(this.page); this.select({ ...this.selected!, isActive: active }); }); }
  resetPassword(): void { if (!this.selected || this.passwordForm.invalid) return; this.api.resetPassword(this.selected.id, this.passwordForm.controls.temporaryPassword.value).subscribe(() => { this.passwordForm.reset(); this.message = this.translate.translate('admin.passwordReset'); }); }
  private saved(user: UserDetails): void { this.selected = user; this.message = this.translate.translate('admin.saved'); this.load(this.page); }
  private applyPage(result: { items: UserSummary[]; page: number; totalCount: number }): void { this.users = result.items; this.page = result.page; this.totalCount = result.totalCount; }
}
