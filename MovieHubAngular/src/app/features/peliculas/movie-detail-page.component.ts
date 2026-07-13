import { Component, inject, input, output, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';

import { Movie } from '../../models/movie.model';
import { StarRatingComponent } from '../../shared/components/star-rating.component';
import { ValoracionService } from '../../core/services/valoracion.service';
import { AuthService } from '../../core/services/auth.service';
import { TrailerDialogComponent } from './trailer-dialog.component';

@Component({
  selector: 'app-movie-detail-page',
  standalone: true,
  imports: [
    CommonModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatTooltipModule, StarRatingComponent
  ],
  templateUrl: './movie-detail-page.component.html',
  styleUrl: './movie-detail-page.component.scss'
})
export class MovieDetailPageComponent {
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly valoracionService = inject(ValoracionService);
  private readonly auth = inject(AuthService);

  readonly movie = input.required<Movie>();
  readonly back = output<void>();
  readonly userRating = signal(0);

  constructor() {
    effect(() => {
      const m = this.movie();
      if (!m?.id) return;
      this.loadUserRating(m.id);
    });
  }

  openTrailer(): void {
    this.dialog.open(TrailerDialogComponent, {
      width: '480px',
      disableClose: true
    });
  }

  rateMovie(puntuacion: number): void {
    this.userRating.set(puntuacion);
    this.valoracionService.create({
      peliculaId: this.movie().id,
      puntuacion
    }).subscribe({
      error: (err: HttpErrorResponse) => {
        this.userRating.set(0);
        if (err.error?.message) {
          this.snackBar.open(err.error.message, 'Cerrar', { duration: 4000 });
        }
      }
    });
  }

  private loadUserRating(peliculaId: number): void {
    const userEmail = this.auth.currentUser()?.email;
    if (!userEmail) return;

    this.valoracionService.getByPelicula(peliculaId).subscribe({
      next: (ratings) => {
        const found = ratings.find((r) => r.usuarioEmail === userEmail);
        if (found) this.userRating.set(found.puntuacion);
      }
    });
  }
}
