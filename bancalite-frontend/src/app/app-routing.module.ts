import { NgModule } from '@angular/core';
import { RouterModule, Routes, PreloadAllModules } from '@angular/router';
import { ProtectedLayoutComponent } from './layout/containers/protected-layout/protected-layout.component';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: 'login',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: '',
    component: ProtectedLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      // Cada feature se carga de forma perezosa para optimizar el arranque
      {
        path: '',
        loadChildren: () => import('./features/dashboard/dashboard.module').then(m => m.DashboardModule)
      },
      {
        path: 'clientes',
        loadChildren: () => import('./features/clientes/clientes.module').then(m => m.ClientesModule)
      },
      {
        path: 'cuentas',
        loadChildren: () => import('./features/cuentas/cuentas.module').then(m => m.CuentasModule)
      },
      {
        path: 'movimientos',
        loadChildren: () => import('./features/movimientos/movimientos.module').then(m => m.MovimientosModule)
      },
      {
        path: 'reportes',
        loadChildren: () => import('./features/reportes/reportes.module').then(m => m.ReportesModule)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })],
  exports: [RouterModule]
})
export class AppRoutingModule {}
