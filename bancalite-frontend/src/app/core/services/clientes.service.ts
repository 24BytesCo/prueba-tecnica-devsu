import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ClienteForm, ClienteListItem, Paged } from '../../shared/models/clientes.models';
import { Observable, map, switchMap, take, of } from 'rxjs';
import { ApiResult } from '../../shared/models/api.models';
import { Store } from '@ngrx/store';
import { authFeature } from '../state/auth/auth.reducer';

@Injectable({ providedIn: 'root' })
export class ClientesService {
  private baseUrl = environment.apiBaseUrl;
  constructor(private http: HttpClient, private store: Store) {}

  list(pagina = 1, tamano = 10, q = '', estado?: boolean | null): Observable<Paged<ClienteListItem>> {
    return this.store.select(authFeature.selectCodeRol).pipe(
      take(1),
      switchMap(codeRol => {
        const isAdmin = (codeRol || '').toLowerCase() === 'admin';
        if (isAdmin) {
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

        // No‑admin: derivar mi clienteId desde /cuentas/mias y traer detalle con /clientes/{id}
        return this.http.get<ApiResult<any[]>>(`${this.baseUrl}/cuentas/mias`).pipe(
          switchMap(res => {
            const cuentas = (res.datos || []) as any[];
            const clienteId = cuentas[0]?.clienteId;
            if (!clienteId) {
              const empty: Paged<ClienteListItem> = { pagina: 1, tamano, total: 0, items: [] };
              return of(empty);
            }
            return this.http.get<ApiResult<any>>(`${this.baseUrl}/clientes/${clienteId}`).pipe(
              map(r => {
                const d: any = r.datos || {};
                const p: any = d.persona || {};
                const item: ClienteListItem = {
                  clienteId: d.clienteId || d.id || clienteId,
                  personaId: p.id || d.personaId || '',
                  nombres: p.nombres || d.nombres || '',
                  apellidos: p.apellidos || d.apellidos || '',
                  edad: p.edad || d.edad || 0,
                  tipoDocumentoIdentidadId: p.tipoDocumentoIdentidadId || d.tipoDocumentoIdentidadId || '',
                  tipoDocumentoIdentidadNombre: p.tipoDocumentoIdentidadNombre || d.tipoDocumentoIdentidadNombre || '',
                  numeroDocumento: p.numeroDocumento || d.numeroDocumento || '',
                  email: p.email || d.email || null,
                  estado: typeof d.estado === 'boolean' ? d.estado : true
                };
                const items = [item];
                const total = items.length;
                const start = Math.max(0, (pagina - 1) * tamano);
                const pageItems = items.slice(start, start + tamano);
                const paged: Paged<ClienteListItem> = { pagina, tamano, total, items: pageItems };
                return paged;
              })
            );
          })
        );
      })
    );
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
