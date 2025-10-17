import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LoginPageComponent } from './pages/login-page.component';

const routes: Routes = [
  { path: '', component: LoginPageComponent }
];

@NgModule({
  declarations: [LoginPageComponent],
  imports: [CommonModule, FormsModule, RouterModule.forChild(routes)]
})
export class AuthModule {}
