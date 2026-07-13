import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'ratingPercent',
  standalone: true
})
export class RatingPercentPipe implements PipeTransform {
  transform(value: number): string {
    return Math.round((value / 5) * 100) + '%';
  }
}
