import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CuentasService } from '../../../core/services/cuentas.service';
import { MovimientoItem } from '../../../shared/models/cuentas.models';

@Component({
  template: `
    <section class="container">
      <div class="row">
        <h1 class="brand">Detalle de Cuenta</h1>
        <span class="spacer"></span>
        <button class="btn" (click)="back()">Volver</button>
      </div>

      <div class="table" *ngIf="cuenta">
        <div class="thead">
          <div>Número</div>
          <div>Tipo</div>
          <div>Titular</div>
          <div>Saldo actual</div>
          <div>Estado</div>
        </div>
        <div class="rowt">
          <div>{{ cuenta.numeroCuenta }}</div>
          <div>{{ cuenta.tipoCuentaNombre }}</div>
          <div>{{ cuenta.clienteNombre }}</div>
          <div>{{ cuenta.saldoActual | number:'1.2-2' }}</div>
          <div>{{ cuenta.estado }}</div>
        </div>
      </div>

      <h3 style="margin-top:16px;">Movimientos recientes</h3>
      <div class="table movs">
        <div class="thead">
          <div>Fecha</div>
          <div>Tipo</div>
          <div>Monto</div>
          <div>Saldo previo</div>
          <div>Saldo posterior</div>
          <div>Descripción</div>
        </div>
        <div class="rowt" *ngFor="let m of movimientos">
          <div>{{ m.fecha | date:'short' }}</div>
          <div>{{ m.tipoCodigo }}</div>
          <div>{{ m.monto | number:'1.2-2' }}</div>
          <div>{{ m.saldoPrevio | number:'1.2-2' }}</div>
          <div>{{ m.saldoPosterior | number:'1.2-2' }}</div>
          <div>{{ m.descripcion || '-' }}</div>
        </div>
        <div class="rowt" *ngIf="movimientos.length===0" style="justify-content:center; color:#888;">Sin movimientos</div>
      </div>
    </section>
  `,
  styles: [
    `
      .movs .thead, .movs .rowt { grid-template-columns: 1.2fr .6fr .8fr .9fr .9fr 1.6fr; }
    `
  ]
})
export class CuentasDetailPageComponent {
  cuenta: any;
  movimientos: MovimientoItem[] = [];

  constructor(private route: ActivatedRoute, private router: Router, private api: CuentasService) {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.api.get(id).subscribe((d: any) => {
        this.cuenta = d;
        if (d?.numeroCuenta) {
          this.api.movimientos(d.numeroCuenta).subscribe(list => {
            // Mostramos últimos 10 por fecha descendente
            this.movimientos = [...(list || [])]
              .sort((a, b) => new Date(b.fecha).getTime() - new Date(a.fecha).getTime())
              .slice(0, 10);
          });
        }
      });
    }
  }

  back() { this.router.navigateByUrl('/cuentas'); }
}
