import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { CuentasService } from '../../../core/services/cuentas.service';
import { CuentaListItem } from '../../../shared/models/cuentas.models';
import Swal from 'sweetalert2';
import { Store } from '@ngrx/store';
import { authFeature } from '../../../core/state/auth/auth.reducer';

@Component({
  template: `
    <section class="container">
      <div class="row">
        <h1 class="brand">Cuentas</h1>
        <span class="spacer"></span>
        <button class="btn btn-primary btn-lg" (click)="openNew()" [disabled]="clienteInactivo">
          Abrir Cuenta
        </button>
      </div>

      <div  class="row" style="gap:12px; align-items:center; margin: 12px 0;">
        <input
          *ngIf="!isUser"
          class="input"
          (input)="onSearch($event)"
          placeholder="Buscar por nombre, documento o numero de cuenta"
          style="width:420px;"
        />
        <label
          style="display:flex; align-items:center; gap:6px; font-size:13px; color: var(--muted);"
        >
          <input type="checkbox" [checked]="activo" (change)="toggleActivo($event)" /> Cuentas
          activas
        </label>
        <label
          style="display:flex; align-items:center; gap:6px; font-size:13px; color: var(--muted);"
        >
          <input
            type="checkbox"
            [checked]="clientesActivos"
            (change)="toggleClientesActivos($event)"
          />
          Clientes activos
        </label>
      </div>

      <div class="table">
        <div class="thead">
          <div>Nro Cuenta</div>
          <div>Tipo</div>
          <div>Titular</div>
          <div>Cliente</div>
          <div>Saldo</div>
          <div>Estado</div>
          <div style="text-align:right">Acciones</div>
        </div>
        <div class="rowt" *ngFor="let c of items">
          <div>
            <a href (click)="goDetail(c, $event)">{{ c.numeroCuenta }}</a>
          </div>
          <div>{{ c.tipoCuentaNombre }}</div>
          <div>{{ c.clienteNombre }}</div>
          <div>{{ c.clienteActivo ? 'Activo' : 'Inactivo' }}</div>
          <div>{{ c.saldoActual | number: '1.2-2' }}</div>
          <div>{{ c.estado }}</div>
          <div style="text-align:right">
            <a href (click)="openEdit(c, $event)">Editar</a>
            <span class="sep">|</span>
            <a href (click)="confirmDelete(c, $event)">Eliminar</a>
          </div>
        </div>
        <div class="rowt" *ngIf="items.length === 0" style="justify-content:center; color:#888;">
          Sin resultados
        </div>
      </div>

      <div class="row" style="margin-top:12px;">
        <button (click)="prev()" [disabled]="pagina === 1">Anterior</button>
        <span style="margin:0 8px;">Pagina {{ pagina }}</span>
        <button (click)="next()" [disabled]="items.length < tamano">Siguiente</button>
      </div>
    </section>
  `,
  styles: [
    `
      /* 7 columnas para cuentas (incluye estado de cliente) */
      .thead,
      .rowt {
        grid-template-columns: 1.2fr 1fr 1.4fr 0.9fr 1fr 0.9fr 1fr;
      }
      .sep {
        color: #cbd5e1;
        margin: 0 6px;
      }
      .table a {
        color: var(--brand-color);
        text-decoration: none;
      }
      .table a:hover {
        text-decoration: underline;
      }
      form.row label {
        font-size: 12px;
        color: var(--muted);
        display: block;
        margin-bottom: 6px;
      }
    `,
  ],
})
export class CuentasListPageComponent {
  items: CuentaListItem[] = [];
  pagina = 1;
  tamano = 10;
  private search$ = new Subject<string>();
  q = '';
  activo = true;
  clientesActivos = true;
  clienteInactivo = false;
  isUser = false;
  private initializedByInactivity = false;

  constructor(
    private cuentas: CuentasService,
    private router: Router,
    private store: Store
  ) {
    this.search$.pipe(debounceTime(250), distinctUntilChanged()).subscribe(() => this.load());
    this.load();

    // Determinar rol para deshabilitar acciones en User + cliente inactivo
    this.store.select(authFeature.selectCodeRol).subscribe((code) => {
      this.isUser = (code || '').toLowerCase() === 'user';
    });

    // Escuchar estado del cliente del usuario actual para desactivar acciones
    this.store.select(authFeature.selectClienteActivo).subscribe((act) => {
      this.clienteInactivo = act === false;
      if (this.clienteInactivo && !this.initializedByInactivity) {
        // Por defecto, mostrar todas (no filtrar por activos) si el usuario está inactivo
        this.activo = false;
        this.clientesActivos = false;
        this.initializedByInactivity = true;
        this.load();
      }
    });
  }

  load() {
    this.cuentas
      .list(this.pagina, this.tamano, {
        q: this.q,
        activo: this.activo,
        clientesActivos: this.clientesActivos,
      })
      .subscribe((res) => (this.items = res.items || []));
  }
  prev() {
    if (this.pagina > 1) {
      this.pagina--;
      this.load();
    }
  }
  next() {
    this.pagina++;
    this.load();
  }

  openNew() {
    this.router.navigateByUrl('/cuentas/nuevo');
  }
  openEdit(c: CuentaListItem, ev: Event) {
    ev.preventDefault();
    this.router.navigateByUrl(`/cuentas/${c.cuentaId}/editar`);
  }
  confirmDelete(c: CuentaListItem, ev: Event) {
    ev.preventDefault();
    Swal.fire({
      icon: 'warning',
      title: 'Eliminar cuenta',
      text: `¿Eliminar ${c.numeroCuenta}?`,
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
    }).then((r) => {
      if (r.isConfirmed) this.cuentas.delete(c.cuentaId).subscribe(() => this.load());
    });
  }
  goDetail(c: CuentaListItem, ev: Event) {
    ev.preventDefault();
    this.router.navigateByUrl(`/cuentas/${c.cuentaId}`);
  }
  onSearch(ev: Event) {
    this.q = (ev.target as HTMLInputElement).value || '';
    this.search$.next(this.q);
  }
  toggleActivo(ev: Event) {
    this.activo = (ev.target as HTMLInputElement).checked;
    this.load();
  }
  toggleClientesActivos(ev: Event) {
    this.clientesActivos = (ev.target as HTMLInputElement).checked;
    this.load();
  }
}
