import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';
import { CuentasListPageComponent } from './pages/cuentas-list-page.component';

const routes: Routes = [
  { path: '', component: CuentasListPageComponent }
];

@NgModule({
  declarations: [CuentasListPageComponent],
  imports: [CommonModule, RouterModule.forChild(routes)]
})
export class CuentasModule {}

