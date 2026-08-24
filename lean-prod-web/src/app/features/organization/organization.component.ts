import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthService } from '../../core/auth/auth.service';
import { Permissions } from '../../core/auth/permissions';
import { AddressListComponent } from '../master-data/addresses/address-list.component';
import { OrganizationDetails } from '../master-data/master-data.models';
import { MasterDataService } from '../master-data/master-data.service';
import { PageState, PageStateComponent } from '../../core/ui/page-state.component';

@Component({ selector: 'app-organization', standalone: true, imports: [ReactiveFormsModule, TranslocoPipe, AddressListComponent, PageStateComponent], templateUrl: './organization.component.html', styleUrls: ['../master-data/master-data.css', './organization.component.css'] })
export class OrganizationComponent implements OnInit {
  private readonly api = inject(MasterDataService); private readonly fb = inject(FormBuilder); private readonly auth = inject(AuthService);
  organization?: OrganizationDetails; readonly canManage = this.auth.hasPermission(Permissions.organizationManage); message = ''; state: PageState = 'loading';
  readonly form = this.fb.nonNullable.group({ legalName: ['', Validators.required], tradingName: '', legalForm: '', countryCode: ['BY', [Validators.required, Validators.minLength(2), Validators.maxLength(2)]], taxNumber: '', statisticalNumber: '', companyRegistrationNumber: '', defaultCurrencyCode: ['BYN', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]], timeZoneId: ['Europe/Minsk', Validators.required], defaultLanguageCode: 'be', email: '', phone: '', website: '', printFooter: '' });
  ngOnInit(): void { this.api.organization().subscribe({ next: x => { this.organization = x; this.form.patchValue({ ...x, tradingName: x.tradingName ?? '', legalForm: x.legalForm ?? '', taxNumber: x.taxNumber ?? '', statisticalNumber: x.statisticalNumber ?? '', companyRegistrationNumber: x.companyRegistrationNumber ?? '', email: x.email ?? '', phone: x.phone ?? '', website: x.website ?? '', printFooter: x.printFooter ?? '' }); if (!this.canManage) this.form.disable(); this.state = 'ready'; }, error: () => this.state = 'error' }); }
  save(): void { if (this.form.invalid) return; this.state = 'saving'; const v = this.form.getRawValue(); const nullable = (x: string) => x.trim() || null; this.api.saveOrganization({ ...v, tradingName: nullable(v.tradingName), legalForm: nullable(v.legalForm), taxNumber: nullable(v.taxNumber), statisticalNumber: nullable(v.statisticalNumber), companyRegistrationNumber: nullable(v.companyRegistrationNumber), email: nullable(v.email), phone: nullable(v.phone), website: nullable(v.website), printFooter: nullable(v.printFooter), logoFileId: this.organization?.logoFileId ?? null, rowVersion: this.organization?.rowVersion ?? null }).subscribe({ next: x => { this.organization = x; this.message = '✓'; this.state = 'success'; }, error: () => this.state = 'error' }); }
}
