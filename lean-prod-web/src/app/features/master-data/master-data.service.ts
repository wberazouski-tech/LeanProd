import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AddressDetails, CatalogItem, DepartmentDetails, DepartmentSummary, EquipmentDetails, EquipmentOption, EquipmentStateEvent, EquipmentSummary, EquipmentType, OptionItem, OrganizationDetails, Page, StorageDetails, StorageOption, StorageSummary, UnitCatalogOption, UnitConversion, UnitDetails, UnitSummary } from './master-data.models';

@Injectable({ providedIn: 'root' })
export class MasterDataService {
  private readonly http = inject(HttpClient); private readonly api = environment.apiUrl;
  departments(page = 1, search = '', isActive = '', pageSize = 20) { let p = new HttpParams().set('page', page).set('pageSize', pageSize); if (search) p = p.set('search', search); if (isActive) p = p.set('isActive', isActive); return this.http.get<Page<DepartmentSummary>>(`${this.api}departments`, { params: p }); }
  department(id: string) { return this.http.get<DepartmentDetails>(`${this.api}departments/${id}`); }
  departmentOptions() { return this.http.get<OptionItem[]>(`${this.api}departments/options`); }
  saveDepartment(id: string | undefined, value: object) { return id ? this.http.put<DepartmentDetails>(`${this.api}departments/${id}`, value) : this.http.post<DepartmentDetails>(`${this.api}departments`, value); }
  setDepartmentActive(id: string, active: boolean) { return this.http.post<boolean>(`${this.api}departments/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  storages(page = 1, search = '', isActive = '', pageSize = 20) { let p = new HttpParams().set('page', page).set('pageSize', pageSize); if (search) p = p.set('search', search); if (isActive) p = p.set('isActive', isActive); return this.http.get<Page<StorageSummary>>(`${this.api}storage-locations`, { params: p }); }
  storage(id: string) { return this.http.get<StorageDetails>(`${this.api}storage-locations/${id}`); }
  storageOptions() { return this.http.get<StorageOption[]>(`${this.api}storage-locations/options`); }
  kinds() { return this.http.get<CatalogItem[]>(`${this.api}storage-locations/kinds`); }
  types() { return this.http.get<CatalogItem[]>(`${this.api}storage-locations/types`); }
  saveStorage(id: string | undefined, value: object) { return id ? this.http.put<StorageDetails>(`${this.api}storage-locations/${id}`, value) : this.http.post<StorageDetails>(`${this.api}storage-locations`, value); }
  setStorageActive(id: string, active: boolean) { return this.http.post<boolean>(`${this.api}storage-locations/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  units(page = 1, search = '', isActive = '', language = 'en') { let p = new HttpParams().set('page', page).set('pageSize', 20).set('language', language); if (search) p = p.set('search', search); if (isActive) p = p.set('isActive', isActive); return this.http.get<Page<UnitSummary>>(`${this.api}unit-of-measures`, { params: p }); }
  unit(id: string, language = 'en') { return this.http.get<UnitDetails>(`${this.api}unit-of-measures/${id}`, { params: { language } }); }
  unitOptions(language = 'en') { return this.http.get<UnitSummary[]>(`${this.api}unit-of-measures/options`, { params: { language } }); }
  unitCatalog(search = '') { let p = new HttpParams(); if (search) p = p.set('search', search); return this.http.get<UnitCatalogOption[]>(`${this.api}unit-of-measures/catalog`, { params: p }); }
  createUnit(value: object) { return this.http.post<UnitDetails>(`${this.api}unit-of-measures`, value); }
  updateUnit(id: string, value: object) { return this.http.put<UnitDetails>(`${this.api}unit-of-measures/${id}`, value); }
  setUnitActive(id: string, active: boolean) { return this.http.post<boolean>(`${this.api}unit-of-measures/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  unitConversions() { return this.http.get<UnitConversion[]>(`${this.api}unit-of-measures/conversions`); }
  createUnitConversion(value: object) { return this.http.post<UnitConversion>(`${this.api}unit-of-measures/conversions`, value); }
  deleteUnitConversion(id: string) { return this.http.delete<boolean>(`${this.api}unit-of-measures/conversions/${id}`); }
  equipment(page = 1, filters: { search?: string; isActive?: string; departmentId?: string; equipmentTypeId?: string; state?: string } = {}) { let p = new HttpParams().set('page', page).set('pageSize', 20); for (const [key, value] of Object.entries(filters)) if (value) p = p.set(key, value); return this.http.get<Page<EquipmentSummary>>(`${this.api}equipment`, { params: p }); }
  equipmentDetails(id: string) { return this.http.get<EquipmentDetails>(`${this.api}equipment/${id}`); }
  equipmentOptions() { return this.http.get<EquipmentOption[]>(`${this.api}equipment/options`); }
  saveEquipment(id: string | undefined, value: object) { return id ? this.http.put<EquipmentDetails>(`${this.api}equipment/${id}`, value) : this.http.post<EquipmentDetails>(`${this.api}equipment`, value); }
  setEquipmentActive(id: string, active: boolean) { return this.http.post<boolean>(`${this.api}equipment/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  equipmentTypes(activeOnly = false) { return this.http.get<EquipmentType[]>(`${this.api}equipment-types`, { params: { activeOnly } }); }
  saveEquipmentType(id: string | undefined, value: object) { return id ? this.http.put<EquipmentType>(`${this.api}equipment-types/${id}`, value) : this.http.post<EquipmentType>(`${this.api}equipment-types`, value); }
  setEquipmentTypeActive(id: string, active: boolean) { return this.http.post<boolean>(`${this.api}equipment-types/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  equipmentStates(id: string) { return this.http.get<EquipmentStateEvent[]>(`${this.api}equipment/${id}/states`); }
  changeEquipmentState(id: string, value: object) { return this.http.post<EquipmentStateEvent>(`${this.api}equipment/${id}/states`, value); }
  updateEquipmentState(id: string, eventId: string, value: object) { return this.http.put<EquipmentStateEvent>(`${this.api}equipment/${id}/states/${eventId}`, value); }
  organization() { return this.http.get<OrganizationDetails>(`${this.api}organization`); }
  saveOrganization(value: object) { return this.http.put<OrganizationDetails>(`${this.api}organization`, value); }
  addresses(owner: 'organization' | 'department' | 'storage-location', id?: string) { return this.http.get<AddressDetails[]>(this.ownerAddressUrl(owner, id)); }
  createAddress(owner: 'organization' | 'department' | 'storage-location', id: string | undefined, value: object) { return this.http.post<AddressDetails>(this.ownerAddressUrl(owner, id), value); }
  updateAddress(id: string, value: object) { return this.http.put<AddressDetails>(`${this.api}addresses/${id}`, value); }
  setAddressActive(id: string, active: boolean) { return this.http.post<boolean>(`${this.api}addresses/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  makeAddressPrimary(id: string) { return this.http.post<boolean>(`${this.api}addresses/${id}/make-primary`, {}); }
  private ownerAddressUrl(owner: 'organization' | 'department' | 'storage-location', id?: string): string { return owner === 'organization' ? `${this.api}organization/addresses` : `${this.api}${owner === 'department' ? 'departments' : 'storage-locations'}/${id}/addresses`; }
}
