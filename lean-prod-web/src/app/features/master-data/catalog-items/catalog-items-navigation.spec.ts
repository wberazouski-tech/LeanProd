import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { TranslocoService } from '@jsverse/transloco';
import { AuthService } from '../../../core/auth/auth.service';
import { MasterDataService } from '../master-data.service';
import { CatalogItemDetails } from '../master-data.models';
import { CatalogItemsComponent } from './catalog-items.component';

describe('Items page navigation', () => {
  let component: CatalogItemsComponent;
  let router: jasmine.SpyObj<Router>;
  const item: CatalogItemDetails = { id: 'item', type: 'Product', workingName: 'Oil', fullName: null, articleNumber: 'OIL',
    baseUnitOfMeasureId: 'unit', baseUnitName: 'Piece', baseUnitSymbol: 'pc', catalogItemClassId: 'class',
    catalogItemClassCode: 'C', catalogItemClassName: 'Class', cost: '10.00', description: null, isActive: true, rowVersion: 'v1' };

  beforeEach(() => {
    router = jasmine.createSpyObj<Router>('Router', ['navigate'], { url: '/catalog/products' });
    const api = { saving: () => false, catalogItem: jasmine.createSpy().and.returnValue(of(item)),
      catalogItems: jasmine.createSpy().and.returnValue(of({ items: [item], totalCount: 1 })) };
    TestBed.configureTestingModule({ providers: [
      { provide: MasterDataService, useValue: api }, { provide: AuthService, useValue: { hasPermission: () => true } },
      { provide: TranslocoService, useValue: { translate: (key: string) => key } },
      { provide: ActivatedRoute, useValue: { data: of({ type: 'Product' }) } }, { provide: Router, useValue: router }
    ] });
    component = TestBed.runInInjectionContext(() => new CatalogItemsComponent());
  });

  it('opens the separate item editor from a row action', async () => {
    component.edit(item, new Event('click'));
    await Promise.resolve();
    expect(router.navigate).toHaveBeenCalledWith(['/catalog/items', 'item', 'edit'], { queryParams: { back: '/catalog/products' } });
  });

  it('opens the separate item editor for the selected item', async () => {
    component.selected = item;
    component.editSelected();
    await Promise.resolve();
    expect(router.navigate).toHaveBeenCalledWith(['/catalog/items', 'item', 'edit'], { queryParams: { back: '/catalog/products' } });
  });
});
