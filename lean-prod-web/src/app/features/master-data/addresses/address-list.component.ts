import { CommonModule } from '@angular/common';
import { Component, DestroyRef, Input, OnChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { Subject, catchError, of, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AddressDetails } from '../master-data.models';
import { MasterDataService } from '../master-data.service';
import { MasterDataUiModule } from '../shared/master-data-ui.module';
import { PageState } from '../../../core/ui/page-state.component';

@Component({ selector: 'app-address-list', standalone: true, imports: [CommonModule, ReactiveFormsModule, TranslocoPipe, MasterDataUiModule], templateUrl: './address-list.component.html', styleUrl: '../master-data.css' })
export class AddressListComponent implements OnChanges {
  @Input({ required: true }) owner!: 'organization' | 'department' | 'storage-location';
  @Input() ownerId?: string;
  @Input() canManage = false;
  @Input() compact = false;
  readonly requests = inject(MasterDataService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly reload = new Subject<void>();
  private ownerKey = '';
  items: AddressDetails[] = [];
  editing?: AddressDetails;
  showEditor = false;
  state: PageState = 'loading';
  readonly types = ['Registered', 'Correspondence', 'Office', 'Delivery', 'Other'];
  readonly form = this.fb.nonNullable.group({
    addressType: 'Registered', countryCode: ['BY', [Validators.required, Validators.minLength(2), Validators.maxLength(2)]],
    locality: '', postalCode: '', addressLine: ['', [Validators.required, Validators.pattern(/.*\S.*/)]],
    gln: ['', Validators.pattern(/^$|^\d{13}$/)], isPrimary: false
  });
  constructor() {
    this.reload.pipe(switchMap(() => {
      if (this.owner !== 'organization' && !this.ownerId) return of([] as AddressDetails[]);
      return this.requests.addresses(this.owner, this.ownerId).pipe(catchError(() => {
        this.state = 'error';
        return of(null);
      }));
    }), takeUntilDestroyed(this.destroyRef)).subscribe(items => {
      if (items === null) return;
      this.items = items;
      this.state = items.length ? 'ready' : 'empty';
    });
  }
  ngOnChanges(): void {
    const key = this.owner + ':' + (this.ownerId ?? '');
    if (key === this.ownerKey) return;
    this.ownerKey = key;
    this.items = [];
    this.editing = undefined;
    this.showEditor = false;
    this.form.reset();
    this.load();
  }
  load(): void { this.state = 'loading'; this.reload.next(); }
  add(): void {
    if (!this.canManage || this.requests.saving()) return;
    this.editing = undefined; this.showEditor = true;
    this.form.reset({ addressType: 'Registered', countryCode: 'BY', locality: '', postalCode: '', addressLine: '', gln: '', isPrimary: this.items.length === 0 });
  }
  edit(x: AddressDetails): void { if (this.requests.saving()) return;
    if (!this.canManage || this.requests.saving() || !this.items.some(item => item.id === x.id)) return;
    this.editing = x; this.showEditor = true;
    this.form.reset({ addressType: x.addressType, countryCode: x.countryCode, locality: x.locality ?? '', postalCode: x.postalCode ?? '', addressLine: x.addressLine, gln: x.gln ?? '', isPrimary: x.isPrimary });
  }
  save(): void {
    if (!this.canManage || this.requests.saving()) return;
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const key = this.ownerKey;
    const v = this.form.getRawValue();
    const body = { ...v, locality: v.locality || null, postalCode: v.postalCode || null, gln: v.gln || null, rowVersion: this.editing?.rowVersion ?? null };
    const request = this.editing ? this.requests.updateAddress(this.editing.id, body) : this.requests.createAddress(this.owner, this.ownerId, body);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { if (key !== this.ownerKey) return; this.showEditor = false; this.load(); },
      error: () => { if (key === this.ownerKey) this.state = 'error'; }
    });
  }
  active(x: AddressDetails, value: boolean): void {
    if (!this.canManage || this.requests.saving()) return;
    this.requests.setAddressActive(x.id, value).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: () => this.load(), error: () => this.state = 'error' });
  }
  primary(x: AddressDetails): void {
    if (!this.canManage || this.requests.saving()) return;
    this.requests.makeAddressPrimary(x.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: () => this.load(), error: () => this.state = 'error' });
  }
  addressText(x: AddressDetails): string { return [x.postalCode, x.locality, x.addressLine].filter(Boolean).join(', '); }
}
