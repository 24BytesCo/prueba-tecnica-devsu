import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MovimientosListPageComponent } from './pages/movimientos-list-page.component';
import { MovimientosFormPageComponent } from './pages/movimientos-form-page.component';

const routes: Routes = [
  { path: '', component: MovimientosListPageComponent },
  { path: 'nuevo', component: MovimientosFormPageComponent }
];

@NgModule({
  declarations: [MovimientosListPageComponent, MovimientosFormPageComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class MovimientosModule {}
