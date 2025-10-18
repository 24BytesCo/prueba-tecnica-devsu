import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';
import { DashboardPageComponent } from './pages/dashboard-page.component';

const routes: Routes = [
  { path: '', component: DashboardPageComponent }
];

@NgModule({
  declarations: [DashboardPageComponent],
  imports: [CommonModule, RouterModule.forChild(routes)]
})
export class DashboardModule {}

