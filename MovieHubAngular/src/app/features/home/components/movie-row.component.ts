import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

import { Movie, MovieRow } from '../../../models/movie.model';
import { trackByMovieId } from '../../../shared/utils/track-by';
import { MovieCardComponent } from './movie-card.component';

@Component({
  selector: 'app-movie-row',
  standalone: true,
  imports: [CommonModule, MovieCardComponent],
  templateUrl: './movie-row.component.html',
  styleUrl: './movie-row.component.scss'
})
export class MovieRowComponent {
  readonly row = input.required<MovieRow>();
  readonly movieClick = output<Movie>();

  protected readonly trackByMovieId = trackByMovieId;
}
