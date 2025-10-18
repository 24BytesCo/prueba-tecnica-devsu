import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-modal',
  template: `
    <div class="overlay" *ngIf="open" (click)="onOverlayClick()">
      <div class="dialog" (click)="$event.stopPropagation()">
        <div class="hdr">
          <h3 class="ttl">{{ title }}</h3>
          <button type="button" class="close" (click)="close.emit()">×</button>
        </div>
        <div class="body">
          <ng-content></ng-content>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .overlay { position: fixed; inset: 0; background: rgba(17,24,39,.45); display:flex; align-items: center; justify-content: center; z-index: 1000; backdrop-filter: blur(1px); }
      .dialog { width: 720px; max-width: 92vw; background: #fff; border-radius: 10px; box-shadow: 0 20px 60px rgba(0,0,0,.25); overflow: hidden; }
      .hdr { display:flex; align-items:center; padding: 12px 16px; border-bottom:1px solid var(--border); }
      .ttl { margin:0; font-size: 18px; font-weight: 700; color:#111827; }
      .close { margin-left:auto; background: transparent; border: 0; font-size: 22px; cursor:pointer; line-height: 1; }
      .body { padding: 16px; }
    `
  ]
})
export class ModalComponent {
  @Input() open = false;
  @Input() title = '';
  @Input() closable = true;
  @Output() close = new EventEmitter<void>();

  onOverlayClick() {
    if (this.closable) this.close.emit();
  }
}
