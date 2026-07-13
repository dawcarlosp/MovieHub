import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal, effect } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog } from '@angular/material/dialog';

import { MovieStateService } from './core/services/movie-state.service';
import { AuthService } from './core/services/auth.service';
import { FavoritoStateService } from './core/services/favorito-state.service';
import { GeneroService } from './services/genero.service';
import { MovieService } from './services/movie.service';
import { NavbarComponent } from './core/layout/navbar.component';
import { SkeletonComponent } from './features/loading/skeleton.component';
import { HomePageComponent } from './features/home/home-page.component';
import { GeneroPageComponent } from './features/genero/genero-page.component';
import { LoginPageComponent } from './features/auth/login-page.component';
import { RegisterDialogComponent } from './features/auth/register-dialog.component';
import { MovieDetailPageComponent } from './features/peliculas/movie-detail-page.component';
import { FavoritosPageComponent } from './features/peliculas/favoritos-page.component';

import { Movie, MovieRow } from './models/movie.model';
import { Genero } from './models/genero.model';
import { ActiveView } from './shared/types';
import { TrailerDialogComponent } from './features/peliculas/trailer-dialog.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, NavbarComponent,
    SkeletonComponent, HomePageComponent, GeneroPageComponent,
    LoginPageComponent, MovieDetailPageComponent, FavoritosPageComponent,
    MatIconModule, MatDividerModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private readonly movieState = inject(MovieStateService);
  private readonly auth = inject(AuthService);
  private readonly favoritoState = inject(FavoritoStateService);
  private readonly movieService = inject(MovieService);
  private readonly generoService = inject(GeneroService);
  private readonly dialog = inject(MatDialog);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly generos = signal<Genero[]>([]);
  protected readonly selectedGenero = signal<Genero | null>(null);
  protected readonly activeView = signal<ActiveView>('home');
  protected readonly peliculasExpanded = signal(false);
  protected readonly showLogin = signal(false);
  protected readonly selectedMovie = signal<Movie | null>(null);

  protected readonly state = this.movieState;
  protected readonly authService = this.auth;

  constructor() {
    effect(() => {
      this.showLogin.set(this.auth.currentUser() === null);
    });
  }

  ngOnInit(): void {
    if (!this.auth.isLoggedIn()) return;
    this.favoritoState.loadFavoritos();
    this.loadGeneros();
    this.loadMovies();
  }

  onLoggedIn(): void {
    this.error.set(null);
    this.loading.set(true);
    this.goHome();
    this.favoritoState.loadFavoritos();
    this.loadGeneros();
    this.loadMovies();
  }

  onLoginClick(): void {
    this.showLogin.set(true);
  }

  onLogoutClick(): void {
    this.auth.logout();
  }

  onRegisterClick(): void {
    const ref = this.dialog.open(RegisterDialogComponent, {
      width: '420px',
      disableClose: true
    });
    ref.afterClosed().subscribe((result) => {
      if (result) this.onLoggedIn();
    });
  }

  showMovieDetail(movie: Movie): void {
    this.selectedMovie.set(movie);
    this.activeView.set('detalle');
  }

  goToDetail(movie: Movie | null): void {
    if (movie) this.showMovieDetail(movie);
  }

  openTrailerDialog(): void {
    this.dialog.open(TrailerDialogComponent, {
      width: '480px',
      disableClose: true
    });
  }

  goToFavoritos(): void {
    this.selectedMovie.set(null);
    this.activeView.set('favoritos');
  }

  onBackFromDetail(): void {
    this.loading.set(true);
    this.loadMovies();
    this.goHome();
  }

  private loadGeneros(): void {
    this.generoService.getAll().subscribe({
      next: (g) => this.generos.set(g),
      error: () => console.warn('No se pudieron cargar los géneros')
    });
  }

  private loadMovies(): void {
    this.movieService.getCatalogMovies().subscribe({
      next: (movies) => {
        this.movieState.setMovies(movies);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se ha podido conectar con la API de MovieHub.');
        this.loading.set(false);
      }
    });
  }

  selectGenero(genero: Genero): void {
    this.selectedGenero.set(genero);
    this.activeView.set('genero');
    this.peliculasExpanded.set(false);
  }

  goHome(): void {
    this.selectedGenero.set(null);
    this.selectedMovie.set(null);
    this.activeView.set('home');
    this.peliculasExpanded.set(false);
  }

  togglePeliculas(): void {
    this.peliculasExpanded.update((v) => !v);
  }
}
