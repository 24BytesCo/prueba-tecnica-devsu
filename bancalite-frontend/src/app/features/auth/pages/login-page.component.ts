import { Component } from '@angular/core';

@Component({
  template: `
    <section class="container" style="max-width:420px; margin:48px auto;">
      <h1 class="brand">Ingresar</h1>
      <div style="display:flex; flex-direction:column; gap:12px; margin-top:12px;">
        <input placeholder="Email" style="padding:10px;" />
        <input placeholder="Contraseña" type="password" style="padding:10px;" />
        <button class="btn-primary">Entrar</button>
      </div>
    </section>
  `,
  styles: [`.btn-primary{background:var(--accent); border:1px solid #e5cc18; padding:10px;}`]
})
export class LoginPageComponent {}

