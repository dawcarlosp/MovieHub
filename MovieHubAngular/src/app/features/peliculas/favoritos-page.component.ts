import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

import { FavoritoService, Favorito } from '../../core/services/favorito.service';
import { StarRatingComponent } from '../../shared/components/star-rating.component';
import { FavoritoButtonComponent } from '../../shared/components/favorito-button.component';

@Component({
  selector: 'app-favoritos-page',
  standalone: true,
  imports: [
    CommonModule, MatIconModule, MatCardModule,
    StarRatingComponent, FavoritoButtonComponent
  ],
  templateUrl: './favoritos-page.component.html',
  styleUrl: './favoritos-page.component.scss'
})
export class FavoritosPageComponent {
  private readonly favoritoService = inject(FavoritoService);
  readonly favoritos = signal<Favorito[]>([]);

  constructor() {
    this.favoritoService.getAll().subscribe({
      next: (favs) => this.favoritos.set(favs ?? [])
    });
  }
}
