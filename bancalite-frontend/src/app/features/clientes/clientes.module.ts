import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { ClientesListPageComponent } from './pages/clientes-list-page.component';
import { ClientesFormPageComponent } from './pages/clientes-form-page.component';

const routes: Routes = [
  { path: '', component: ClientesListPageComponent },
  { path: 'nuevo', component: ClientesFormPageComponent },
  { path: ':id/editar', component: ClientesFormPageComponent }
];

@NgModule({
  declarations: [ClientesListPageComponent, ClientesFormPageComponent],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SharedModule, RouterModule.forChild(routes)]
})
export class ClientesModule {}
