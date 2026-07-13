import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, map, switchMap } from 'rxjs';

import { Movie, PaginatedResponse } from '../models/movie.model';
import { environment } from '../../environments/environment';

const MAX_PAGE_SIZE = 50;
const CATALOG_PAGES = 2;

@Injectable({
  providedIn: 'root'
})
export class MovieService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrlBase}/Peliculas`;

  getPage(page: number, pageSize: number = MAX_PAGE_SIZE): Observable<PaginatedResponse<Movie>> {
    return this.http.get<PaginatedResponse<Movie>>(this.baseUrl, {
      params: { page, pageSize }
    });
  }

  getCatalogMovies(): Observable<Movie[]> {
    const firstPage$ = this.getPage(1, MAX_PAGE_SIZE);

    return firstPage$.pipe(
      switchMap((first) => {
        const totalPages = Math.min(first.totalPages, CATALOG_PAGES);

        if (totalPages <= 1) {
          return new Observable<Movie[]>((subscriber) => {
            subscriber.next(first.items);
            subscriber.complete();
          });
        }

        const restRequests = Array.from(
          { length: totalPages - 1 },
          (_, i) => this.getPage(i + 2, MAX_PAGE_SIZE)
        );

        return forkJoin(restRequests).pipe(
          map((rest) => [
            ...first.items,
            ...rest.flatMap((r) => r.items)
          ])
        );
      })
    );
  }
}
