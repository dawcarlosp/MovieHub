import { Component, inject, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';

import { Genero } from '../../models/genero.model';
import { ActiveView } from '../../shared/types';
import { trackByGeneroId } from '../../shared/utils/track-by';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule, MatIconModule, MatButtonModule,
    MatMenuModule, MatTooltipModule, MatDividerModule
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
  private readonly auth = inject(AuthService);

  readonly generos = input<Genero[]>([]);
  readonly activeView = input<ActiveView>('home');
  readonly peliculasExpanded = input(false);
  readonly selectedGenero = input<Genero | null>(null);

  readonly homeClick = output<void>();
  readonly selectGenero = output<Genero>();
  readonly togglePeliculas = output<void>();
  readonly loginClick = output<void>();
  readonly registerClick = output<void>();
  readonly logoutClick = output<void>();
  readonly favoritosClick = output<void>();

  protected readonly trackByGeneroId = trackByGeneroId;
  protected readonly authService = this.auth;
}
