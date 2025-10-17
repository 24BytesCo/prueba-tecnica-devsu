import { Component } from '@angular/core';

@Component({
  selector: 'app-header',
  template: `
    <header class="header">
      <div class="brand">BANCO</div>
    </header>
  `,
  styles: [
    `
      .header { height: 56px; display:flex; align-items:center; padding:0 16px; border-bottom:1px solid #eee; }
      .brand { font-weight: 700; font-size: 22px; color: var(--brand-color); letter-spacing: 1px; }
    `
  ]
})
export class HeaderComponent {}

