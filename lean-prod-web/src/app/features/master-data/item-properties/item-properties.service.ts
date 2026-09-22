import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { ItemBatch, PropertyDefinition, PropertyValues } from './item-properties.models';
import { Page } from '../master-data.models';

@Injectable({ providedIn: 'root' })
export class ItemPropertiesService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;
  definitions(classId: string) { return this.http.get<PropertyDefinition[]>(`${this.api}catalog-item-classes/${classId}/properties`); }
  saveDefinition(classId: string, id: string | undefined, body: object) {
    const url = `${this.api}catalog-item-classes/${classId}/properties`;
    return id ? this.http.put<PropertyDefinition>(`${url}/${id}`, body) : this.http.post<PropertyDefinition>(url, body);
  }
  values(itemId: string, batchId?: string) { return this.http.get<PropertyValues>(this.valuesUrl(itemId, batchId)); }
  saveValues(itemId: string, batchId: string | undefined, body: object) { return this.http.put<PropertyValues>(this.valuesUrl(itemId, batchId), body); }
  batches(itemId: string, page = 1) { return this.http.get<Page<ItemBatch>>(`${this.api}catalog-items/${itemId}/batches`, { params: { page } }); }
  saveBatch(itemId: string, id: string | undefined, body: object) {
    const url = `${this.api}catalog-items/${itemId}/batches`;
    return id ? this.http.put<ItemBatch>(`${url}/${id}`, body) : this.http.post<ItemBatch>(url, body);
  }
  private valuesUrl(itemId: string, batchId?: string) {
    return `${this.api}catalog-items/${itemId}${batchId ? `/batches/${batchId}` : ''}/properties`;
  }
}
