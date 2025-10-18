import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CatalogosService, CatalogoItem } from '../../../core/services/catalogos.service';
import { MovimientosService } from '../../../core/services/movimientos.service';
import { CuentasService } from '../../../core/services/cuentas.service';
import { MovimientoCreateForm } from '../../../shared/models/movimientos.models';
import Swal from 'sweetalert2';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { CuentaListItem } from '../../../shared/models/cuentas.models';
import { Store } from '@ngrx/store';
import { authFeature } from '../../../core/state/auth/auth.reducer';

@Component({
  template: `
    <section class="container wrapper">
      <div class="card">
        <h1 class="title">Registrar Movimiento</h1>
        <p class="subtitle">Crédito o débito con validaciones de negocio.</p>
        <form [formGroup]="form" (ngSubmit)="save()" class="grid">
          <label>Número de Cuenta</label>
          <div class="suggest-wrap input-icon">
            <input class="input" formControlName="numeroCuenta" (input)="onSearchCuenta($event)" placeholder="Buscar por número, titular o cédula" />
            <div class="suggest" *ngIf="sugerencias.length>0">
              <div class="item" *ngFor="let s of sugerencias" (click)="seleccionarCuenta(s)">
                {{ s.numeroCuenta }} — {{ s.clienteNombre }} ({{ s.tipoCuentaNombre }})
              </div>
            </div>
          </div>

          <label>Tipo</label>
          <select class="input select" formControlName="tipoCodigo">
            <option value="" disabled selected>Seleccione…</option>
            <option *ngFor="let t of tipos" [value]="t.codigo">{{ t.nombre }}</option>
          </select>

          <label>Monto</label>
          <input class="input" type="number" step="0.01" formControlName="monto" />

          <label>Descripción</label>
          <input class="input" formControlName="descripcion" placeholder="Opcional" />

          <label>Idempotency Key</label>
          <input class="input" formControlName="idempotencyKey" placeholder="Opcional para evitar duplicados" />

          <div class="actions">
            <button class="btn-primary" type="submit" [disabled]="form.invalid || clienteInactivo">Guardar</button>
            <button class="btn" type="button" (click)="cancel()">Cancelar</button>
          </div>
        </form>
      </div>
    </section>
  `,
  styles: [
    `
      .wrapper { display:flex; justify-content:center; }
      .card { width: 720px; max-width: 92vw; background:#fff; border:1px solid var(--border); border-radius:16px; padding:24px; box-shadow: 0 10px 30px rgba(0,0,0,.06); }
      .title { color: var(--brand-color); margin: 0 0 6px 0; }
      .subtitle { margin: 0 0 18px 0; color: var(--muted); }
      .grid { display:grid; grid-template-columns: 200px minmax(0,1fr); gap: 14px 16px; align-items:center; }
      .grid > *:nth-child(2n) { width: 100%; }
      .grid .actions { grid-column: 2 / span 1; display:flex; gap:8px; margin-top: 6px; }
      select.input { width: 100%; box-sizing: border-box; display:block; }
      .input-icon { position: relative; width: 100%; }
      .suggest-wrap { position: relative; }
      .suggest { position: absolute; top: calc(100% + 6px); left: 0; right: 0; z-index: 3000; background:#fff; border:1px solid var(--border); border-radius: 8px; box-shadow: 0 6px 16px rgba(0,0,0,.08); max-height: 260px; overflow: auto; }
      .suggest .item { padding: 8px 10px; cursor: pointer; }
      .suggest .item:hover { background:#f8fafc; }
      .input-icon .icon { position: absolute; right: 10px; top: 50%; transform: translateY(-50%); width: 18px; height: 18px; pointer-events: none; }
      .input-icon > .input { padding-right: 32px; }
    `
  ]
})
export class MovimientosFormPageComponent {
  form: FormGroup;
  tipos: CatalogoItem[] = [];
  sugerencias: CuentaListItem[] = [];
  private search$ = new Subject<string>();
  clienteInactivo = false;
  isAdmin = false;

  constructor(private fb: FormBuilder, private route: ActivatedRoute, private router: Router, private cat: CatalogosService, private api: MovimientosService, private cuentas: CuentasService, private store: Store) {
    this.form = this.fb.group({
      numeroCuenta: ['', Validators.required],
      tipoCodigo: ['', Validators.required],
      monto: [0, [Validators.required, Validators.min(0.01)]],
      descripcion: [''],
      idempotencyKey: ['']
    });

    this.cat.tiposMovimiento().subscribe(list => (this.tipos = list));

    const numeroPrefill = this.route.snapshot.queryParamMap.get('numeroCuenta');
    if (numeroPrefill) this.form.patchValue({ numeroCuenta: numeroPrefill });
    // Si no viene por query y es User con una sola cuenta, preseleccionar
    this.store.select(authFeature.selectCodeRol).subscribe(code => {
      this.isAdmin = (code || '').toLowerCase() === 'admin';
      if (!this.isAdmin && !this.form.get('numeroCuenta')?.value) {
        this.cuentas.list(1, 2, {}).subscribe(p => {
          if (p?.total === 1 && p.items?.length === 1) {
            this.form.get('numeroCuenta')?.setValue(p.items[0].numeroCuenta);
          }
        });
      }
    });

    // Búsqueda reactiva de cuentas por número, titular o documento
    this.search$
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        switchMap(q => {
          const term = (q || '').trim();
          if (!term) { this.sugerencias = []; }
          const filtros = this.isAdmin ? { q: term, activo: true, clientesActivos: true } : { q: term } as any;
          return this.cuentas.list(1, 5, filtros);
        })
      )
      .subscribe(res => (this.sugerencias = res?.items || []));

    // Estado del cliente actual para desactivar Guardar
    this.store.select(authFeature.selectClienteActivo).subscribe(act => this.clienteInactivo = act === false);
    
    // Rol para decidir filtros en sugerencias (ya suscrito arriba para preselección)
  }

  save() {
    if (this.form.invalid) return;
    const raw = this.form.getRawValue();
    const payload: MovimientoCreateForm = {
      numeroCuenta: (raw.numeroCuenta || '').trim(),
      tipoCodigo: (raw.tipoCodigo || '').trim().toUpperCase(),
      monto: Number(raw.monto || 0),
      descripcion: raw.descripcion?.trim() || undefined,
      idempotencyKey: raw.idempotencyKey?.trim() || undefined
    };
    this.api.create(payload).subscribe({
      next: res => {
        const d: any = res.datos || {};
        const prev = typeof d.saldoPrevio === 'number' ? d.saldoPrevio.toFixed(2) : '—';
        const post = typeof d.saldoPosterior === 'number' ? d.saldoPosterior.toFixed(2) : '—';
        Swal.fire({ icon: 'success', title: 'Movimiento registrado', text: `Saldo: ${prev} → ${post}` })
          .then(() => {
            this.router.navigateByUrl(`/movimientos?numeroCuenta=${encodeURIComponent(payload.numeroCuenta)}`);
          });
      },
      error: err => {
        const msg = err?.error?.title || 'Ocurrió un error';
        const detail = err?.error?.detail || 'No se pudo registrar el movimiento.';
        Swal.fire({ icon: 'error', title: detail, text: msg });
      }
    });
  }

  cancel() { this.router.navigateByUrl('/movimientos'); }

  onSearchCuenta(ev: Event) {
    const v = (ev.target as HTMLInputElement).value || '';
    // el control ya actualiza numeroCuenta, aquí disparamos sugerencias
    this.search$.next(v);
  }

  seleccionarCuenta(s: CuentaListItem) {
    // al seleccionar, fijamos el número de cuenta y cerramos sugerencias
    this.form.get('numeroCuenta')?.setValue(s.numeroCuenta);
    this.sugerencias = [];
  }
}
