import { CommonModule } from '@angular/common';
import { Component, OnInit, signal, ChangeDetectionStrategy, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';

import { Movie, MovieRow } from './models/movie.model';
import { Genero } from './models/genero.model';
import { MovieService } from './services/movie.service';
import { GeneroService } from './services/genero.service';

const MOVIES_PER_ROW = 20;

export type ActiveView = 'home' | 'genero';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet, CommonModule,
    MatToolbarModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatCardModule, MatTooltipModule,
    MatChipsModule, MatDividerModule, MatMenuModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App implements OnInit {
  private readonly generoService = inject(GeneroService);
  private readonly movieService = inject(MovieService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly heroMovie = signal<Movie | null>(null);
  protected readonly rows = signal<MovieRow[]>([]);
  protected readonly generos = signal<Genero[]>([]);
  protected readonly selectedGenero = signal<Genero | null>(null);
  protected readonly activeView = signal<ActiveView>('home');
  protected readonly peliculasExpanded = signal(false);

  private allMovies: Movie[] = [];

  ngOnInit(): void {
    this.loadGeneros();
    this.loadMovies();
  }

  private loadGeneros(): void {
    this.generoService.getAll().subscribe({
      next: (generos) => this.generos.set(generos),
      error: () => console.warn('No se pudieron cargar los géneros')
    });
  }

  private loadMovies(): void {
    this.movieService.getCatalogMovies().subscribe({
      next: (movies) => {
        this.allMovies = movies;
        this.heroMovie.set(this.pickHeroMovie(movies));
        this.rows.set(this.buildRowsByGenre(movies));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se ha podido conectar con la API de MovieHub. Comprueba que el backend esté arrancado.');
        this.loading.set(false);
      }
    });
  }

  selectGenero(genero: Genero): void {
    this.selectedGenero.set(genero);
    this.activeView.set('genero');
    this.peliculasExpanded.set(false);
    const generoRows = this.buildRowsByGenre(this.allMovies).filter(
      (r) => r.title === genero.nombre
    );
    this.rows.set(generoRows);
  }

  goHome(): void {
    this.selectedGenero.set(null);
    this.activeView.set('home');
    this.peliculasExpanded.set(false);
    this.heroMovie.set(this.pickHeroMovie(this.allMovies));
    this.rows.set(this.buildRowsByGenre(this.allMovies));
  }

  togglePeliculas(): void {
    this.peliculasExpanded.update((v) => !v);
  }

  truncateSynopsis(text: string | null): string {
    if (!text) return '';
    const cleaned = text.replace(/\s+/g, ' ').trim();
    return cleaned.length > 200 ? cleaned.slice(0, 197) + '...' : cleaned;
  }

  trackByGeneroId(_index: number, genero: Genero): number {
    return genero.id;
  }

  trackByMovieId(_index: number, movie: Movie): number {
    return movie.id;
  }

  trackByRowTitle(_index: number, row: MovieRow): string {
    return row.title;
  }

  private pickHeroMovie(movies: Movie[]): Movie | null {
    if (movies.length === 0) return null;
    return [...movies].sort((a, b) => b.puntuacionMedia - a.puntuacionMedia)[0];
  }

  private buildRowsByGenre(movies: Movie[]): MovieRow[] {
    const generosUnicos = Array.from(new Set(movies.flatMap((m) => m.generos)));
    return generosUnicos
      .map((genero) => ({
        title: genero,
        movies: movies
          .filter((m) => m.generos.includes(genero))
          .sort((a, b) => b.puntuacionMedia - a.puntuacionMedia)
          .slice(0, MOVIES_PER_ROW)
      }))
      .filter((row) => row.movies.length > 0);
  }
}
