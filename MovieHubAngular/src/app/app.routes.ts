import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/inicio', pathMatch: 'full' },
  { path: 'inicio', loadComponent: () => import('./features/home/home-page.component').then(c => c.HomePageComponent) },
  { path: 'genero/:nombre', loadComponent: () => import('./features/genero/genero-page.component').then(c => c.GeneroPageComponent) },
  { path: 'login', loadComponent: () => import('./features/auth/login-page.component').then(c => c.LoginPageComponent) }
];
