import { Component, input } from '@angular/core';

@Component({
  selector: 'app-genre-banner',
  standalone: true,
  imports: [],
  templateUrl: './genre-banner.component.html',
  styleUrl: './genre-banner.component.scss'
})
export class GenreBannerComponent {
  readonly nombre = input.required<string>();
}
