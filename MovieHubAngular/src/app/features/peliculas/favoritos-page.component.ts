import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

import { FavoritoStateService } from '../../core/services/favorito-state.service';
import { FavoritoService, Favorito } from '../../core/services/favorito.service';
import { StarRatingComponent } from '../../shared/components/star-rating.component';
import { FavoritoButtonComponent } from '../../shared/components/favorito-button.component';

@Component({
  selector: 'app-favoritos-page',
  standalone: true,
  imports: [
    CommonModule, MatButtonModule, MatIconModule, MatCardModule,
    StarRatingComponent, FavoritoButtonComponent
  ],
  templateUrl: './favoritos-page.component.html',
  styleUrl: './favoritos-page.component.scss'
})
export class FavoritosPageComponent {
  private readonly favoritoService = inject(FavoritoService);
  protected readonly favoritoState = inject(FavoritoStateService);

  favoritos: Favorito[] = [];

  constructor() {
    this.loadFavoritos();
  }

  private loadFavoritos(): void {
    this.favoritoService.getAll().subscribe({
      next: (favs) => {
        console.log('Favoritos cargados:', favs);
        this.favoritos = favs;
      },
      error: (err) => {
        console.error('Error al cargar favoritos:', err);
      }
    });
  }
}
