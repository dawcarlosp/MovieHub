import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './star-rating.component.html',
  styleUrl: './star-rating.component.scss'
})
export class StarRatingComponent {
  readonly puntuacion = input.required<number>();
  readonly readonly = input(false);
  readonly size = input<'small' | 'medium'>('medium');
  readonly rate = output<number>();
}
