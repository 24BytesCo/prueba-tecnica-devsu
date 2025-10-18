import { Component } from '@angular/core';
import { ClientesService } from '../../../core/services/clientes.service';
import { ClienteListItem } from '../../../shared/models/clientes.models';
import { debounceTime, distinctUntilChanged, Subject, switchMap } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CatalogosService, CatalogoItem } from '../../../core/services/catalogos.service';
import Swal from 'sweetalert2';

@Component({
  template: `
    <section class="container">
      <div class="row">
        <h1 class="brand">Clientes</h1>
        <span class="spacer"></span>
        <button class="btn-primary" (click)="openNew()">Nuevo</button>
      </div>
      <div class="row" style="margin: 12px 0;">
        <input (input)="onSearch($event)" placeholder="Buscar" style="padding:8px; width: 280px;" />
      </div>
      <div class="table">
        <div class="thead">
          <div>Nombre</div>
          <div>Documento</div>
          <div>Email</div>
          <div>Estado</div>
          <div style="text-align:right">Acciones</div>
        </div>
        <div class="rowt" *ngFor="let c of items">
          <div>{{ c.nombres }} {{ c.apellidos }}</div>
          <div>{{ c.numeroDocumento }}</div>
          <div>{{ c.email || '-' }}</div>
          <div>{{ c.estado ? 'Activo' : 'Inactivo' }}</div>
          <div style="text-align:right">
            <a href (click)="openEdit(c, $event)">Editar</a>
            <span class="sep">|</span>
            <a href (click)="remove(c, $event)">Eliminar</a>
          </div>
        </div>
      </div>
      <div class="row" style="margin-top:12px;">
        <button (click)="prev()" [disabled]="pagina===1">Anterior</button>
        <span style="margin:0 8px;">Página {{ pagina }}</span>
        <button (click)="next()" [disabled]="items.length<tamano">Siguiente</button>
      </div>
      <app-modal [open]="showModal" [title]="isEdit ? 'Editar Cliente' : 'Nuevo Cliente'" [closable]="false" (close)="closeModal()">
        <form [formGroup]="form" (ngSubmit)="saveForm()" class="form form-grid single">
          <div class="form-field">
            <label>Nombres</label>
            <input class="input" formControlName="nombres" />
            <small class="error" *ngIf="form.controls.nombres.touched && form.controls.nombres.invalid">Requerido (máx 120)</small>
          </div>
          <div class="form-field">
            <label>Apellidos</label>
            <input class="input" formControlName="apellidos" />
            <small class="error" *ngIf="form.controls.apellidos.touched && form.controls.apellidos.invalid">Requerido (máx 120)</small>
          </div>
          <div class="form-field">
            <label>Edad</label>
            <input class="input" formControlName="edad" type="number" />
          </div>
          <div class="form-field">
            <label>Género</label>
            <select class="input select" formControlName="generoId">
              <option value="" disabled selected>Seleccione…</option>
              <option *ngFor="let g of generos" [value]="g.id">{{ g.nombre }}</option>
            </select>
          </div>
          <div class="form-field">
            <label>Tipo de Documento</label>
            <select class="input select" formControlName="tipoDocumentoIdentidad">
              <option value="" disabled selected>Seleccione…</option>
              <option *ngFor="let t of tiposDoc" [value]="t.id">{{ t.nombre }}</option>
            </select>
          </div>
          <div class="form-field">
            <label>Número Documento</label>
            <input class="input" formControlName="numeroDocumento" />
          </div>
          <div class="form-field">
            <label>Email</label>
            <input class="input" formControlName="email" type="email" />
          </div>
          <div class="form-field">
            <label>Password</label>
            <input class="input" formControlName="password" type="password" />
          </div>
          <div style="margin-top:12px; display:flex; gap:8px;">
            <button class="btn btn-primary" type="submit" [disabled]="form.invalid">Guardar</button>
            <button class="btn" type="button" (click)="closeModal()">Cancelar</button>
          </div>
        </form>
      </app-modal>
    </section>
  `,
  styles: [
    `
      .btn-primary { background: var(--accent); border: 1px solid #e5cc18; padding: 8px 16px; border-radius: 4px; cursor:pointer; }
      .table { border:1px solid #eee; border-radius:4px; background:#fff; }
      .thead, .rowt { display:grid; grid-template-columns: 2fr 1fr 2fr 1fr 1.2fr; gap:8px; padding:10px; align-items:center; }
      .thead { background:#f7f7f7; font-weight:600; border-bottom:1px solid #eee; }
      .rowt { border-bottom:1px solid #f2f2f2; }
      .sep { color:#ccc; margin: 0 6px; }
      .form .row { display:flex; flex-direction:column; margin:8px 0; }
      .form label { font-size:12px; color:#666; margin-bottom:4px; }
      .form input { padding:8px; }
    `
  ]
})
export class ClientesListPageComponent {
  items: ClienteListItem[] = [];
  pagina = 1;
  tamano = 10;
  private search$ = new Subject<string>();
  private q = '';
  showModal = false;
  isEdit = false;
  editId: string | null = null;
  form: FormGroup;
  generos: CatalogoItem[] = [];
  tiposDoc: CatalogoItem[] = [];

  constructor(private api: ClientesService, private fb: FormBuilder, private cat: CatalogosService) {
    // Búsqueda reactiva con pequeño debounce
    this.search$
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        switchMap(q => this.api.list(this.pagina, this.tamano, q))
      )
      .subscribe(res => (this.items = res.items || []));
    this.load();

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

    // Cargar catálogos para selects
    this.cat.generos().subscribe(list => (this.generos = list));
    this.cat.tiposDocumento().subscribe(list => (this.tiposDoc = list));
  }

  load() {
    this.api.list(this.pagina, this.tamano, this.q).subscribe(res => (this.items = res.items || []));
  }
  onSearch(ev: Event) {
    const v = (ev.target as HTMLInputElement).value || '';
    this.q = v;
    this.search$.next(v);
  }
  prev() { if (this.pagina > 1) { this.pagina--; this.load(); } }
  next() { this.pagina++; this.load(); }
  remove(c: ClienteListItem, ev: Event) {
    ev.preventDefault();
    Swal.fire({ icon:'warning', title:'Eliminar', text:`¿Eliminar a ${c.nombres} ${c.apellidos}?`, showCancelButton:true, confirmButtonText:'Sí, eliminar' })
      .then(r => { if (r.isConfirmed) this.api.delete(c.clienteId).subscribe(() => this.load()); });
  }

  // Modal helpers
  openNew() {
    this.isEdit = false; this.editId = null;
    this.form.reset({ edad: 18 });
    // Habilitar edición del número documento para creación
    this.form.get('numeroDocumento')?.enable({ emitEvent: false });
    this.showModal = true;
  }
  openEdit(c: ClienteListItem, ev: Event) {
    ev.preventDefault();
    this.isEdit = true; this.editId = c.clienteId;
    // Inhabilitar edición del número documento en edición
    this.form.get('numeroDocumento')?.disable({ emitEvent: false });
    this.form.patchValue({
      nombres: c.nombres,
      apellidos: c.apellidos,
      edad: c.edad,
      generoId: (c as any).generoId || '',
      tipoDocumentoIdentidad: (c as any).tipoDocumentoIdentidadId || '',
      numeroDocumento: c.numeroDocumento,
      email: c.email || ''
    });
    this.showModal = true;
  }
  closeModal() { this.showModal = false; }
  saveForm() {
    if (this.form.invalid) return;
    // Incluir campos deshabilitados (numeroDocumento) al enviar
    const value = this.form.getRawValue() as any;
    const op = this.isEdit && this.editId ? this.api.updatePut(this.editId, value) : this.api.create(value);
    op.subscribe(() => { this.showModal = false; this.load(); });
  }
}
