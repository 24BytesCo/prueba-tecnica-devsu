import { Component } from '@angular/core';
import { Store } from '@ngrx/store';
import { AuthActions } from '../../../core/state/auth/auth.actions';
import { selectProfile } from '../../../core/state/auth/auth.reducer';
import { Observable } from 'rxjs';
import { Profile } from '../../../shared/models/auth.models';

@Component({
  selector: 'app-header',
  template: `
    <header class="header">
      <div routerLink="/" class="brand">BANCO</div>
      <div class="right">
        <span class="user" *ngIf="(profile$ | async) as p">{{ p?.email || p?.nombreCompleto }}</span>
        <button class="btn-link" (click)="logout()">Cerrar sesión</button>
      </div>
    </header>
  `,
  styles: [
    `
      .header { height: 56px; display:flex; align-items:center; padding:0 16px; border-bottom:1px solid #eee; }
      .brand { font-weight: 700; font-size: 22px; color: var(--brand-color); letter-spacing: 1px; }
      .brand:hover { cursor: pointer; }
      .right { margin-left:auto; display:flex; align-items:center; gap:12px; }
      .btn-link { background: transparent; border: none; color: #1a3a74; cursor: pointer; font-weight: 600; }
      .btn-link:hover { text-decoration: underline; }
      .user { color:#666; }
    `
  ]
})
export class HeaderComponent {
  profile$!: Observable<Profile | null>;
  constructor(private store: Store) {
    // Mostramos el email/nombre en el header leyendo el selector del store
    this.profile$ = this.store.select(selectProfile);
  }
  logout() {
    // Disparamos el logout; los efectos se encargan de limpiar y navegar
    this.store.dispatch(AuthActions.logout());
  }
}
