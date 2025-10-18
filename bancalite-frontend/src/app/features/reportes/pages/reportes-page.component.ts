import { Component } from '@angular/core';
import { ReportesService } from '../../../core/services/reportes.service';
import { EstadoCuentaDto } from '../../../shared/models/reportes.models';
import { CuentasService } from '../../../core/services/cuentas.service';
import { ClientesService } from '../../../core/services/clientes.service';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { CuentaListItem } from '../../../shared/models/cuentas.models';
import { ClienteListItem } from '../../../shared/models/clientes.models';
import { Store } from '@ngrx/store';
import { authFeature } from '../../../core/state/auth/auth.reducer';

@Component({
  template: `
    <section class="container">
      <div class="row">
        <h1 class="brand">Estado de Cuenta</h1>
        <span class="spacer"></span>
        <button class="btn" (click)="descargarJson()" [disabled]="!puedeConsultar()">Descargar JSON</button>
        <button class="btn" (click)="descargarPdfBase64()" [disabled]="!puedeConsultar()">Descargar PDF (Base64)</button>
      </div>

      <div class="row filters" style="gap:12px; align-items:center; margin: 12px 0; flex-wrap: wrap;">
        <label style="display:flex; align-items:center; gap:6px; font-size:13px; color: var(--muted);">
          <input type="radio" name="modo" [checked]="modo==='cuenta'" (change)="setModo('cuenta')" /> Por cuenta
        </label>
        <label style="display:flex; align-items:center; gap:6px; font-size:13px; color: var(--muted);">
          <input type="radio" name="modo" [checked]="modo==='cliente'" (change)="setModo('cliente')" /> Por cliente
        </label>

        <div *ngIf="modo==='cuenta'" class="suggest-wrap input-icon" style="width: 320px;">
          <input class="input" placeholder="Buscar cuenta por número, titular o cédula" [value]="numeroCuenta" (input)="onBuscarCuenta($event)" />
          <div class="suggest" *ngIf="sugCuentas.length>0">
            <div class="item" *ngFor="let s of sugCuentas" (click)="seleccionarCuenta(s)">{{ s.numeroCuenta }} — {{ s.clienteNombre }} ({{ s.tipoCuentaNombre }})</div>
          </div>
        </div>

        <div *ngIf="modo==='cliente'" class="suggest-wrap input-icon" style="width: 320px;">
          <input class="input" placeholder="Buscar cliente por nombre o documento" [value]="clienteTexto" (input)="onBuscarCliente($event)" />
          <div class="suggest" *ngIf="sugClientes.length>0">
            <div class="item" *ngFor="let c of sugClientes" (click)="seleccionarCliente(c)">{{ c.nombres }} {{ c.apellidos }} — {{ c.numeroDocumento }}</div>
          </div>
        </div>

        <input class="input" type="date" [value]="desde || ''" (change)="onDesdeChange($event)" />
        <input class="input" type="date" [value]="hasta || ''" (change)="onHastaChange($event)" />
        <button class="btn btn-primary" (click)="consultar()" [disabled]="!puedeConsultar()">Ver</button>
      </div>

      <div *ngIf="data">
        <h3>Resumen</h3>

        <div class="summary-card">
          <!-- Datos principales -->
          <div class="kv">
            <div class="label">Cliente</div>
            <div class="value">{{ data.clienteNombre || '-' }}</div>
          </div>
          <div class="kv">
            <div class="label">Cuenta</div>
            <div class="value">{{ data.numeroCuenta || '-' }}</div>
          </div>
          <div class="kv">
            <div class="label">Desde</div>
            <div class="value">{{ data.desde | date:'shortDate' }}</div>
          </div>
          <div class="kv">
            <div class="label">Hasta</div>
            <div class="value">{{ data.hasta | date:'shortDate' }}</div>
          </div>

          <div class="divider"></div>

          <!-- Métricas -->
          <div class="metric good">
            <div class="metric-label">Créditos</div>
            <div class="metric-value">{{ data.totalCreditos | number:'1.2-2' }}</div>
          </div>
          <div class="metric warn">
            <div class="metric-label">Débitos</div>
            <div class="metric-value">{{ data.totalDebitos | number:'1.2-2' }}</div>
          </div>
          <div class="metric neutral">
            <div class="metric-label">Saldo Inicial</div>
            <div class="metric-value">{{ data.saldoInicial | number:'1.2-2' }}</div>
          </div>
          <div class="metric neutral">
            <div class="metric-label">Saldo Final</div>
            <div class="metric-value">{{ data.saldoFinal | number:'1.2-2' }}</div>
          </div>
        </div>

        <h3>Movimientos</h3>
        <div class="table movs">
          <div class="thead"><div>Fecha</div><div>Número</div><div>Tipo</div><div>Monto</div><div>Saldo previo</div><div>Saldo posterior</div><div>Descripción</div></div>
          <div class="rowt" *ngFor="let m of data.movimientos">
            <div>{{ m.fecha | date:'short' }}</div>
            <div>{{ m.numeroCuenta }}</div>
            <div>{{ m.tipoCodigo }}</div>
            <div>{{ m.monto | number:'1.2-2' }}</div>
            <div>{{ m.saldoPrevio | number:'1.2-2' }}</div>
            <div>{{ m.saldoPosterior | number:'1.2-2' }}</div>
            <div>{{ m.descripcion || '-' }}</div>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [
    `
      .movs .thead, .movs .rowt { grid-template-columns: 1.1fr 1fr .6fr .8fr .9fr .9fr 1.6fr; }
      .input-icon { position: relative; width: 100%; }
      .suggest-wrap { position: relative; }
      .suggest { position: absolute; top: calc(100% + 6px); left: 0; right: 0; z-index: 3000; background:#fff; border:1px solid var(--border); border-radius: 8px; box-shadow: 0 6px 16px rgba(0,0,0,.08); max-height: 260px; overflow: auto; }
      .suggest .item { padding: 8px 10px; cursor: pointer; }
      .suggest .item:hover { background:#f8fafc; }
      .input-icon .icon { position: absolute; right: 10px; top: 50%; transform: translateY(-50%); width: 18px; height: 18px; pointer-events: none; }
      .input-icon > .input { padding-right: 32px; }

      /* Resumen en formato tarjeta */
      .summary-card {
        margin: 8px 0 16px 0;
        background: #fff;
        border: 1px solid var(--border);
        border-radius: 12px;
        padding: 16px;
        display: grid;
        grid-template-columns: repeat(4, minmax(0, 1fr));
        gap: 14px 18px;
      }
      .summary-card .kv { display: flex; flex-direction: column; }
      .summary-card .kv .label { font-size: 12px; color: var(--muted); font-weight: 600; margin-bottom: 4px; }
      .summary-card .kv .value { font-size: 16px; }
      .summary-card .divider { grid-column: 1 / -1; height: 1px; background: var(--border); margin: 4px 0; }
      .summary-card .metric { background: #f8fafc; border: 1px solid var(--border); border-radius: 10px; padding: 12px; display: flex; flex-direction: column; align-items: flex-start; }
      .summary-card .metric .metric-label { font-size: 12px; color: var(--muted); margin-bottom: 6px; font-weight: 600; }
      .summary-card .metric .metric-value { font-size: 18px; font-weight: 700; }
      .summary-card .metric.good .metric-value { color: #0d9488; }
      .summary-card .metric.warn .metric-value { color: #b91c1c; }
      .summary-card .metric.neutral .metric-value { color: #111827; }

      @media (max-width: 960px) {
        .summary-card { grid-template-columns: repeat(2, minmax(0, 1fr)); }
      }
      @media (max-width: 560px) {
        .summary-card { grid-template-columns: 1fr; }
      }
    `
  ]
})
export class ReportesPageComponent {
  modo: 'cuenta' | 'cliente' = 'cuenta';
  numeroCuenta = '';
  clienteId: string | null = null;
  clienteTexto = '';
  desde = new Date(Date.now() - 6 * 24 * 3600 * 1000).toISOString().substring(0, 10);
  hasta = new Date().toISOString().substring(0, 10);
  data: EstadoCuentaDto | null = null;

  sugCuentas: CuentaListItem[] = [];
  sugClientes: ClienteListItem[] = [];
  private cuentas$ = new Subject<string>();
  private clientes$ = new Subject<string>();
  private isAdmin = false;

  constructor(private reportes: ReportesService, private cuentas: CuentasService, private clientes: ClientesService, private store: Store) {
    this.cuentas$
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        switchMap(q => {
          const term = (q || '').trim();
          // Para Admin y User: mostrar cuentas sin filtrar por estado (incluye inactivas)
          const filtros = { q: term } as any;
          return this.cuentas.list(1, 5, filtros);
        })
      )
      .subscribe(res => this.sugCuentas = res?.items || []);

    this.clientes$
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        // Admin: listar clientes sin filtrar estado; User: servicio devuelve su propio cliente
        switchMap(q => this.clientes.list(1, 5, q))
      )
      .subscribe(res => this.sugClientes = res?.items || []);
    // Rol: si es User y solo tiene una cuenta, preseleccionar por defecto (modo cuenta)
    this.store.select(authFeature.selectCodeRol).subscribe(code => {
      this.isAdmin = (code || '').toLowerCase() === 'admin';
      if (!this.isAdmin && this.modo === 'cuenta' && !this.numeroCuenta) {
        this.cuentas.list(1, 2, {}).subscribe(p => {
          if (p?.total === 1 && p.items?.length === 1) {
            this.numeroCuenta = p.items[0].numeroCuenta;
            // Mostrar por defecto
            this.consultar();
          }
        });
      }
    });
  }

  setModo(m: 'cuenta' | 'cliente') { this.modo = m; this.resetSeleccion(); }
  resetSeleccion() { this.numeroCuenta = ''; this.clienteId = null; this.clienteTexto=''; this.data = null; this.sugCuentas=[]; this.sugClientes=[]; }
  puedeConsultar() { return (this.modo==='cuenta' && !!this.numeroCuenta) || (this.modo==='cliente' && !!this.clienteId); }

  onBuscarCuenta(ev: Event) { const v = (ev.target as HTMLInputElement).value || ''; this.numeroCuenta = v; this.cuentas$.next(v); }
  seleccionarCuenta(s: CuentaListItem) { this.numeroCuenta = s.numeroCuenta; this.sugCuentas = []; }

  onBuscarCliente(ev: Event) { const v = (ev.target as HTMLInputElement).value || ''; this.clienteTexto = v; this.clientes$.next(v); }
  seleccionarCliente(c: ClienteListItem) { this.clienteId = c.clienteId; this.clienteTexto = `${c.nombres} ${c.apellidos} — ${c.numeroDocumento}`; this.sugClientes = []; }

  onDesdeChange(ev: Event) { this.desde = (ev.target as HTMLInputElement).value || this.desde; this.consultar(); }
  onHastaChange(ev: Event) { this.hasta = (ev.target as HTMLInputElement).value || this.hasta; this.consultar(); }

  consultar() {
    if (!this.puedeConsultar()) return;
    const dIso = new Date(this.desde).toISOString();
    const hIso = new Date(this.hasta).toISOString();
    const params = this.modo === 'cuenta' ? { numeroCuenta: this.numeroCuenta, desde: dIso, hasta: hIso } : { clienteId: this.clienteId!, desde: dIso, hasta: hIso };
    this.reportes.estadoCuenta(params as any).subscribe(res => this.data = res);
  }

  descargarJson() {
    if (!this.puedeConsultar()) return;
    const dIso = new Date(this.desde).toISOString();
    const hIso = new Date(this.hasta).toISOString();
    const params = this.modo === 'cuenta' ? { numeroCuenta: this.numeroCuenta, desde: dIso, hasta: hIso } : { clienteId: this.clienteId!, desde: dIso, hasta: hIso };
    this.reportes.estadoCuentaJsonBlob(params as any).subscribe(blob => {
      const fileName = `reporte-estado-cuenta-${new Date().toISOString().slice(0,10)}.json`;
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      setTimeout(() => URL.revokeObjectURL(link.href), 2000);
    });
  }

  descargarPdfBase64() {
    if (!this.puedeConsultar()) return;
    const dIso = new Date(this.desde).toISOString();
    const hIso = new Date(this.hasta).toISOString();
    const params = this.modo === 'cuenta' ? { numeroCuenta: this.numeroCuenta, desde: dIso, hasta: hIso } : { clienteId: this.clienteId!, desde: dIso, hasta: hIso };
    this.reportes.estadoCuentaPdfBase64(params as any).subscribe(({ fileName, contentType, base64 }) => {
      const bytes = atob(base64);
      const len = bytes.length;
      const buffer = new Uint8Array(len);
      for (let i = 0; i < len; i++) buffer[i] = bytes.charCodeAt(i);
      const blob = new Blob([buffer], { type: contentType });
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    });
  }
}
