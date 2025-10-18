import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ClientesService } from '../../../core/services/clientes.service';
import { CatalogosService, CatalogoItem } from '../../../core/services/catalogos.service';
import Swal from 'sweetalert2';

@Component({
  template: `
    <section class="container wrapper">
      <div class="card">
        <h1 class="title">{{ isEdit ? 'Editar Cliente' : 'Nuevo Cliente' }}</h1>
        <p class="subtitle">Completa los datos del cliente.</p>
        <form [formGroup]="form" (ngSubmit)="save()" class="grid">
          <label>Nombres</label>
          <input class="input" formControlName="nombres" />

          <label>Apellidos</label>
          <input class="input" formControlName="apellidos" />

          <label>Edad</label>
          <input class="input" formControlName="edad" type="number" />

          <label>Género</label>
          <select class="input select" formControlName="generoId">
            <option value="" disabled selected>Seleccione…</option>
            <option *ngFor="let g of generos" [value]="g.id">{{ g.nombre }}</option>
          </select>

          <label>Tipo Documento</label>
          <select class="input select" formControlName="tipoDocumentoIdentidad">
            <option value="" disabled selected>Seleccione…</option>
            <option *ngFor="let t of tiposDoc" [value]="t.id">{{ t.nombre }}</option>
          </select>

          <label>Número Documento</label>
          <input class="input" formControlName="numeroDocumento" [readonly]="isEdit" />

          <label>Email</label>
          <input class="input" formControlName="email" type="email" />

          <label *ngIf="!isEdit">Password</label>
          <input class="input" *ngIf="!isEdit" formControlName="password" type="password" [readonly]="isEdit" />

          <div class="actions">
            <button class="btn btn-primary" type="submit" [disabled]="form.invalid">Guardar</button>
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
      .grid .actions { grid-column: 2 / span 1; display:flex; gap:8px; margin-top: 6px; }
      .card .input { width: 100%; box-sizing: border-box; display:block; }
      select.input { width: 100%; }
    `
  ]
})
export class ClientesFormPageComponent {
  isEdit = false;
  id: string | null = null;
  form!: FormGroup;
  generos: CatalogoItem[] = [];
  tiposDoc: CatalogoItem[] = [];

  constructor(private fb: FormBuilder, private route: ActivatedRoute, private router: Router, private api: ClientesService, private catalogos: CatalogosService) {
    this.form = this.fb.group({
      nombres: ['', [Validators.required, Validators.maxLength(120)]],
      apellidos: ['', [Validators.required, Validators.maxLength(120)]],
      edad: [18, [Validators.required, Validators.min(0)]],
      generoId: ['', Validators.required],
      tipoDocumentoIdentidad: ['', Validators.required],
      numeroDocumento: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.email]],
      password: ['']
    });

    // Cargar catálogos
    this.catalogos.generos().subscribe(list => (this.generos = list));
    this.catalogos.tiposDocumento().subscribe(list => (this.tiposDoc = list));

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.id = id;
      // Inhabilitar edición del número de documento y cargar detalle
      this.form.get('numeroDocumento')?.disable({ emitEvent: false });
      this.api.get(id).subscribe((d: any) => {
        this.form.patchValue({
          nombres: d?.nombres || '',
          apellidos: d?.apellidos || '',
          edad: d?.edad ?? 18,
          generoId: d?.generoId || '',
          tipoDocumentoIdentidad: d?.tipoDocumentoIdentidadId || '',
          numeroDocumento: d?.numeroDocumento || '',
          email: d?.email || ''
        });
      });
    }
  }

  save() {
    if (this.form.invalid) return;
    const value = this.form.getRawValue() as any; // incluye número doc si está deshabilitado
    const op = this.isEdit && this.id ? this.api.updatePut(this.id, value) : this.api.create(value);
    op.subscribe({
      next: () => this.router.navigateByUrl('/clientes'),
      error: err => {
        const detail = err?.error?.detail || err?.error?.title || 'No se pudo guardar.';
        const title = 'Error al guardar';
        Swal.fire({ icon: 'error', title, text: detail });
      }
    });
  }

  cancel() { this.router.navigateByUrl('/clientes'); }
}
