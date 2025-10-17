import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MovimientosListPageComponent } from './pages/movimientos-list-page.component';

const routes: Routes = [
  { path: '', component: MovimientosListPageComponent }
];

@NgModule({
  declarations: [MovimientosListPageComponent],
  imports: [CommonModule, RouterModule.forChild(routes)]
})
export class MovimientosModule {}

