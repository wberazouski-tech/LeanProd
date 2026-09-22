import { TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';
import { AddressListComponent } from './address-list.component';
import { MasterDataService } from '../master-data.service';
import { AddressDetails } from '../master-data.models';

describe('Address owner isolation', () => {
  let component: AddressListComponent;
  let first: Subject<AddressDetails[]>;
  let second: Subject<AddressDetails[]>;
  const address = (id: string): AddressDetails => ({ id, addressType: 'Office', countryCode: 'BY', locality: null, postalCode: null, addressLine: id, gln: null, isPrimary: false, isActive: true, rowVersion: 'v1' });
  beforeEach(() => {
    first = new Subject(); second = new Subject();
    TestBed.configureTestingModule({ providers: [{ provide: MasterDataService, useValue: {
      addresses: (_owner: string, id: string) => id === 'A' ? first : second, saving: () => false
    } }] });
    component = TestBed.runInInjectionContext(() => new AddressListComponent());
    component.owner = 'department'; component.canManage = true;
  });
  it('ignores late responses from the previous owner', () => {
    component.ownerId = 'A'; component.ngOnChanges();
    component.ownerId = 'B'; component.ngOnChanges();
    second.next([address('B-address')]);
    first.next([address('A-address')]);
    expect(component.items.map(x => x.id)).toEqual(['B-address']);
    expect(first.observed).toBeFalse();
  });
  it('closes and clears the previous owner editor', () => {
    component.ownerId = 'A'; component.ngOnChanges();
    first.next([address('A-address')]);
    component.edit(component.items[0]);
    expect(component.showEditor).toBeTrue();
    component.ownerId = 'B'; component.ngOnChanges();
    expect(component.editing).toBeUndefined();
    expect(component.showEditor).toBeFalse();
    expect(component.items).toEqual([]);
  });
  it('exposes load failure instead of an apparently empty successful list', () => {
    component.ownerId = 'A'; component.ngOnChanges();
    first.error(new Error('offline'));
    expect(component.state).toBe('error');
  });
});
