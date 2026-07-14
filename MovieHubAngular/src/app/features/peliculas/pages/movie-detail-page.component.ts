import { Component, inject, input, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Location } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';

import { Movie } from '../../../models/movie.model';
import { StarRatingComponent } from '../../../shared/components/star-rating.component';
import { FavoritoButtonComponent } from '../../../shared/components/favorito-button.component';
import { MovieService } from '../../../core/services/movie.service';
import { ValoracionService } from '../../../core/services/valoracion.service';
import { AuthService } from '../../../core/services/auth.service';
import { TrailerDialogComponent } from '../components/trailer-dialog.component';

@Component({
  selector: 'app-movie-detail-page',
  standalone: true,
  imports: [
    CommonModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatTooltipModule, StarRatingComponent, FavoritoButtonComponent
  ],
  templateUrl: './movie-detail-page.component.html',
  styleUrl: './movie-detail-page.component.scss'
})
export class MovieDetailPageComponent {
  private readonly movieService = inject(MovieService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly valoracionService = inject(ValoracionService);
  private readonly auth = inject(AuthService);
  private readonly location = inject(Location);

  readonly id = input.required<string>();
  readonly movie = signal<Movie | null>(null);
  readonly loading = signal(true);
  readonly userRating = signal(0);
  readonly currentRatingId = signal<number | null>(null);

  constructor() {
    effect(() => {
      const movieId = Number(this.id());
      if (!movieId) return;
      this.loadMovie(movieId);
    });

    effect(() => {
      const m = this.movie();
      if (!m?.id) return;
      this.currentRatingId.set(null);
      this.userRating.set(0);
      this.loadUserRating(m.id);
    });
  }

  goBack(): void {
    this.location.back();
  }

  openTrailer(): void {
    this.dialog.open(TrailerDialogComponent, {
      width: '480px',
      disableClose: true
    });
  }

  rateMovie(puntuacion: number): void {
    const ratingId = this.currentRatingId();
    const currentRating = this.userRating();
    const m = this.movie();
    if (!m) return;

    if (ratingId && puntuacion === currentRating) {
      this.valoracionService.delete(ratingId).subscribe({
        next: () => {
          this.currentRatingId.set(null);
          this.userRating.set(0);
        },
        error: (err: HttpErrorResponse) => {
          if (err.error?.message) {
            this.snackBar.open(err.error.message, 'Cerrar', { duration: 4000 });
          }
        }
      });
      return;
    }

    this.userRating.set(puntuacion);

    const obs = ratingId
      ? this.valoracionService.update(ratingId, puntuacion)
      : this.valoracionService.create({ peliculaId: m.id, puntuacion });

    obs.subscribe({
      next: (res) => {
        if (!ratingId && res) this.currentRatingId.set((res as any).id ?? null);
      },
      error: (err: HttpErrorResponse) => {
        this.userRating.set(0);
        if (err.error?.message) {
          this.snackBar.open(err.error.message, 'Cerrar', { duration: 4000 });
        }
      }
    });
  }

  private loadMovie(movieId: number): void {
    this.loading.set(true);
    this.movieService.getById(movieId).subscribe({
      next: (m) => {
        this.movie.set(m);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  private loadUserRating(peliculaId: number): void {
    const userEmail = this.auth.currentUser()?.email;
    if (!userEmail) return;

    this.valoracionService.getByPelicula(peliculaId).subscribe({
      next: (ratings) => {
        const found = ratings.find((r) => r.usuarioEmail === userEmail);
        if (found) {
          this.userRating.set(found.puntuacion);
          this.currentRatingId.set(found.id);
        }
      }
    });
  }
}
