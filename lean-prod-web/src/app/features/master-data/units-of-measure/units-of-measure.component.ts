import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { skip } from 'rxjs';
import { MasterDataUiModule } from '../shared/master-data-ui.module';
import { CommonModule } from '@angular/common';
import { Component, DestroyRef, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslocoPipe, TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { Permissions } from '../../../core/auth/permissions';
import { LanguageService } from '../../../core/i18n/language.service';
import { UnitCatalogOption, UnitConversion, UnitDetails, UnitSummary } from '../master-data.models';
import { MasterDataService } from '../master-data.service';
import { PageState, PageStateComponent } from '../../../core/ui/page-state.component';
import { TableActionsComponent } from '../../../core/ui/table-actions.component';

@Component({
  selector: 'app-units-of-measure', standalone: true,
  imports: [MasterDataUiModule, CommonModule, ReactiveFormsModule, TranslocoPipe, PageStateComponent, TableActionsComponent],
  templateUrl: './units-of-measure.component.html', styleUrls: ['../master-data.css', './units-of-measure.component.css']
})
export class UnitsOfMeasureComponent implements OnInit {
  page = 1;
  private listRequest = 0;

  readonly requests = inject(MasterDataService);
  private readonly api = this.requests; private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly localizedDrafts = new Map<string, string>();
  private previousLanguage = '';
  private readonly auth = inject(AuthService); private readonly t = inject(TranslocoService);
  readonly language = inject(LanguageService); readonly canManage = this.auth.hasPermission(Permissions.masterDataManage);
  @ViewChild('unitDialog') unitDialog?: ElementRef<HTMLDialogElement>;
  @ViewChild('conversionDialog') conversionDialog?: ElementRef<HTMLDialogElement>;
  items: UnitSummary[] = []; catalog: UnitCatalogOption[] = []; conversions: UnitConversion[] = [];
  unitOptions: UnitSummary[] = [];
  hoveredConversion?: UnitConversion;
  selected?: UnitDetails; selectedCatalog?: UnitCatalogOption; total = 0; message = ''; state: PageState = 'loading';
  readonly quantityTypes = ['Mass', 'Length', 'Area', 'Volume', 'Time', 'Temperature', 'Count', 'Other'];
  readonly filters = this.fb.nonNullable.group({ search: '', isActive: '' });
  readonly catalogSearch = this.fb.nonNullable.control('');
  readonly createForm = this.fb.nonNullable.group({ quantityType: ['', Validators.required], decimalPlaces: [3, [Validators.required, Validators.min(0), Validators.max(6)]], localizedName: '' });
  readonly editForm = this.fb.nonNullable.group({ quantityType: ['', Validators.required], decimalPlaces: [0, [Validators.required, Validators.min(0), Validators.max(6)]], localizedName: '' });
  readonly conversionForm = this.fb.nonNullable.group({ fromUnitId: ['', Validators.required], toUnitId: ['', Validators.required], multiplier: [1, [Validators.required, Validators.min(0.000000000001)]], offset: [0, Validators.required] });

  ngOnInit(): void {
    this.previousLanguage = this.language.current();
    this.load(); this.loadUnitOptions(); this.loadConversions();
    this.t.langChanges$.pipe(skip(1), takeUntilDestroyed(this.destroyRef)).subscribe(language => {
      const id = this.selected?.id ?? '';
      this.localizedDrafts.set(id + ':' + this.previousLanguage, this.editForm.controls.localizedName.value);
      this.localizedDrafts.set('new:' + this.previousLanguage, this.createForm.controls.localizedName.value);
      this.editForm.controls.localizedName.setValue(this.localizedDrafts.get(id + ':' + language) ?? this.selected?.translations.find(x => x.languageCode === language)?.name ?? '');
      this.createForm.controls.localizedName.setValue(this.localizedDrafts.get('new:' + language) ?? '');
      this.previousLanguage = language;
      this.load(); this.loadUnitOptions();
    });
  }
  load(more = false): void {
    if (more && this.state === 'loading') return;
    const requestId = ++this.listRequest;
    const requestedPage = more ? this.page + 1 : 1;
    this.state = 'loading';
    const f = this.filters.getRawValue();
    this.api.units(f.search.trim(), f.isActive, this.language.current(), requestedPage).subscribe({
      next: x => {
        if (requestId !== this.listRequest) return;
        this.items = more ? [...this.items, ...x.items] : x.items;
        this.page = requestedPage; this.total = x.totalCount;
        this.state = this.items.length ? 'ready' : 'empty';
      },
      error: () => { if (requestId === this.listRequest) this.state = 'error'; }
    });
  }
  select(item: UnitSummary): void { if (this.requests.saving()) return; this.api.unit(item.id, this.language.current()).subscribe(x => { this.selected = x; this.editForm.reset({ quantityType: x.quantityType, decimalPlaces: x.decimalPlaces, localizedName: x.translations.find(v => v.languageCode === this.language.current())?.name ?? '' }); }); }
  openUnitDialog(): void { this.selectedCatalog = undefined; this.catalogSearch.setValue(''); this.createForm.reset({ quantityType: '', decimalPlaces: 3, localizedName: '' }); this.searchCatalog(); this.unitDialog?.nativeElement.showModal(); }
  editSelected(): void { if (this.requests.saving()) return; if (this.selected) this.editForm.markAsDirty(); }
  copySelected(): void { if (this.requests.saving()) return; if (!this.selected) return; const x = this.selected; this.selectedCatalog = undefined; this.catalogSearch.setValue(''); this.createForm.reset({ quantityType: x.quantityType, decimalPlaces: x.decimalPlaces, localizedName: x.translations.find(v => v.languageCode === this.language.current())?.name ?? '' }); this.searchCatalog(); this.unitDialog?.nativeElement.showModal(); }
  searchCatalog(): void { this.api.unitCatalog(this.catalogSearch.value.trim()).subscribe(x => this.catalog = x); }
  chooseCatalog(item: UnitCatalogOption): void { if (!item.isAdded) this.selectedCatalog = item; }
  createUnit(): void { if (!this.canManage || this.requests.saving()) return; if (!this.selectedCatalog || this.createForm.invalid || (this.language.current() !== 'en' && !this.createForm.value.localizedName?.trim())) return; const v = this.createForm.getRawValue(); this.api.createUnit({ catalogCode: this.selectedCatalog.code, quantityType: v.quantityType, decimalPlaces: v.decimalPlaces, languageCode: this.language.current(), localizedName: this.language.current() === 'en' ? null : v.localizedName.trim() }).subscribe(x => { this.unitDialog?.nativeElement.close(); this.selected = x; this.message = this.t.translate('masterData.saved'); this.load(); this.loadUnitOptions(); }); }
  saveUnit(): void { if (!this.canManage || this.requests.saving()) return; if (!this.selected || this.editForm.invalid || (this.language.current() !== 'en' && !this.editForm.value.localizedName?.trim())) return; const v = this.editForm.getRawValue(); this.api.updateUnit(this.selected.id, { quantityType: v.quantityType, decimalPlaces: v.decimalPlaces, languageCode: this.language.current(), localizedName: this.language.current() === 'en' ? null : v.localizedName.trim(), rowVersion: this.selected.rowVersion }).subscribe(x => { this.selected = x; this.message = this.t.translate('masterData.saved'); this.load(); this.loadConversions(); }); }
  setActive(active: boolean): void { if (!this.canManage || this.requests.saving()) return; if (!this.selected) return; this.api.setUnitActive(this.selected.id, active).subscribe(() => { this.selected = { ...this.selected!, isActive: active }; this.load(); this.loadUnitOptions(); }); }
  openConversionDialog(): void { this.conversionForm.reset({ fromUnitId: '', toUnitId: '', multiplier: 1, offset: 0 }); this.conversionDialog?.nativeElement.showModal(); }
  createConversion(): void { if (!this.canManage || this.requests.saving()) return; if (this.conversionForm.invalid) return; this.api.createUnitConversion(this.conversionForm.getRawValue()).subscribe(() => { this.conversionDialog?.nativeElement.close(); this.loadConversions(); }); }
  loadConversions(): void { this.api.unitConversions().subscribe(x => this.conversions = x); }
  loadUnitOptions(): void { const language = this.language.current(); this.api.unitOptions(language).subscribe(x => { if (language === this.language.current()) this.unitOptions = x; }); }
  deleteConversion(id: string): void { if (!this.canManage || this.requests.saving()) return; this.api.deleteUnitConversion(id).subscribe(() => this.loadConversions()); }
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
