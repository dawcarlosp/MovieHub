import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, forkJoin, map, switchMap } from 'rxjs';

import { Movie, PaginatedResponse } from '../../models/movie.model';
import { environment } from '../../../environments/environment';

export interface Estadisticas {
  totalPeliculas: number;
  puntuacionMediaGlobal: number;
  totalGeneros: number;
  totalValoraciones: number;
}

const MAX_PAGE_SIZE = 50;
const CATALOG_PAGES = 2;

@Injectable({ providedIn: 'root' })
export class MovieService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrlBase}/Peliculas`;

  search(params: {
    page?: number;
    pageSize?: number;
    titulo?: string;
    generoId?: number;
    orden?: string;
  } = {}): Observable<PaginatedResponse<Movie>> {
    let httpParams = new HttpParams();
    if (params.page) httpParams = httpParams.set('page', params.page);
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize);
    if (params.titulo) httpParams = httpParams.set('titulo', params.titulo);
    if (params.generoId) httpParams = httpParams.set('generoId', params.generoId);
    if (params.orden) httpParams = httpParams.set('orden', params.orden);

    return this.http.get<PaginatedResponse<Movie>>(this.baseUrl, { params: httpParams });
  }

  getById(id: number): Observable<Movie> {
    return this.http.get<Movie>(`${this.baseUrl}/${id}`);
  }

  getMejorValoradas(): Observable<Movie[]> {
    return this.http.get<Movie[]>(`${this.baseUrl}/mejor-valoradas`);
  }

  getMasRecientes(): Observable<Movie[]> {
    return this.http.get<Movie[]>(`${this.baseUrl}/mas-recientes`);
  }

  getEstadisticas(): Observable<Estadisticas> {
    return this.http.get<Estadisticas>(`${this.baseUrl}/estadisticas`);
  }

  getCatalogMovies(): Observable<Movie[]> {
    const firstPage$ = this.search({ page: 1, pageSize: MAX_PAGE_SIZE });

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
          (_, i) => this.search({ page: i + 2, pageSize: MAX_PAGE_SIZE })
        );
        return forkJoin(restRequests).pipe(
          map((rest) => [...first.items, ...rest.flatMap((r) => r.items)])
        );
      })
    );
  }
}
