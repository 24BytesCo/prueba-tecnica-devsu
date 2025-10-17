import { NgModule } from '@angular/core';
import { RouterModule, Routes, PreloadAllModules } from '@angular/router';

const routes: Routes = [
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
  },
  {
    path: 'login',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
  },
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })],
  exports: [RouterModule]
})
export class AppRoutingModule {}

