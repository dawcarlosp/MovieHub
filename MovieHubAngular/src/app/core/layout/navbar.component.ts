import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { Genero } from '../../models/genero.model';
import { trackByGeneroId } from '../../shared/utils/track-by';
import { AuthService } from '../services/auth.service';
import { GeneroService } from '../services/genero.service';
import { RegisterDialogComponent } from '../../features/auth/register-dialog.component';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatTooltipModule,
    MatDividerModule,
  ],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly generoService = inject(GeneroService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly generos = signal<Genero[]>([]);
  readonly peliculasExpanded = signal(false);

  protected readonly trackByGeneroId = trackByGeneroId;
  protected readonly authService = this.auth;

  ngOnInit(): void {
    this.generoService.getAll().subscribe({
      next: (g) => this.generos.set(g),
      error: () => console.warn('No se pudieron cargar los géneros'),
    });
  }

  togglePeliculas(): void {
    this.peliculasExpanded.update((v) => !v);
  }

  openRegister(): void {
    const ref = this.dialog.open(RegisterDialogComponent, {
      width: '420px',
      disableClose: true,
    });
    ref.afterClosed().subscribe((result) => {
      if (result) this.router.navigate(['/inicio']);
    });
  }
}
