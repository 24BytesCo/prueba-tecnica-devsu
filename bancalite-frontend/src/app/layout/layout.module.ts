import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { HeaderComponent } from './components/header/header.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { FooterComponent } from './components/footer/footer.component';
import { RouterModule } from '@angular/router';
import { ProtectedLayoutComponent } from './containers/protected-layout/protected-layout.component';
import { LoaderComponent } from '../shared/components/loader/loader.component';

@NgModule({
  declarations: [HeaderComponent, SidebarComponent, FooterComponent, ProtectedLayoutComponent],
  imports: [CommonModule, RouterModule, LoaderComponent],
  exports: [ProtectedLayoutComponent]
})
export class LayoutModule {}
