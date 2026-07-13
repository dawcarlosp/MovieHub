import { Component, inject, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';

import { FavoritoStateService } from '../../core/services/favorito-state.service';

@Component({
  selector: 'app-favorito-button',
  standalone: true,
  imports: [MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './favorito-button.component.html',
  styleUrl: './favorito-button.component.scss'
})
export class FavoritoButtonComponent {
  private readonly snackBar = inject(MatSnackBar);
  protected readonly favoritoState = inject(FavoritoStateService);

  readonly peliculaId = input.required<number>();

  toggle(): void {
    this.favoritoState.toggle(this.peliculaId()).subscribe({
      error: () => this.snackBar.open('Error al actualizar favoritos.', 'Cerrar', { duration: 4000 })
    });
  }
}
