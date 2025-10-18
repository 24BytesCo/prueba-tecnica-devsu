import { Component } from '@angular/core';
import { Router } from '@angular/router';
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
        <button class="btn btn-primary btn-lg" (click)="openNew()">Nuevo</button>
      </div>
      <div class="row" style="margin: 12px 0;">
        <div class="input-icon">
          <input class="input" (input)="onSearch($event)" placeholder="Buscar por nombre o documento" style="width:420px;" />
          <svg class="icon" viewBox="0 0 24 24"><path fill="#64748b" d="M15.5 14h-.79l-.28-.27a6.471 6.471 0 0 0 1.48-5.34C15.18 5.59 12.6 3 9.5 3S3.82 5.59 3.82 8.39s2.58 5.39 5.68 5.39c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l4.25 4.25c.41.41 1.08.41 1.49 0 .41-.41.41-1.08 0-1.49L15.5 14Zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14Z"/></svg>
        </div>
        <label style="display:flex; align-items:center; gap:6px; font-size:13px; color: var(--muted);">
          <input type="checkbox" [checked]="soloActivos" (change)="toggleSoloActivos($event)" /> Clientes activos
        </label>
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
      <!-- Se reemplaza el modal por páginas dedicadas de creación/edición -->
    </section>
  `,
  styles: [
    `
      
      .table { border:1px solid #eee; border-radius:4px; background:#fff; }
      .thead, .rowt { display:grid; grid-template-columns: 2fr 1fr 2fr 1fr 1.2fr; gap:8px; padding:10px; align-items:center; }
      .thead { background:#f7f7f7; font-weight:600; border-bottom:1px solid #eee; }
      .rowt { border-bottom:1px solid #f2f2f2; }
      .sep { color:#ccc; margin: 0 6px; }
      .form .row { display:flex; flex-direction:column; margin:8px 0; }
      .form label { font-size:12px; color:#666; margin-bottom:4px; }
      .form input { padding:8px; }
      .input-icon { position: relative; width: 100%; }
      .input-icon .icon { position: absolute; right: 10px; top: 50%; transform: translateY(-50%); width: 18px; height: 18px; pointer-events: none; }
      .input-icon > .input { padding-right: 32px; }
    `
  ]
})
export class ClientesListPageComponent {
  items: ClienteListItem[] = [];
  pagina = 1;
  tamano = 10;
  private search$ = new Subject<string>();
  private q = '';
  soloActivos = true;
  showModal = false;
  isEdit = false;
  editId: string | null = null;
  form: FormGroup;
  generos: CatalogoItem[] = [];
  tiposDoc: CatalogoItem[] = [];

  constructor(private api: ClientesService, private fb: FormBuilder, private cat: CatalogosService, private router: Router) {
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
    const estado = this.soloActivos ? true : false;
    this.api.list(this.pagina, this.tamano, this.q, estado).subscribe(res => (this.items = res.items || []));
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

  // Navegación a páginas dedicadas (sin modal)
  openNew() { this.router.navigateByUrl('/clientes/nuevo'); }
  openEdit(c: ClienteListItem, ev: Event) { ev.preventDefault(); this.router.navigateByUrl(`/clientes/${c.clienteId}/editar`); }

  toggleSoloActivos(ev: Event) {
    this.soloActivos = (ev.target as HTMLInputElement).checked;
    this.load();
  }
}
