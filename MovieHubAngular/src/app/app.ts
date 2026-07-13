import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';

import { MovieStateService } from './core/services/movie-state.service';
import { GeneroService } from './services/genero.service';
import { MovieService } from './services/movie.service';
import { NavbarComponent } from './core/layout/navbar.component';
import { SkeletonComponent } from './features/loading/skeleton.component';
import { HomePageComponent } from './features/home/home-page.component';
import { GeneroPageComponent } from './features/genero/genero-page.component';

import { Genero } from './models/genero.model';
import { ActiveView } from './shared/types';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, NavbarComponent,
    SkeletonComponent, HomePageComponent, GeneroPageComponent,
    MatIconModule, MatDividerModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  private readonly movieState = inject(MovieStateService);
  private readonly movieService = inject(MovieService);
  private readonly generoService = inject(GeneroService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly generos = signal<Genero[]>([]);
  protected readonly selectedGenero = signal<Genero | null>(null);
  protected readonly activeView = signal<ActiveView>('home');
  protected readonly peliculasExpanded = signal(false);

  protected readonly state = this.movieState;

  ngOnInit(): void {
    this.loadGeneros();
    this.loadMovies();
  }

  private loadGeneros(): void {
    this.generoService.getAll().subscribe({
      next: (g) => this.generos.set(g),
      error: () => console.warn('No se pudieron cargar los géneros')
    });
  }

  private loadMovies(): void {
    this.movieService.getCatalogMovies().subscribe({
      next: (movies) => {
        this.movieState.setMovies(movies);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se ha podido conectar con la API de MovieHub.');
        this.loading.set(false);
      }
    });
  }

  selectGenero(genero: Genero): void {
    this.selectedGenero.set(genero);
    this.activeView.set('genero');
    this.peliculasExpanded.set(false);
  }

  goHome(): void {
    this.selectedGenero.set(null);
    this.activeView.set('home');
    this.peliculasExpanded.set(false);
  }

  togglePeliculas(): void {
    this.peliculasExpanded.update((v) => !v);
  }
}
