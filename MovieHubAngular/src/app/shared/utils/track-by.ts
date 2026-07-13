import { Genero } from '../../models/genero.model';
import { Movie, MovieRow } from '../../models/movie.model';

export function trackByGeneroId(index: number, item: Genero): number {
  return item.id;
}

export function trackByMovieId(index: number, item: Movie): number {
  return item.id;
}

export function trackByRowTitle(index: number, item: MovieRow): string {
  return item.title;
}
