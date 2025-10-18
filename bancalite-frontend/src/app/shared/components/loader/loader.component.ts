import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoaderService } from '../../../core/services/loader.service';

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="loader-overlay" *ngIf="loader.loading$ | async">
      <div class="spinner"></div>
    </div>
  `,
  styles: [`
    .loader-overlay { position: fixed; inset: 0; background: rgba(255,255,255,0.35); backdrop-filter: blur(1px); z-index: 9999; display:flex; align-items:center; justify-content:center; }
    .spinner { width: 36px; height: 36px; border: 3px solid rgba(0,0,0,0.1); border-top-color: var(--accent, #f6d21b); border-radius: 50%; animation: spin 0.8s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class LoaderComponent {
  constructor(public loader: LoaderService) {}
}

