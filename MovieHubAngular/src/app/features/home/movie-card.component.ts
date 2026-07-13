import { Component, input, output } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { CommonModule } from '@angular/common';

import { Movie } from '../../models/movie.model';
import { FavoritoButtonComponent } from '../../shared/components/favorito-button.component';

@Component({
  selector: 'app-movie-card',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatChipsModule, FavoritoButtonComponent],
  templateUrl: './movie-card.component.html',
  styleUrl: './movie-card.component.scss'
})
export class MovieCardComponent {
  readonly movie = input.required<Movie>();
  readonly movieClick = output<Movie>();
}
