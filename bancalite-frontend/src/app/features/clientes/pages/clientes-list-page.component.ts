import { Component } from '@angular/core';

@Component({
  template: `
    <section class="container">
      <div class="row">
        <h1 class="brand">Clientes</h1>
        <span class="spacer"></span>
        <button class="btn-primary">Nuevo</button>
      </div>
      <div class="row" style="margin: 12px 0;">
        <input placeholder="Buscar" style="padding:8px; width: 280px;" />
      </div>
      <div style="border:1px solid #eee; border-radius:4px; height:300px; background:#fff;"></div>
    </section>
  `,
  styles: [
    `
      .btn-primary { background: var(--accent); border: 1px solid #e5cc18; padding: 8px 16px; border-radius: 4px; cursor:pointer; }
    `
  ]
})
export class ClientesListPageComponent {}

