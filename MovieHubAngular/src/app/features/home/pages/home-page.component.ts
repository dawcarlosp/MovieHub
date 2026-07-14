import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

import { Movie, MovieRow } from '../../../models/movie.model';
import { MovieStateService } from '../../../core/services/movie-state.service';
import { MovieService } from '../../../core/services/movie.service';
import { FavoritoStateService } from '../../../core/services/favorito-state.service';
import { trackByRowTitle } from '../../../shared/utils/track-by';
import { HeroSectionComponent } from '../components/hero-section.component';
import { MovieRowComponent } from '../components/movie-row.component';
import { TrailerDialogComponent } from '../../peliculas/components/trailer-dialog.component';
import { SkeletonComponent } from '../../ui/skeleton.component';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule, MatIconModule, HeroSectionComponent, MovieRowComponent, SkeletonComponent],
  templateUrl: './home-page.component.html'
})
export class HomePageComponent {
  private readonly movieState = inject(MovieStateService);
  private readonly movieService = inject(MovieService);
  private readonly favoritoState = inject(FavoritoStateService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly heroMovie = this.movieState.heroMovie;
  readonly rows = this.movieState.rows;

  protected readonly trackByRowTitle = trackByRowTitle;

  constructor() {
    this.loadData();
  }

  showMovieDetail(movie: Movie): void {
    this.router.navigate(['/pelicula', movie.id]);
  }

  goToDetail(movie: Movie): void {
    this.showMovieDetail(movie);
  }

  openTrailerDialog(): void {
    this.dialog.open(TrailerDialogComponent, {
      width: '480px',
      disableClose: true
    });
  }

  private loadData(): void {
    this.loading.set(true);

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

    this.favoritoState.loadFavoritos();
  }
}
