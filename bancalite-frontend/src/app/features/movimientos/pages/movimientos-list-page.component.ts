import { Component } from '@angular/core';
import { MovimientosService } from '../../../core/services/movimientos.service';
import { MovimientoItem } from '../../../shared/models/cuentas.models';
import { CatalogosService, CatalogoItem } from '../../../core/services/catalogos.service';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { Router, ActivatedRoute } from '@angular/router';
import { CuentasService } from '../../../core/services/cuentas.service';
import { CuentaListItem } from '../../../shared/models/cuentas.models';

@Component({
  template: `
    <section class="container">
      <div class="row">
        <h1 class="brand">Movimientos</h1>
        <span class="spacer"></span>
        <button class="btn btn-primary btn-lg" (click)="openNew()">Nuevo</button>
      </div>

      <div class="row filters" style="gap:12px; align-items:center; margin: 12px 0; flex-wrap: wrap;">
        <div class="suggest-wrap input-icon" style="width: 320px;">
          <input class="input" placeholder="Buscar por número, titular o cédula" [value]="numeroCuenta" (input)="onSearchCuenta($event)" />
          <div class="suggest" *ngIf="sugerencias.length>0">
            <div class="item" *ngFor="let s of sugerencias" (click)="seleccionarCuenta(s)">
              {{ s.numeroCuenta }} — {{ s.clienteNombre }} ({{ s.tipoCuentaNombre }})
            </div>
          </div>
        </div>
        <input class="input" type="date" [value]="desde || ''" (change)="onDesdeChange($event)" />
        <input class="input" type="date" [value]="hasta || ''" (change)="onHastaChange($event)" />
        <select class="input select" style="width:180px;" [value]="tipoFiltro" (change)="onTipoChange($event)">
          <option value="">Todos</option>
          <option *ngFor="let t of tipos" [value]="t.codigo">{{ t.nombre }}</option>
        </select>
        <input class="input" placeholder="Buscar descripción" style="width:260px;" [value]="q" (input)="onSearch($event)" />
      </div>

      <div *ngIf="!numeroCuenta" style="margin:12px 0; color: var(--muted);">Ingrese un número de cuenta para listar movimientos.</div>

      <div class="table movs" *ngIf="numeroCuenta">
        <div class="thead">
          <div>Fecha</div>
          <div>Tipo</div>
          <div>Monto</div>
          <div>Saldo previo</div>
          <div>Saldo posterior</div>
          <div>Descripción</div>
        </div>
        <div class="rowt" *ngFor="let m of vista">
          <div>{{ m.fecha | date:'short' }}</div>
          <div>{{ m.tipoCodigo }}</div>
          <div>{{ m.monto | number:'1.2-2' }}</div>
          <div>{{ m.saldoPrevio | number:'1.2-2' }}</div>
          <div>{{ m.saldoPosterior | number:'1.2-2' }}</div>
          <div>{{ m.descripcion || '-' }}</div>
        </div>
        <div class="rowt" *ngIf="vista.length===0" style="justify-content:center; color:#888;">Sin resultados</div>
      </div>
    </section>
  `,
  styles: [
    `
      .movs .thead, .movs .rowt { grid-template-columns: 1.2fr .6fr .8fr .9fr .9fr 1.6fr; }
      .input-icon { position: relative; width: 100%; }
      .suggest-wrap { position: relative; }
      .suggest { position: absolute; top: calc(100% + 6px); left: 0; right: 0; z-index: 3000; background:#fff; border:1px solid var(--border); border-radius: 8px; box-shadow: 0 6px 16px rgba(0,0,0,.08); max-height: 260px; overflow: auto; }
      .suggest .item { padding: 8px 10px; cursor: pointer; }
      .suggest .item:hover { background:#f8fafc; }
      .input-icon .icon { position: absolute; right: -30px; top: 50%; transform: translateY(-50%); width: 18px; height: 18px; pointer-events: none; }
      .input-icon > .input { padding-right: 32px; }
    `
  ]
})
export class MovimientosListPageComponent {
  items: MovimientoItem[] = [];
  vista: MovimientoItem[] = [];
  tipos: CatalogoItem[] = [];
  numeroCuenta = '';
  desde: string | null = null;
  hasta: string | null = null;
  tipoFiltro: string = '';
  q = '';

  private search$ = new Subject<void>();
  sugerencias: CuentaListItem[] = [];
  private suggest$ = new Subject<string>();

  constructor(private api: MovimientosService, private cat: CatalogosService, private router: Router, private route: ActivatedRoute, private cuentas: CuentasService) {
    this.cat.tiposMovimiento().subscribe(list => (this.tipos = list));
    this.search$.pipe(debounceTime(250), distinctUntilChanged()).subscribe(() => this.load());
    const qNumero = this.route.snapshot.queryParamMap.get('numeroCuenta');
    if (qNumero) { this.numeroCuenta = qNumero; this.load(); }

    // Sugerencias de cuentas: buscar por número, titular y documento
    this.suggest$.pipe(
      debounceTime(250),
      distinctUntilChanged(),
      switchMap(q => {
        const term = (q || '').trim();
        if (!term) { this.sugerencias = []; }
        return this.cuentas.list(1, 5, { q: term, activo: true, clientesActivos: true });
      })
    ).subscribe(res => this.sugerencias = res?.items || []);
  }

  openNew() {
    const q = this.numeroCuenta ? `?numeroCuenta=${encodeURIComponent(this.numeroCuenta)}` : '';
    this.router.navigateByUrl(`/movimientos/nuevo${q}`);
  }

  onNumeroChange(ev: Event) { this.numeroCuenta = (ev.target as HTMLInputElement).value || ''; this.search$.next(); }
  onSearchCuenta(ev: Event) {
    this.numeroCuenta = (ev.target as HTMLInputElement).value || '';
    this.suggest$.next(this.numeroCuenta);
    // no llamamos load hasta seleccionar o presionar enter; se mantiene debounced por onNumeroChange si se usara
  }
  onDesdeChange(ev: Event) {
    this.desde = (ev.target as HTMLInputElement).value || null;
    this.load();
  }
  onHastaChange(ev: Event) {
    this.hasta = (ev.target as HTMLInputElement).value || null;
    this.load();
  }
  onTipoChange(ev: Event) { this.tipoFiltro = (ev.target as HTMLSelectElement).value || ''; this.applyFilters(); }
  onSearch(ev: Event) { this.q = (ev.target as HTMLInputElement).value || ''; this.applyFilters(); }

  load() {
    if (!this.numeroCuenta) { this.items = []; this.vista = []; return; }
    const desdeIso = this.desde ? new Date(this.desde).toISOString() : undefined;
    const hastaIso = this.hasta ? new Date(this.hasta).toISOString() : undefined;
    if (this.desde && this.hasta && new Date(this.desde) > new Date(this.hasta)) { this.items = []; this.vista = []; return; }
    this.api.list(this.numeroCuenta, desdeIso, hastaIso).subscribe(list => { this.items = list || []; this.applyFilters(); });
  }

  private applyFilters() {
    let data = [...(this.items || [])];
    if (this.tipoFiltro) data = data.filter(m => m.tipoCodigo === this.tipoFiltro);
    if (this.q) {
      const v = this.q.toLowerCase();
      data = data.filter(m => (m.descripcion || '').toLowerCase().includes(v));
    }
    // Orden descendente por fecha para vista
    this.vista = data.sort((a, b) => new Date(b.fecha).getTime() - new Date(a.fecha).getTime());
  }

  seleccionarCuenta(s: CuentaListItem) {
    this.numeroCuenta = s.numeroCuenta;
    this.sugerencias = [];
    this.search$.next();
  }
}
