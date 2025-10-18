import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CuentasService } from '../../../core/services/cuentas.service';
import { CatalogosService, CatalogoItem } from '../../../core/services/catalogos.service';
import { ClientesService } from '../../../core/services/clientes.service';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';

@Component({
  template: `
    <section class="container wrapper">
      <div class="card">
        <h1 class="title">{{ isEdit ? 'Editar Cuenta' : 'Abrir Nueva Cuenta' }}</h1>
        <p class="subtitle" *ngIf="!isEdit">Completa los siguientes datos para crear la cuenta.</p>
        <form [formGroup]="form" (ngSubmit)="save()" class="grid">
          <label>Número de</label>
          <input class="input" formControlName="numeroCuenta" placeholder="Opcional, se autogenerará si se deja vacío." />

          <label>Tipo Cuenta</label>
          <select class="input select" formControlName="tipoCuentaId">
            <option value="" disabled selected>Seleccione…</option>
            <option *ngFor="let t of tiposCuenta" [value]="t.id">{{ t.nombre }}</option>
          </select>

          <label>Cliente</label>
          <div class="suggest-wrap input-icon">
            <input class="input" formControlName="clienteNombre" (input)="onSearchCliente($event)" [disabled]="isEdit" placeholder="Buscar cliente (nombre/doc)" />
            <svg class="icon" viewBox="0 0 24 24"><path fill="#64748b" d="M15.5 14h-.79l-.28-.27a6.471 6.471 0 0 0 1.48-5.34C15.18 5.59 12.6 3 9.5 3S3.82 5.59 3.82 8.39s2.58 5.39 5.68 5.39c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l4.25 4.25c.41.41 1.08.41 1.49 0 .41-.41.41-1.08 0-1.49L15.5 14Zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14Z"/></svg>
            <div class="suggest" *ngIf="!isEdit && sugerencias.length>0">
              <div class="item" *ngFor="let s of sugerencias" (click)="seleccionarCliente(s)">
                {{ s.nombres }} {{ s.apellidos }} — {{ s.numeroDocumento }}
              </div>
            </div>
          </div>

          <label>Saldo Inicial</label>
          <input class="input" type="number" formControlName="saldoInicial" />

          <div class="actions">
            <button class="btn-primary" type="submit" [disabled]="form.invalid">Guardar</button>
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
      /* Asegurar que todas las celdas de la 2da columna ocupen 100% */
      .grid > *:nth-child(2n) { width: 100%; }
      .grid .actions { grid-column: 2 / span 1; display:flex; gap:8px; margin-top: 6px; }
      .card .input { width: 100%; box-sizing: border-box; display:block; }
      .input-icon { position: relative; width: 100%; }
      .suggest-wrap { position: relative; }
      .suggest { position: absolute; top: calc(100% + 6px); left: 0; right: 0; z-index: 3000; background:#fff; border:1px solid var(--border); border-radius: 8px; box-shadow: 0 6px 16px rgba(0,0,0,.08); max-height: 260px; overflow: auto; }
      .suggest .item { padding: 8px 10px; cursor: pointer; }
      .suggest .item:hover { background:#f8fafc; }
      .input-icon .icon { position: absolute; right: 10px; top: 50%; transform: translateY(-50%); width: 18px; height: 18px; pointer-events: none; }
      .input-icon > .input { padding-right: 32px; }
      /* Forzar selects a igual ancho que inputs */
      select.input { width: 100%; box-sizing: border-box; display:block; }
    `
  ]
})
export class CuentasFormPageComponent {
  isEdit = false;
  id: string | null = null;
  form!: FormGroup;
  tiposCuenta: CatalogoItem[] = [];
  sugerencias: any[] = [];
  private search$ = new Subject<string>();

  constructor(private fb: FormBuilder, private route: ActivatedRoute, private router: Router, private api: CuentasService, private cat: CatalogosService, private clientes: ClientesService) {
    this.form = this.fb.group({
      numeroCuenta: [''],
      tipoCuentaId: ['', Validators.required],
      clienteId: ['', Validators.required],
      clienteNombre: [''],
      saldoInicial: [0, [Validators.required, Validators.min(0)]],
      estado: ['Activa']
    });

    this.cat.tiposCuenta().subscribe(list => (this.tiposCuenta = list));

    this.search$
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        switchMap(q => this.clientes.list(1, 5, q, true))
      )
      .subscribe(res => (this.sugerencias = res.items || []));

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true; this.id = id;
      this.api.get(id).subscribe((d: any) => {
        this.form.patchValue({
          numeroCuenta: d.numeroCuenta,
          tipoCuentaId: d.tipoCuentaId,
          clienteId: d.clienteId,
          clienteNombre: d.clienteNombre || '',
          estado: d.estado
        });
        this.form.get('saldoInicial')?.disable({ emitEvent: false });
        this.form.get('clienteNombre')?.disable({ emitEvent: false });
      });
    }
  }

  save() {
    if (this.form.invalid) return;
    const raw = this.form.getRawValue() as any;
    if (!this.isEdit) {
      const payload = {
        numeroCuenta: raw.numeroCuenta || '',
        tipoCuentaId: raw.tipoCuentaId,
        clienteId: raw.clienteId,
        saldoInicial: Number(raw.saldoInicial || 0)
      };
      this.api.create(payload).subscribe(() => this.router.navigateByUrl('/cuentas'));
    } else if (this.id) {
      const put = { numeroCuenta: raw.numeroCuenta, tipoCuentaId: raw.tipoCuentaId, clienteId: raw.clienteId };
      const estado = raw.estado;
      this.api.update(this.id, put).subscribe(() => {
        if (estado) {
          this.api.patchEstado(this.id!, { estado }).subscribe(() => this.router.navigateByUrl('/cuentas'));
        } else {
          this.router.navigateByUrl('/cuentas');
        }
      });
    }
  }

  cancel() { this.router.navigateByUrl('/cuentas'); }

  onSearchCliente(ev: Event) {
    if (this.isEdit || this.form.get('clienteNombre')?.disabled) return;
    const v = (ev.target as HTMLInputElement).value || '';
    this.form.get('clienteId')?.setValue('');
    this.search$.next(v);
  }
  seleccionarCliente(s: any) {
    this.form.get('clienteId')?.setValue(s.clienteId);
    this.form.get('clienteNombre')?.setValue(`${s.nombres} ${s.apellidos} — ${s.numeroDocumento}`);
    this.sugerencias = [];
  }
}
