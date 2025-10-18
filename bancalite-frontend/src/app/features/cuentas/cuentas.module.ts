import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CuentasListPageComponent } from './pages/cuentas-list-page.component';
import { CuentasFormPageComponent } from './pages/cuentas-form-page.component';
import { CuentasDetailPageComponent } from './pages/cuentas-detail-page.component';
import { ReactiveFormsModule } from '@angular/forms';

const routes: Routes = [
  { path: '', component: CuentasListPageComponent },
  { path: 'nuevo', component: CuentasFormPageComponent },
  { path: ':id/editar', component: CuentasFormPageComponent },
  { path: ':id', component: CuentasDetailPageComponent }
];

@NgModule({
  declarations: [CuentasListPageComponent, CuentasFormPageComponent, CuentasDetailPageComponent],
  imports: [CommonModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class CuentasModule {}
