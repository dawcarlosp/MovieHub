import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

import { Movie, MovieRow } from '../../models/movie.model';
import { trackByRowTitle } from '../../shared/utils/track-by';
import { HeroSectionComponent } from './hero-section.component';
import { MovieRowComponent } from './movie-row.component';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule, HeroSectionComponent, MovieRowComponent],
  templateUrl: './home-page.component.html'
})
export class HomePageComponent {
  readonly heroMovie = input.required<Movie | null>();
  readonly rows = input.required<MovieRow[]>();
  readonly movieClick = output<Movie>();
  readonly trailerClick = output<void>();
  readonly infoClick = output<void>();

  protected readonly trackByRowTitle = trackByRowTitle;
}
