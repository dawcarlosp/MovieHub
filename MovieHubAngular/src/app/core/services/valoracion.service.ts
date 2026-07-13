import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Valoracion {
  id: number;
  usuarioEmail: string;
  puntuacion: number;
  fecha: string;
}

export interface CreateValoracionDto {
  peliculaId: number;
  puntuacion: number;
}

@Injectable({ providedIn: 'root' })
export class ValoracionService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrlBase}/Valoraciones`;

  getByPelicula(peliculaId: number): Observable<Valoracion[]> {
    return this.http.get<Valoracion[]>(`${this.baseUrl}/pelicula/${peliculaId}`);
  }

  create(dto: CreateValoracionDto): Observable<Valoracion> {
    return this.http.post<Valoracion>(this.baseUrl, dto);
  }

  update(id: number, puntuacion: number): Observable<Valoracion> {
    return this.http.put<Valoracion>(`${this.baseUrl}/${id}`, puntuacion);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
