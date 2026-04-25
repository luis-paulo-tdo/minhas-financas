import { Routes } from '@angular/router';
import { autenticacaoGuard } from './core/guards/autenticacao.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/autenticacao/login/login.component').then(m => m.LoginComponent) },
  {
    path: '',
    loadComponent: () => import('./features/principal/principal.component').then(m => m.PrincipalComponent),
    canActivate: [autenticacaoGuard],
    children: [
      { path: 'despesas', loadComponent: () => import('./features/despesas/despesas.component').then(m => m.DespesasComponent) },
      { path: 'estabelecimentos', loadComponent: () => import('./features/estabelecimentos/estabelecimentos.component').then(m => m.EstabelecimentosComponent) },
      { path: 'marcas', loadComponent: () => import('./features/marcas/marcas.component').then(m => m.MarcasComponent) },
      { path: '', redirectTo: 'despesas', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: '' }
];
