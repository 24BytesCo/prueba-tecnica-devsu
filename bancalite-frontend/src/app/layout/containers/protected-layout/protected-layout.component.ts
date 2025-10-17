import { Component } from '@angular/core';

@Component({
  selector: 'app-protected-layout',
  template: `
    <app-header></app-header>
    <div class="layout">
      <app-sidebar></app-sidebar>
      <main class="content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [
    `
      :host { display:block; min-height:100vh; }
      .layout { display:flex; }
      .content { flex:1; padding: 16px; }
    `
  ]
})
export class ProtectedLayoutComponent {}

