import { CommonModule } from '@angular/common';
import { Component, OnInit, signal, HostListener, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';

import { Movie, MovieRow } from './models/movie.model';
import { MovieService } from './services/movie.service';

const MOVIES_PER_ROW = 20;

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet, CommonModule,
    MatToolbarModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatCardModule, MatTooltipModule, MatChipsModule, MatDividerModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App implements OnInit {
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly heroMovie = signal<Movie | null>(null);
  protected readonly rows = signal<MovieRow[]>([]);
  protected readonly scrolled = signal(false);

  constructor(private movieService: MovieService) {}

  @HostListener('window:scroll')
  onScroll(): void {
    this.scrolled.set(window.scrollY > 50);
  }

  ngOnInit(): void {
    this.movieService.getCatalogMovies().subscribe({
      next: (movies) => {
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
