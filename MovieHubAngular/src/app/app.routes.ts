import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/inicio', pathMatch: 'full' },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/pages/login-page.component').then((c) => c.LoginPageComponent),
  },
  {
    path: '',
    loadComponent: () =>
      import('./core/layout/shell.component').then((c) => c.ShellComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'inicio',
        loadComponent: () =>
          import('./features/home/pages/home-page.component').then((c) => c.HomePageComponent),
      },
      {
        path: 'genero/:nombre',
        loadComponent: () =>
          import('./features/genero/pages/genero-page.component').then((c) => c.GeneroPageComponent),
      },
      {
        path: 'pelicula/:id',
        loadComponent: () =>
          import('./features/peliculas/pages/movie-detail-page.component').then(
            (c) => c.MovieDetailPageComponent,
          ),
      },
      {
        path: 'favoritos',
        loadComponent: () =>
          import('./features/peliculas/pages/favoritos-page.component').then(
            (c) => c.FavoritosPageComponent,
          ),
      },
      { path: '**', redirectTo: '/inicio' },
    ],
  },
];
