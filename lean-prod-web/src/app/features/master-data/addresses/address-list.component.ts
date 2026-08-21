import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { AddressDetails } from '../master-data.models';
import { MasterDataService } from '../master-data.service';

@Component({ selector: 'app-address-list', standalone: true, imports: [CommonModule, ReactiveFormsModule, TranslocoPipe], templateUrl: './address-list.component.html', styleUrl: '../master-data.css' })
export class AddressListComponent implements OnChanges {
  @Input({ required: true }) owner!: 'organization' | 'department' | 'storage-location'; @Input() ownerId?: string; @Input() canManage = false; @Input() compact = false;
  private readonly api = inject(MasterDataService); private readonly fb = inject(FormBuilder);
  items: AddressDetails[] = []; editing?: AddressDetails; showEditor = false;
  readonly types = ['Registered', 'Correspondence', 'Office', 'Delivery', 'Other'];
  readonly form = this.fb.nonNullable.group({ addressType: 'Registered', countryCode: ['BY', [Validators.required, Validators.minLength(2), Validators.maxLength(2)]], locality: '', postalCode: '', addressLine: ['', Validators.required], gln: ['', Validators.pattern(/^$|^\d{13}$/)], isPrimary: false });
  ngOnChanges(): void { if (this.owner === 'organization' || this.ownerId) this.load(); }
  load(): void { this.api.addresses(this.owner, this.ownerId).subscribe(x => this.items = x); }
  add(): void { this.editing = undefined; this.showEditor = true; this.form.reset({ addressType: 'Registered', countryCode: 'BY', locality: '', postalCode: '', addressLine: '', gln: '', isPrimary: this.items.length === 0 }); }
  edit(x: AddressDetails): void { this.editing = x; this.showEditor = true; this.form.reset({ addressType: x.addressType, countryCode: x.countryCode, locality: x.locality ?? '', postalCode: x.postalCode ?? '', addressLine: x.addressLine, gln: x.gln ?? '', isPrimary: x.isPrimary }); }
  save(): void { if (this.form.invalid) return; const v = this.form.getRawValue(); const body = { ...v, locality: v.locality || null, postalCode: v.postalCode || null, gln: v.gln || null, rowVersion: this.editing?.rowVersion ?? null }; const req = this.editing ? this.api.updateAddress(this.editing.id, body) : this.api.createAddress(this.owner, this.ownerId, body); req.subscribe(() => { this.showEditor = false; this.load(); }); }
  active(x: AddressDetails, value: boolean): void { this.api.setAddressActive(x.id, value).subscribe(() => this.load()); }
  primary(x: AddressDetails): void { this.api.makeAddressPrimary(x.id).subscribe(() => this.load()); }
  addressText(x: AddressDetails): string { return [x.postalCode, x.locality, x.addressLine].filter(Boolean).join(', '); }
}
