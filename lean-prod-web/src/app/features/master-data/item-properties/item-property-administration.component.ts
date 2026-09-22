import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { Subject, catchError, of, startWith, switchMap, takeUntil, tap } from 'rxjs';
import { MasterDataService } from '../master-data.service';
import { CatalogItemClassOption, CatalogItemType } from '../master-data.models';

@Component({
  selector: 'app-item-property-administration',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, TranslocoPipe],
  styleUrls: ['../master-data.css'],
  template: `
    <h1>{{ 'itemProperties.title' | transloco }}</h1>
    <p>{{ 'itemProperties.administrationHelp' | transloco }}</p>
    <label>{{ 'itemProperties.itemType' | transloco }}
      <select [formControl]="type">
        @for (itemType of types; track itemType) {
          <option [value]="itemType">{{ ('catalogItemTypes.' + itemType) | transloco }}</option>
        }
      </select>
    </label>
    @if (loading) { <p role="status">{{ 'pageState.loading' | transloco }}</p> }
    @if (failed) {
      <p role="alert">{{ 'itemProperties.error' | transloco }}</p>
      <button type="button" (click)="reload()">{{ 'itemProperties.reload' | transloco }}</button>
    }
    @if (!loading && !failed) {
      <table class="master-data-table">
        <thead><tr><th>{{ 'masterData.code' | transloco }}</th><th>{{ 'catalogItemClasses.class' | transloco }}</th><th></th></tr></thead>
        <tbody>
          @for (itemClass of classes; track itemClass.id) {
            <tr><td>{{ itemClass.code }}</td><td>{{ itemClass.name }}</td>
              <td><a [routerLink]="['/administration/item-properties', itemClass.id]">{{ 'itemProperties.settings' | transloco }}</a></td>
            </tr>
          } @empty {
            <tr><td colspan="3">{{ 'itemProperties.noClasses' | transloco }}</td></tr>
          }
        </tbody>
      </table>
    }
  `
})
export class ItemPropertyAdministrationComponent implements OnInit, OnDestroy {
  private readonly api = inject(MasterDataService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyed = new Subject<void>();
  readonly types: CatalogItemType[] = ['Product', 'Work', 'PrimaryMaterial', 'AuxiliaryMaterial',
    'SemiFinishedProduct', 'Packaging', 'ToolingAndTools', 'SparePart', 'PurchasedService', 'Waste'];
  readonly type = new FormControl<CatalogItemType>('Product', { nonNullable: true });
  classes: CatalogItemClassOption[] = [];
  loading = false;
  failed = false;

  ngOnInit(): void {
    const initial = this.route.snapshot.queryParamMap.get('type') as CatalogItemType;
    if (this.types.includes(initial)) this.type.setValue(initial);
    this.type.valueChanges.pipe(
      startWith(this.type.value),
      tap(() => { this.loading = true; this.failed = false; this.classes = []; }),
      switchMap(type => this.api.catalogItemClassOptions(type, false).pipe(
        catchError(() => { this.failed = true; return of([]); })
      )),
      takeUntil(this.destroyed)
    ).subscribe(classes => {
      this.classes = classes.filter(itemClass => !itemClass.isGroup);
      this.loading = false;
    });
  }
  reload(): void { this.type.setValue(this.type.value); }
  ngOnDestroy(): void { this.destroyed.next(); this.destroyed.complete(); }
}
