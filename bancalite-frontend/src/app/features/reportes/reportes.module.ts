import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReportesPageComponent } from './pages/reportes-page.component';

const routes: Routes = [
  { path: '', component: ReportesPageComponent }
];

@NgModule({
  declarations: [ReportesPageComponent],
  imports: [CommonModule, RouterModule.forChild(routes)]
})
export class ReportesModule {}

