import { defer, EMPTY, finalize } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AddressDetails, CatalogItem, CatalogItemClassDetails, CatalogItemClassOption, CatalogItemClassSummary, CatalogItemDetails, CatalogItemSummary, CatalogItemType, CatalogTechnologyDetails, CatalogTechnologySummary, DepartmentDetails, DepartmentSummary, EquipmentDetails, EquipmentOption, EquipmentStateEvent, EquipmentSummary, EquipmentType, OptionItem, OrganizationDetails, Page, StorageDetails, StorageOption, StorageSummary, TechnologyStage, TechnologyStageTemplate, UnitCatalogOption, UnitConversion, UnitDetails, UnitSummary } from './master-data.models';

@Injectable({ providedIn: 'root' })
export class MasterDataService {
  private readonly pending = signal(false);
  readonly saving = this.pending.asReadonly();
  private mutate<T>(method: string, url: string, body?: object) {
    return defer(() => {
      if (this.pending()) return EMPTY;
      this.pending.set(true);
      return this.http.request<T>(method, url, { body }).pipe(finalize(() => this.pending.set(false)));
    });
  }
  private readonly http = inject(HttpClient); private readonly api = environment.apiUrl;
  departments(page = 1, search = '', isActive = '', pageSize = 20) { let p = new HttpParams().set('page', page).set('pageSize', pageSize); if (search) p = p.set('search', search); if (isActive) p = p.set('isActive', isActive); return this.http.get<Page<DepartmentSummary>>(`${this.api}departments`, { params: p }); }
  department(id: string) { return this.http.get<DepartmentDetails>(`${this.api}departments/${id}`); }
  departmentOptions() { return this.http.get<OptionItem[]>(`${this.api}departments/options`); }
  workScheduleOptions() { return this.http.get<OptionItem[]>(`${this.api}departments/work-schedule-options`); }
  saveDepartment(id: string | undefined, value: object) { return id ? this.mutate<DepartmentDetails>('PUT', `${this.api}departments/${id}`, value) : this.mutate<DepartmentDetails>('POST', `${this.api}departments`, value); }
  setDepartmentActive(id: string, active: boolean) { return this.mutate<boolean>('POST', `${this.api}departments/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  storages(page = 1, search = '', isActive = '', pageSize = 20) { let p = new HttpParams().set('page', page).set('pageSize', pageSize); if (search) p = p.set('search', search); if (isActive) p = p.set('isActive', isActive); return this.http.get<Page<StorageSummary>>(`${this.api}storage-locations`, { params: p }); }
  storage(id: string) { return this.http.get<StorageDetails>(`${this.api}storage-locations/${id}`); }
  storageOptions() { return this.http.get<StorageOption[]>(`${this.api}storage-locations/options`); }
  kinds() { return this.http.get<CatalogItem[]>(`${this.api}storage-locations/kinds`); }
  types() { return this.http.get<CatalogItem[]>(`${this.api}storage-locations/types`); }
  saveStorage(id: string | undefined, value: object) { return id ? this.mutate<StorageDetails>('PUT', `${this.api}storage-locations/${id}`, value) : this.mutate<StorageDetails>('POST', `${this.api}storage-locations`, value); }
  setStorageActive(id: string, active: boolean) { return this.mutate<boolean>('POST', `${this.api}storage-locations/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  units(search = '', isActive = '', language = 'en', page = 1) { let p = new HttpParams().set('page', page).set('pageSize', 5000).set('language', language); if (search) p = p.set('search', search); if (isActive) p = p.set('isActive', isActive); return this.http.get<Page<UnitSummary>>(`${this.api}unit-of-measures`, { params: p }); }
  unit(id: string, language = 'en') { return this.http.get<UnitDetails>(`${this.api}unit-of-measures/${id}`, { params: { language } }); }
  unitOptions(language = 'en') { return this.http.get<UnitSummary[]>(`${this.api}unit-of-measures/options`, { params: { language } }); }
  unitCatalog(search = '') { let p = new HttpParams(); if (search) p = p.set('search', search); return this.http.get<UnitCatalogOption[]>(`${this.api}unit-of-measures/catalog`, { params: p }); }
  createUnit(value: object) { return this.mutate<UnitDetails>('POST', `${this.api}unit-of-measures`, value); }
  updateUnit(id: string, value: object) { return this.mutate<UnitDetails>('PUT', `${this.api}unit-of-measures/${id}`, value); }
  setUnitActive(id: string, active: boolean) { return this.mutate<boolean>('POST', `${this.api}unit-of-measures/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  unitConversions() { return this.http.get<UnitConversion[]>(`${this.api}unit-of-measures/conversions`); }
  createUnitConversion(value: object) { return this.mutate<UnitConversion>('POST', `${this.api}unit-of-measures/conversions`, value); }
  deleteUnitConversion(id: string) { return this.mutate<boolean>('DELETE', `${this.api}unit-of-measures/conversions/${id}`); }
  catalogItemClasses(type: CatalogItemType, search = '', isActive = '', isGroup = '', page = 1) { let p = new HttpParams().set('type', type).set('page', page).set('pageSize', 5000); if (search) p = p.set('search', search); if (isActive) p = p.set('isActive', isActive); if (isGroup) p = p.set('isGroup', isGroup); return this.http.get<Page<CatalogItemClassSummary>>(`${this.api}catalog-item-classes`, { params: p }); }
  catalogItemClass(id: string) { return this.http.get<CatalogItemClassDetails>(`${this.api}catalog-item-classes/${id}`); }
  catalogItemClassOptions(type: CatalogItemType, activeOnly = true) { return this.http.get<CatalogItemClassOption[]>(`${this.api}catalog-item-classes/options`, { params: { type, activeOnly } }); }
  saveCatalogItemClass(id: string | undefined, value: object) { return id ? this.mutate<CatalogItemClassDetails>('PUT', `${this.api}catalog-item-classes/${id}`, value) : this.mutate<CatalogItemClassDetails>('POST', `${this.api}catalog-item-classes`, value); }
  setCatalogItemClassActive(id: string, active: boolean) { return this.mutate<boolean>('POST', `${this.api}catalog-item-classes/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  equipment(filters: { search?: string; isActive?: string; departmentId?: string; equipmentTypeId?: string; state?: string } = {}, page = 1) { let p = new HttpParams().set('page', page).set('pageSize', 5000); for (const [key, value] of Object.entries(filters)) if (value) p = p.set(key, value); return this.http.get<Page<EquipmentSummary>>(`${this.api}equipment`, { params: p }); }
  equipmentDetails(id: string) { return this.http.get<EquipmentDetails>(`${this.api}equipment/${id}`); }
  equipmentOptions() { return this.http.get<EquipmentOption[]>(`${this.api}equipment/options`); }
  saveEquipment(id: string | undefined, value: object) { return id ? this.mutate<EquipmentDetails>('PUT', `${this.api}equipment/${id}`, value) : this.mutate<EquipmentDetails>('POST', `${this.api}equipment`, value); }
  setEquipmentActive(id: string, active: boolean) { return this.mutate<boolean>('POST', `${this.api}equipment/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  equipmentTypes(activeOnly = false) { return this.http.get<EquipmentType[]>(`${this.api}equipment-types`, { params: { activeOnly } }); }
  saveEquipmentType(id: string | undefined, value: object) { return id ? this.mutate<EquipmentType>('PUT', `${this.api}equipment-types/${id}`, value) : this.mutate<EquipmentType>('POST', `${this.api}equipment-types`, value); }
  setEquipmentTypeActive(id: string, active: boolean) { return this.mutate<boolean>('POST', `${this.api}equipment-types/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  equipmentStates(id: string) { return this.http.get<EquipmentStateEvent[]>(`${this.api}equipment/${id}/states`); }
  changeEquipmentState(id: string, value: object) { return this.mutate<EquipmentStateEvent>('POST', `${this.api}equipment/${id}/states`, value); }
  updateEquipmentState(id: string, eventId: string, value: object) { return this.mutate<EquipmentStateEvent>('PUT', `${this.api}equipment/${id}/states/${eventId}`, value); }
  organization() { return this.http.get<OrganizationDetails>(`${this.api}organization`); }
  saveOrganization(value: object) { return this.mutate<OrganizationDetails>('PUT', `${this.api}organization`, value); }
  addresses(owner: 'organization' | 'department' | 'storage-location', id?: string) { return this.http.get<AddressDetails[]>(this.ownerAddressUrl(owner, id)); }
  createAddress(owner: 'organization' | 'department' | 'storage-location', id: string | undefined, value: object) { return this.mutate<AddressDetails>('POST', this.ownerAddressUrl(owner, id), value); }
  updateAddress(id: string, value: object) { return this.mutate<AddressDetails>('PUT', `${this.api}addresses/${id}`, value); }
  setAddressActive(id: string, active: boolean) { return this.mutate<boolean>('POST', `${this.api}addresses/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  makeAddressPrimary(id: string) { return this.mutate<boolean>('POST', `${this.api}addresses/${id}/make-primary`, {}); }
  catalogItems(type: CatalogItemType, search = '', isActive = '', page = 1) { let p = new HttpParams().set('type', type).set('page', page).set('pageSize', 5000); if (search) p = p.set('search', search); if (isActive) p = p.set('isActive', isActive); return this.http.get<Page<CatalogItemSummary>>(`${this.api}catalog-items`, { params: p }); }
  catalogItem(id: string) { return this.http.get<CatalogItemDetails>(`${this.api}catalog-items/${id}`); }
  saveCatalogItem(id: string | undefined, value: object) { return id ? this.mutate<CatalogItemDetails>('PUT', `${this.api}catalog-items/${id}`, value) : this.mutate<CatalogItemDetails>('POST', `${this.api}catalog-items`, value); }
  changeCatalogItemClass(id: string, catalogItemClassId: string) { return this.mutate<CatalogItemDetails>('PATCH', `${this.api}catalog-items/${id}/class`, { catalogItemClassId }); }
  setCatalogItemActive(id: string, active: boolean) { return this.mutate<boolean>('POST', `${this.api}catalog-items/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  technologies(filters: { search?: string; isActive?: string; catalogItemId?: string; catalogItemClassId?: string } = {}, page = 1) { let p = new HttpParams().set('page', page).set('pageSize', 5000); for (const [key, value] of Object.entries(filters)) if (value) p = p.set(key, value); return this.http.get<Page<CatalogTechnologySummary>>(`${this.api}catalog-technologies`, { params: p }); }
  technology(id: string) { return this.http.get<CatalogTechnologyDetails>(`${this.api}catalog-technologies/${id}`); }
  createTechnology(value: object) { return this.mutate<CatalogTechnologyDetails>('POST', `${this.api}catalog-technologies`, value); }
  saveTechnologyHeader(id: string, value: object) { return this.mutate<CatalogTechnologyDetails>('PUT', `${this.api}catalog-technologies/${id}/header`, value); }
  saveTechnologyStages(id: string, value: object) { return this.mutate<CatalogTechnologyDetails>('PUT', `${this.api}catalog-technologies/${id}/stages`, value); }
  addNewTechnologyStage(id: string, value: object) { return this.mutate<CatalogTechnologyDetails>('POST', `${this.api}catalog-technologies/${id}/stages/new`, value); }
  addExistingTechnologyStage(id: string, value: object) { return this.mutate<CatalogTechnologyDetails>('POST', `${this.api}catalog-technologies/${id}/stages/existing`, value); }
  saveTechnologyMaterials(id: string, stageId: string, value: object) { return this.mutate<CatalogTechnologyDetails>('PUT', `${this.api}catalog-technologies/${id}/stages/${stageId}/materials`, value); }
  saveTechnologyOutputs(id: string, stageId: string, value: object) { return this.mutate<CatalogTechnologyDetails>('PUT', `${this.api}catalog-technologies/${id}/stages/${stageId}/outputs`, value); }
  saveTechnologyOperations(id: string, stageId: string, value: object) { return this.mutate<CatalogTechnologyDetails>('PUT', `${this.api}catalog-technologies/${id}/stages/${stageId}/operations`, value); }
  setTechnologyActive(id: string, active: boolean) { return this.mutate<boolean>('POST', `${this.api}catalog-technologies/${id}/${active ? 'activate' : 'deactivate'}`, {}); }
  technologyStageTemplates(activeOnly = false) { return this.http.get<TechnologyStageTemplate[]>(`${this.api}catalog-technologies/stage-templates`, { params: { activeOnly } }); }
  technologyStages(activeOnly = true) { return this.http.get<TechnologyStage[]>(`${this.api}catalog-technologies/technology-stages`, { params: { activeOnly } }); }
  hasTechnologyStageDuplicate(name: string, departmentId: string) { return this.http.get<boolean>(`${this.api}catalog-technologies/stage-duplicates`, { params: { name, departmentId } }); }
  saveTechnologyStageTemplate(id: string | undefined, value: object) { return id ? this.mutate<TechnologyStageTemplate>('PUT', `${this.api}catalog-technologies/stage-templates/${id}`, value) : this.mutate<TechnologyStageTemplate>('POST', `${this.api}catalog-technologies/stage-templates`, value); }
  private ownerAddressUrl(owner: 'organization' | 'department' | 'storage-location', id?: string): string { return owner === 'organization' ? `${this.api}organization/addresses` : `${this.api}${owner === 'department' ? 'departments' : 'storage-locations'}/${id}/addresses`; }
}
