import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';

import { Movie } from '../../models/movie.model';
import { TruncatePipe } from '../../shared/pipes/truncate.pipe';
import { RatingPercentPipe } from '../../shared/pipes/rating-percent.pipe';

@Component({
  selector: 'app-hero-section',
  standalone: true,
  imports: [
    CommonModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatTooltipModule, TruncatePipe, RatingPercentPipe
  ],
  templateUrl: './hero-section.component.html',
  styleUrl: './hero-section.component.scss'
})
export class HeroSectionComponent {
  readonly movie = input.required<Movie>();
  readonly trailerClick = output<void>();
  readonly infoClick = output<void>();
}
