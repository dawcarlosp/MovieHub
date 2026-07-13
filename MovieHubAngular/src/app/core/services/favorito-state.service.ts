import { Injectable, inject, signal } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';
import { FavoritoService } from './favorito.service';

@Injectable({ providedIn: 'root' })
export class FavoritoStateService {
  private readonly favoritoService = inject(FavoritoService);
  private readonly favoritoIds = signal<Set<number>>(new Set());

  readonly ids = this.favoritoIds.asReadonly();

  esFavorito(peliculaId: number): boolean {
    return this.favoritoIds().has(peliculaId);
  }

  loadFavoritos(): void {
    this.favoritoService.getAll().subscribe({
      next: (favs) => this.favoritoIds.set(new Set(favs.map((f) => f.peliculaId)))
    });
  }

  toggle(peliculaId: number): Observable<boolean> {
    const esFav = this.esFavorito(peliculaId);

    const set = new Set(this.favoritoIds());
    esFav ? set.delete(peliculaId) : set.add(peliculaId);
    this.favoritoIds.set(set);

    const obs = esFav
      ? this.favoritoService.remove(peliculaId).pipe(map(() => false))
      : this.favoritoService.add(peliculaId).pipe(map(() => true));

    return obs.pipe(
      catchError((err) => {
        const revert = new Set(this.favoritoIds());
        esFav ? revert.add(peliculaId) : revert.delete(peliculaId);
        this.favoritoIds.set(revert);
        return throwError(() => err);
      })
    );
  }
}
