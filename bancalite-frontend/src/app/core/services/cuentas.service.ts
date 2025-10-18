import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, map } from 'rxjs';
import { ApiResult } from '../../shared/models/api.models';
import { CuentaCreateForm, CuentaEstadoForm, CuentaListItem, CuentaPutForm, MovimientoItem } from '../../shared/models/cuentas.models';
import { Paged } from '../../shared/models/clientes.models';
import { Store } from '@ngrx/store';
import { authFeature } from '../state/auth/auth.reducer';
import { switchMap, take } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class CuentasService {
  private baseUrl = environment.apiBaseUrl;
  constructor(private http: HttpClient, private store: Store) {}

  list(pagina = 1, tamano = 10, filtros?: { q?: string; activo?: boolean; clientesActivos?: boolean }): Observable<Paged<CuentaListItem>> {
    return this.store.select(authFeature.selectCodeRol).pipe(
      take(1),
      switchMap(codeRol => {
        const isAdmin = (codeRol || '').toLowerCase() === 'admin';
        if (isAdmin) {
          // Endpoint de admin con filtros completos
          let params = new HttpParams().set('pagina', pagina).set('tamano', tamano);
          if (filtros?.q) params = params.set('q', filtros.q);
          if (typeof filtros?.activo === 'boolean') params = params.set('activo', String(!!filtros.activo));
          if (typeof filtros?.clientesActivos === 'boolean') params = params.set('clientesActivos', String(!!filtros.clientesActivos));
          return this.http
            .get<ApiResult<Paged<CuentaListItem>>>(`${this.baseUrl}/cuentas`, { params })
            .pipe(map(res => res.datos));
        } else {
          // No‑admin: usar /cuentas/mias y aplicar filtros/paginación en cliente
          return this.http
            .get<ApiResult<CuentaListItem[]>>(`${this.baseUrl}/cuentas/mias`)
            .pipe(
              map(res => {
                let items = (res.datos || []) as CuentaListItem[];
                // Filtro por texto: numeroCuenta o clienteNombre
                if (filtros?.q) {
                  const v = filtros.q.toLowerCase();
                  items = items.filter(
                    c => c.numeroCuenta.toLowerCase().includes(v) || (c.clienteNombre || '').toLowerCase().includes(v)
                  );
                }
                // Filtro de cuentas activas (si se pide true). Si false/undefined, no filtramos.
                if (typeof filtros?.activo === 'boolean' && filtros.activo) {
                  items = items.filter(c => (c.estado || '').toLowerCase() === 'activa');
                }
                const total = items.length;
                const start = Math.max(0, (pagina - 1) * tamano);
                const end = start + tamano;
                const pageItems = items.slice(start, end);
                const paged: Paged<CuentaListItem> = { pagina, tamano, total, items: pageItems };
                return paged;
              })
            );
        }
      })
    );
  }

  get(id: string) {
    return this.http.get<ApiResult<any>>(`${this.baseUrl}/cuentas/${id}`).pipe(map(r => r.datos));
  }

  create(payload: CuentaCreateForm) {
    return this.http.post<ApiResult<any>>(`${this.baseUrl}/cuentas`, payload);
  }

  update(id: string, payload: CuentaPutForm) {
    return this.http.put<ApiResult<any>>(`${this.baseUrl}/cuentas/${id}`, payload);
  }

  patchEstado(id: string, payload: CuentaEstadoForm) {
    return this.http.patch<ApiResult<any>>(`${this.baseUrl}/cuentas/${id}/estado`, payload);
  }

  delete(id: string) {
    return this.http.delete<ApiResult<any>>(`${this.baseUrl}/cuentas/${id}`);
  }

  movimientos(numeroCuenta: string): Observable<MovimientoItem[]> {
    const params = new HttpParams().set('numeroCuenta', numeroCuenta);
    return this.http
      .get<ApiResult<MovimientoItem[]>>(`${this.baseUrl}/movimientos`, { params })
      .pipe(map(r => r.datos));
  }
}
