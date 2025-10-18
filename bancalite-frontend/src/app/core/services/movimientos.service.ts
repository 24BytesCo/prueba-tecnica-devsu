import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, map } from 'rxjs';
import { ApiResult } from '../../shared/models/api.models';
import { MovimientoItem } from '../../shared/models/cuentas.models';
import { MovimientoCreateForm, MovimientoCreado } from '../../shared/models/movimientos.models';

@Injectable({ providedIn: 'root' })
export class MovimientosService {
  private baseUrl = environment.apiBaseUrl;
  constructor(private http: HttpClient) {}

  list(numeroCuenta: string, desde?: string | Date, hasta?: string | Date): Observable<MovimientoItem[]> {
    if (!numeroCuenta || !numeroCuenta.trim()) return new Observable<MovimientoItem[]>(sub => { sub.next([]); sub.complete(); });
    let params = new HttpParams().set('numeroCuenta', numeroCuenta.trim());
    if (desde) params = params.set('desde', typeof desde === 'string' ? desde : (desde as Date).toISOString());
    if (hasta) params = params.set('hasta', typeof hasta === 'string' ? hasta : (hasta as Date).toISOString());
    return this.http
      .get<ApiResult<MovimientoItem[]>>(`${this.baseUrl}/movimientos`, { params })
      .pipe(map(r => r.datos || []));
  }

  create(payload: MovimientoCreateForm): Observable<ApiResult<MovimientoCreado>> {
    const headers = payload.idempotencyKey ? new HttpHeaders({ 'Idempotency-Key': payload.idempotencyKey }) : undefined;
    return this.http.post<ApiResult<MovimientoCreado>>(`${this.baseUrl}/movimientos`, payload, { headers });
  }
}
