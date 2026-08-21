import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { LanguageService } from '../../../core/i18n/language.service';
import { UnitCatalogOption, UnitConversion, UnitDetails, UnitSummary } from '../master-data.models';
import { MasterDataService } from '../master-data.service';

@Component({
  selector: 'app-units-of-measure', standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslocoPipe],
  templateUrl: './units-of-measure.component.html', styleUrls: ['../master-data.css', './units-of-measure.component.css']
})
export class UnitsOfMeasureComponent implements OnInit {
  private readonly api = inject(MasterDataService); private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService); private readonly t = inject(TranslocoService);
  readonly language = inject(LanguageService); readonly canManage = this.auth.hasPermission(Permissions.masterDataManage);
  @ViewChild('unitDialog') unitDialog?: ElementRef<HTMLDialogElement>;
  @ViewChild('conversionDialog') conversionDialog?: ElementRef<HTMLDialogElement>;
  items: UnitSummary[] = []; catalog: UnitCatalogOption[] = []; conversions: UnitConversion[] = [];
  unitOptions: UnitSummary[] = [];
  hoveredConversion?: UnitConversion;
  selected?: UnitDetails; selectedCatalog?: UnitCatalogOption; page = 1; total = 0; message = '';
  readonly quantityTypes = ['Mass', 'Length', 'Area', 'Volume', 'Time', 'Temperature', 'Count', 'Other'];
  readonly filters = this.fb.nonNullable.group({ search: '', isActive: '' });
  readonly catalogSearch = this.fb.nonNullable.control('');
  readonly createForm = this.fb.nonNullable.group({ quantityType: ['', Validators.required], decimalPlaces: [3, [Validators.required, Validators.min(0), Validators.max(6)]], localizedName: '' });
  readonly editForm = this.fb.nonNullable.group({ quantityType: ['', Validators.required], decimalPlaces: [0, [Validators.required, Validators.min(0), Validators.max(6)]], localizedName: '' });
  readonly conversionForm = this.fb.nonNullable.group({ fromUnitId: ['', Validators.required], toUnitId: ['', Validators.required], multiplier: [1, [Validators.required, Validators.min(0.000000000001)]], offset: [0, Validators.required] });

  ngOnInit(): void { this.load(); this.loadUnitOptions(); this.loadConversions(); }
  load(page = 1): void { const f = this.filters.getRawValue(); this.api.units(page, f.search.trim(), f.isActive, this.language.current()).subscribe(x => { this.items = x.items; this.page = x.page; this.total = x.totalCount; }); }
  select(item: UnitSummary): void { this.api.unit(item.id, this.language.current()).subscribe(x => { this.selected = x; this.editForm.reset({ quantityType: x.quantityType, decimalPlaces: x.decimalPlaces, localizedName: x.translations.find(v => v.languageCode === this.language.current())?.name ?? '' }); }); }
  openUnitDialog(): void { this.selectedCatalog = undefined; this.catalogSearch.setValue(''); this.createForm.reset({ quantityType: '', decimalPlaces: 3, localizedName: '' }); this.searchCatalog(); this.unitDialog?.nativeElement.showModal(); }
  searchCatalog(): void { this.api.unitCatalog(this.catalogSearch.value.trim()).subscribe(x => this.catalog = x); }
  chooseCatalog(item: UnitCatalogOption): void { if (!item.isAdded) this.selectedCatalog = item; }
  createUnit(): void { if (!this.selectedCatalog || this.createForm.invalid || (this.language.current() !== 'en' && !this.createForm.value.localizedName?.trim())) return; const v = this.createForm.getRawValue(); this.api.createUnit({ catalogCode: this.selectedCatalog.code, quantityType: v.quantityType, decimalPlaces: v.decimalPlaces, languageCode: this.language.current(), localizedName: this.language.current() === 'en' ? null : v.localizedName.trim() }).subscribe(x => { this.unitDialog?.nativeElement.close(); this.selected = x; this.message = this.t.translate('masterData.saved'); this.load(); this.loadUnitOptions(); }); }
  saveUnit(): void { if (!this.selected || this.editForm.invalid || (this.language.current() !== 'en' && !this.editForm.value.localizedName?.trim())) return; const v = this.editForm.getRawValue(); this.api.updateUnit(this.selected.id, { quantityType: v.quantityType, decimalPlaces: v.decimalPlaces, languageCode: this.language.current(), localizedName: this.language.current() === 'en' ? null : v.localizedName.trim(), rowVersion: this.selected.rowVersion }).subscribe(x => { this.selected = x; this.message = this.t.translate('masterData.saved'); this.load(this.page); this.loadConversions(); }); }
  setActive(active: boolean): void { if (!this.selected) return; this.api.setUnitActive(this.selected.id, active).subscribe(() => { this.selected = { ...this.selected!, isActive: active }; this.load(this.page); this.loadUnitOptions(); }); }
  openConversionDialog(): void { this.conversionForm.reset({ fromUnitId: '', toUnitId: '', multiplier: 1, offset: 0 }); this.conversionDialog?.nativeElement.showModal(); }
  createConversion(): void { if (this.conversionForm.invalid) return; this.api.createUnitConversion(this.conversionForm.getRawValue()).subscribe(() => { this.conversionDialog?.nativeElement.close(); this.loadConversions(); }); }
  loadConversions(): void { this.api.unitConversions().subscribe(x => this.conversions = x); }
  loadUnitOptions(): void { this.api.unitOptions(this.language.current()).subscribe(x => this.unitOptions = x); }
  deleteConversion(id: string): void { this.api.deleteUnitConversion(id).subscribe(() => this.loadConversions()); }
  conversionHint(item: UnitConversion): string {
    if (item.offset !== 0) return this.t.translate('units.hoverFormula', {
      from: `${item.fromUnitName} (${item.fromLetterCode})`, to: `${item.toUnitName} (${item.toLetterCode})`,
      multiplier: item.multiplier, offset: item.offset
    });
    return this.t.translate('units.hoverRatio', {
      to: `${item.toUnitName} (${item.toLetterCode})`, multiplier: item.multiplier,
      from: `${item.fromUnitName} (${item.fromLetterCode})`
    });
  }
}
