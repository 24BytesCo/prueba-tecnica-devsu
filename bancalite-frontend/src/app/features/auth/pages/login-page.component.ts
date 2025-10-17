import { Component } from '@angular/core';
import { Store } from '@ngrx/store';
import { AuthActions } from '../../../core/state/auth/auth.actions';

@Component({
  template: `
    <section class="container" style="max-width:420px; margin:48px auto;">
      <h1 class="brand">Ingresar</h1>
      <div style="display:flex; flex-direction:column; gap:12px; margin-top:12px;">
        <input [(ngModel)]="email" placeholder="Email" style="padding:10px;" />
        <input [(ngModel)]="password" placeholder="Contraseña" type="password" style="padding:10px;" />
        <button class="btn-primary" (click)="login()">Entrar</button>
      </div>
    </section>
  `,
  styles: [`.btn-primary{background:var(--accent); border:1px solid #e5cc18; padding:10px;}`]
})
export class LoginPageComponent {
  email = '';
  password = '';
  constructor(private store: Store) {}
  login() {
    // Disparamos la acción de login; los efectos hablarán con el backend
    this.store.dispatch(AuthActions.login({ email: this.email, password: this.password }));
  }
}
