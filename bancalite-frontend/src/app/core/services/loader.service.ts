import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoaderService {
  private counter = 0;
  private readonly _loading$ = new BehaviorSubject<boolean>(false);
  readonly loading$ = this._loading$.asObservable();

  start() {
    this.counter++;
    if (!this._loading$.value) this._loading$.next(true);
  }
  stop() {
    this.counter = Math.max(0, this.counter - 1);
    if (this.counter === 0 && this._loading$.value) this._loading$.next(false);
  }
  reset() {
    this.counter = 0;
    this._loading$.next(false);
  }
}

