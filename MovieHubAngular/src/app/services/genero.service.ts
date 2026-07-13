import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Genero } from '../models/genero.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GeneroService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrlBase}/Generos`;

  getAll(): Observable<Genero[]> {
    return this.http.get<Genero[]>(this.baseUrl);
  }
}
