import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ClientesService } from '../../../core/services/clientes.service';
import Swal from 'sweetalert2';

@Component({
  template: `
    <section class="container">
      <h1 class="brand">{{ isEdit ? 'Editar Cliente' : 'Nuevo Cliente' }}</h1>

      <form [formGroup]="form" (ngSubmit)="save()" class="form">
        <div class="row">
          <label>Nombres</label>
          <input formControlName="nombres" />
        </div>
        <div class="row">
          <label>Apellidos</label>
          <input formControlName="apellidos" />
        </div>
        <div class="row">
          <label>Edad</label>
          <input formControlName="edad" type="number" />
        </div>
        <div class="row">
          <label>Género Id</label>
          <input formControlName="generoId" />
        </div>
        <div class="row">
          <label>Tipo Doc Id</label>
          <input formControlName="tipoDocumentoIdentidad" />
        </div>
        <div class="row">
          <label>Número Documento</label>
          <input formControlName="numeroDocumento" />
        </div>
        <div class="row">
          <label>Email</label>
          <input formControlName="email" type="email" />
        </div>
        <div class="row">
          <label>Password</label>
          <input formControlName="password" type="password" />
        </div>
        <div style="margin-top:12px;">
          <button class="btn-primary" type="submit" [disabled]="form.invalid">Guardar</button>
          <button type="button" (click)="cancel()">Cancelar</button>
        </div>
      </form>
    </section>
  `,
  styles: [
    `
      .form .row { display:flex; flex-direction:column; margin:8px 0; }
      label { font-size: 12px; color:#666; margin-bottom:4px; }
      input { padding:8px; }
      .btn-primary { background: var(--accent); border: 1px solid #e5cc18; padding: 8px 16px; border-radius: 4px; cursor:pointer; }
    `
  ]
})
export class ClientesFormPageComponent {
  isEdit = false;
  id: string | null = null;
  form!: FormGroup;

  constructor(private fb: FormBuilder, private route: ActivatedRoute, private router: Router, private api: ClientesService) {
    // Creamos el formulario aquí (ya tenemos fb inyectado)
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

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.id = id;
      // Inhabilitar edición del número de documento en modo edición
      this.form.get('numeroDocumento')?.disable({ emitEvent: false });
      // Para simplificar, no cargamos detalle completo, dejamos edición básica si se llegara con datos
    }
  }

  save() {
    if (this.form.invalid) return;
    const value = this.form.getRawValue() as any;
    const op = this.isEdit && this.id ? this.api.updatePut(this.id, value) : this.api.create(value);
    op.subscribe(() => {
      Swal.fire({ icon: 'success', title: 'OK', text: 'Guardado correctamente' }).then(() => this.router.navigateByUrl('/clientes'));
    });
  }

  cancel() { this.router.navigateByUrl('/clientes'); }
}
