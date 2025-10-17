import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ClientesListPageComponent } from './pages/clientes-list-page.component';

const routes: Routes = [
  { path: '', component: ClientesListPageComponent }
];

@NgModule({
  declarations: [ClientesListPageComponent],
  imports: [CommonModule, RouterModule.forChild(routes)]
})
export class ClientesModule {}

