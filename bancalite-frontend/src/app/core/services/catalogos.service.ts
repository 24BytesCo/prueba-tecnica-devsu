import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, map } from 'rxjs';
import { ApiResult } from '../../shared/models/api.models';

export interface CatalogoItem { id: string; codigo: string; nombre: string; activo: boolean; }

@Injectable({ providedIn: 'root' })
export class CatalogosService {
  private baseUrl = environment.apiBaseUrl;
  constructor(private http: HttpClient) {}

  generos(): Observable<CatalogoItem[]> {
    return this.http.get<ApiResult<CatalogoItem[]>>(`${this.baseUrl}/catalogos/generos`).pipe(map(r => r.datos));
  }

  tiposDocumento(): Observable<CatalogoItem[]> {
    return this.http.get<ApiResult<CatalogoItem[]>>(`${this.baseUrl}/catalogos/tipos-documento`).pipe(map(r => r.datos));
  }

  tiposCuenta(): Observable<CatalogoItem[]> {
    return this.http.get<ApiResult<CatalogoItem[]>>(`${this.baseUrl}/catalogos/tipos-cuenta`).pipe(map(r => r.datos));
  }

  tiposMovimiento(): Observable<CatalogoItem[]> {
    return this.http.get<ApiResult<CatalogoItem[]>>(`${this.baseUrl}/catalogos/tipos-movimiento`).pipe(map(r => r.datos));
  }
}
