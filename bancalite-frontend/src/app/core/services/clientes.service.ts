import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ClienteForm, ClienteListItem, Paged } from '../../shared/models/clientes.models';
import { Observable, map } from 'rxjs';
import { ApiResult } from '../../shared/models/api.models';

@Injectable({ providedIn: 'root' })
export class ClientesService {
  private baseUrl = environment.apiBaseUrl;
  constructor(private http: HttpClient) {}

  list(pagina = 1, tamano = 10, q = '', estado?: boolean | null): Observable<Paged<ClienteListItem>> {
    let params = new HttpParams().set('pagina', pagina).set('tamano', tamano);
    const term = (q || '').trim();
    if (term) {
      const isNumeric = /^[0-9]+$/.test(term);
      params = isNumeric ? params.set('numeroDocumento', term) : params.set('nombres', term);
    }
    if (typeof estado === 'boolean') params = params.set('estado', String(estado));
    return this.http
      .get<ApiResult<Paged<ClienteListItem>>>(`${this.baseUrl}/clientes`, { params })
      .pipe(map(res => res.datos));
  }

  get(id: string) {
    return this.http.get<ApiResult<any>>(`${this.baseUrl}/clientes/${id}`).pipe(map(r => r.datos));
  }

  create(payload: ClienteForm) {
    return this.http.post<ApiResult<any>>(`${this.baseUrl}/clientes`, payload);
  }

  updatePut(id: string, payload: ClienteForm) {
    return this.http.put<ApiResult<any>>(`${this.baseUrl}/clientes/${id}`, payload);
  }

  delete(id: string) {
    return this.http.delete<ApiResult<any>>(`${this.baseUrl}/clientes/${id}`);
  }
}
