import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { EstadoCuentaDto } from '../../shared/models/reportes.models';

@Injectable({ providedIn: 'root' })
export class ReportesService {
  private baseUrl = environment.apiBaseUrl;
  constructor(private http: HttpClient) {}

  estadoCuenta(params: { clienteId?: string; numeroCuenta?: string; desde: string | Date; hasta: string | Date }): Observable<EstadoCuentaDto> {
    let hp = new HttpParams();
    if (params.clienteId) hp = hp.set('clienteId', params.clienteId);
    if (params.numeroCuenta) hp = hp.set('numeroCuenta', params.numeroCuenta);
    const desde = typeof params.desde === 'string' ? params.desde : (params.desde as Date).toISOString();
    const hasta = typeof params.hasta === 'string' ? params.hasta : (params.hasta as Date).toISOString();
    hp = hp.set('desde', desde).set('hasta', hasta);
    return this.http.get<EstadoCuentaDto>(`${this.baseUrl}/reportes`, { params: hp });
  }

  estadoCuentaPdfUrl(params: { clienteId?: string; numeroCuenta?: string; desde: string | Date; hasta: string | Date }): string {
    const q: string[] = [];
    if (params.clienteId) q.push(`clienteId=${encodeURIComponent(params.clienteId)}`);
    if (params.numeroCuenta) q.push(`numeroCuenta=${encodeURIComponent(params.numeroCuenta)}`);
    const desde = typeof params.desde === 'string' ? params.desde : (params.desde as Date).toISOString();
    const hasta = typeof params.hasta === 'string' ? params.hasta : (params.hasta as Date).toISOString();
    q.push(`desde=${encodeURIComponent(desde)}`);
    q.push(`hasta=${encodeURIComponent(hasta)}`);
    return `${this.baseUrl}/reportes/pdf?${q.join('&')}`;
  }

  estadoCuentaPdfBlob(params: { clienteId?: string; numeroCuenta?: string; desde: string | Date; hasta: string | Date }) {
    let hp = new HttpParams();
    if (params.clienteId) hp = hp.set('clienteId', params.clienteId);
    if (params.numeroCuenta) hp = hp.set('numeroCuenta', params.numeroCuenta);
    const desde = typeof params.desde === 'string' ? params.desde : (params.desde as Date).toISOString();
    const hasta = typeof params.hasta === 'string' ? params.hasta : (params.hasta as Date).toISOString();
    hp = hp.set('desde', desde).set('hasta', hasta);
    return this.http.get(`${this.baseUrl}/reportes/pdf`, { params: hp, responseType: 'blob' });
  }

  estadoCuentaPdfBase64(params: { clienteId?: string; numeroCuenta?: string; desde: string | Date; hasta: string | Date }): Observable<{ fileName: string; contentType: string; base64: string; }> {
    let hp = new HttpParams();
    if (params.clienteId) hp = hp.set('clienteId', params.clienteId);
    if (params.numeroCuenta) hp = hp.set('numeroCuenta', params.numeroCuenta);
    const desde = typeof params.desde === 'string' ? params.desde : (params.desde as Date).toISOString();
    const hasta = typeof params.hasta === 'string' ? params.hasta : (params.hasta as Date).toISOString();
    hp = hp.set('desde', desde).set('hasta', hasta);
    return this.http.get<{ fileName: string; contentType: string; base64: string; }>(`${this.baseUrl}/reportes/pdf-base64`, { params: hp });
  }

  estadoCuentaJsonBlob(params: { clienteId?: string; numeroCuenta?: string; desde: string | Date; hasta: string | Date }) {
    let hp = new HttpParams();
    if (params.clienteId) hp = hp.set('clienteId', params.clienteId);
    if (params.numeroCuenta) hp = hp.set('numeroCuenta', params.numeroCuenta);
    const desde = typeof params.desde === 'string' ? params.desde : (params.desde as Date).toISOString();
    const hasta = typeof params.hasta === 'string' ? params.hasta : (params.hasta as Date).toISOString();
    hp = hp.set('desde', desde).set('hasta', hasta);
    return this.http.get(`${this.baseUrl}/reportes/json`, { params: hp, responseType: 'blob' });
  }
}
