import { Component } from '@angular/core';

@Component({
  selector: 'app-sidebar',
  template: `
    <nav class="sidebar">
      <a routerLink="/clientes" routerLinkActive="active">Clientes</a>
      <a routerLink="/cuentas" routerLinkActive="active">Cuentas</a>
      <a routerLink="/movimientos" routerLinkActive="active">Movimientos</a>
      <a routerLink="/reportes" routerLinkActive="active">Reportes</a>
    </nav>
  `,
  styles: [
    `
      .sidebar { width: 200px; background: var(--sidebar-bg); border-right:1px solid #eee; padding: 12px; box-sizing: border-box; height: calc(100vh - 56px); position: sticky; top:56px; }
      a { display:block; padding:10px 8px; border:1px solid #eee; margin-bottom:10px; color:#444; text-decoration:none; border-radius:3px; }
      a.active { background:#fff; font-weight:600; }
    `
  ]
})
export class SidebarComponent {}

