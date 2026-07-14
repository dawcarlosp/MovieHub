import { Component, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MovieStateService } from '../../core/services/movie-state.service';
import { trackByRowTitle } from '../../shared/utils/track-by';
import { GenreBannerComponent } from './genre-banner.component';
import { MovieRowComponent } from '../home/movie-row.component';

@Component({
  selector: 'app-genero-page',
  standalone: true,
  imports: [CommonModule, GenreBannerComponent, MovieRowComponent],
  templateUrl: './genero-page.component.html'
})
export class GeneroPageComponent {
  protected readonly movieState = inject(MovieStateService);

  readonly nombre = input.required<string>();

  protected readonly trackByRowTitle = trackByRowTitle;
}
