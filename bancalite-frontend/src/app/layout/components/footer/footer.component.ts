import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  template: `<footer class="footer">Bancalite © 2025</footer>`,
  styles: [
    `
      .footer { padding: 12px 16px; border-top:1px solid #eee; color:#777; }
    `
  ]
})
export class FooterComponent {}

