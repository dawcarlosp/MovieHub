import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/inicio', pathMatch: 'full' },
  {
    path: 'inicio',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/home/home-page.component').then((c) => c.HomePageComponent),
  },
  {
    path: 'genero/:nombre',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/genero/genero-page.component').then((c) => c.GeneroPageComponent),
  },
  {
    path: 'pelicula/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/peliculas/movie-detail-page.component').then(
        (c) => c.MovieDetailPageComponent,
      ),
  },
  {
    path: 'favoritos',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/peliculas/favoritos-page.component').then((c) => c.FavoritosPageComponent),
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login-page.component').then((c) => c.LoginPageComponent),
  },
  { path: '**', redirectTo: '/inicio' },
];
