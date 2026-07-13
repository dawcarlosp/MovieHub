# Guía de Frontend — MovieHub

Rol asignado: **[@claauudiiaacr](https://github.com/claauudiiaacr)** — rama `feature/frontend`

---

> 💡 **¿Package Manager Console o Terminal normal?** Esta guía usa solo la **Terminal normal** (View → Terminal en VS, Ctrl+` en VS Code, PowerShell, CMD o Windows Terminal).

## Tecnologías

- Angular 22 (standalone components, sin NgModules)
- Build system: `@angular/build` (Vite/esbuild)
- Angular Material (M3 con tema custom Netflix oscuro)
- SCSS para estilos
- Señales (Signals) para estado reactivo
- Tests: Vitest (integrados por defecto en Angular CLI 22)

---

## Estructura de carpetas (actual)

Actualmente el proyecto sigue esta estructura:

```
src/app/
├── core/                      # Singleton services, interceptors, layout
│   ├── interceptors/
│   │   ├── error.interceptor.ts
│   │   └── auth.interceptor.ts
│   ├── layout/
│   │   └── navbar.component.ts
│   └── services/
│       ├── movie-state.service.ts
│       ├── auth.service.ts
│       ├── favorito-state.service.ts   # Estado global de favoritos (Set de IDs + toggle optimista)
│       ├── valoracion.service.ts
│       └── favorito.service.ts
├── shared/                    # Componentes reutilizables, pipes, utilidades
│   ├── components/
│   │   ├── star-rating.component.ts   # Valoración 1-5 estrellas
│   │   └── favorito-button.component.ts # Botón corazón con toggle optimista
│   ├── pipes/
│   │   ├── truncate.pipe.ts
│   │   └── rating-percent.pipe.ts
│   ├── utils/
│   │   └── track-by.ts
│   ├── types/
│   │   └── index.ts           # ActiveView, AuthResponse, LoginDto, RegisterDto
│   └── constants.ts
├── features/                  # Carpetas por funcionalidad
│   ├── home/
│   │   ├── home-page.component.ts
│   │   ├── hero-section.component.ts
│   │   ├── movie-row.component.ts
│   │   └── movie-card.component.ts
│   ├── genero/
│   │   ├── genero-page.component.ts
│   │   └── genre-banner.component.ts
│   ├── auth/
│   │   ├── login-page.component.ts
│   │   └── register-dialog.component.ts
│   ├── peliculas/
│   │   ├── movie-detail-page.component.ts
│   │   ├── trailer-dialog.component.ts
│   │   └── favoritos-page.component.ts  # Página "Mi lista"
│   └── loading/
│       └── skeleton.component.ts
├── services/                  # Servicios HTTP (movie.service, genero.service)
├── models/                    # Interfaces TypeScript (Movie, Genero, etc.)
├── app.component.ts
├── app.config.ts
└── app.routes.ts
```

Para crear una nueva feature, sigue este orden:

```
Model → Service → Componente → Ruta
```

No al revés. Siempre empieza por los datos.

---

## Paso 1: Modelo (`models/`)

Crea una interfaz TypeScript que refleje el DTO del backend:

```typescript
// features/peliculas/models/pelicula.model.ts
export interface Pelicula {
  id: number;
  titulo: string;
  descripcion: string;
  duracion: number;
  anioEstreno: number;
  director: string;
  imagen: string | null;
  puntuacionMedia: number;
  generos: string[];
}

export interface CreatePelicula {
  titulo: string;
  descripcion: string;
  duracion: number;
  anioEstreno: number;
  director: string;
  generoIds: number[];
}
```

---

## Paso 2: Servicio (`services/`)

Cada feature tiene su propio servicio que extiende de un servicio base o usa `HttpClient` directamente:

```typescript
// features/peliculas/services/pelicula.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Pelicula } from '../models/pelicula.model';

@Injectable({ providedIn: 'root' })
export class PeliculaService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:7154/api/peliculas';

  getAll(): Observable<Pelicula[]> {
    return this.http.get<Pelicula[]>(this.apiUrl);
  }

  getById(id: number): Observable<Pelicula> {
    return this.http.get<Pelicula>(`${this.apiUrl}/${id}`);
  }

  create(data: CreatePelicula): Observable<Pelicula> {
    return this.http.post<Pelicula>(this.apiUrl, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
```

---

## Paso 3: Componente

Usa **señales** para el estado local y **OnPush** como estrategia de detección de cambios:

```typescript
// features/peliculas/pages/listado-peliculas/listado-peliculas.component.ts
import { Component, inject, signal, OnInit } from '@angular/core';
import { PeliculaService } from '../../services/pelicula.service';
import { Pelicula } from '../../models/pelicula.model';

@Component({
  selector: 'app-listado-peliculas',
  standalone: true,
  imports: [...],
  templateUrl: './listado-peliculas.component.html',
  styleUrl: './listado-peliculas.component.scss',
})
export class ListadoPeliculasComponent implements OnInit {
  private readonly peliculaService = inject(PeliculaService);

  readonly peliculas = signal<Pelicula[]>([]);
  readonly cargando = signal(true);

  ngOnInit(): void {
    this.peliculaService.getAll().subscribe({
      next: (data) => {
        this.peliculas.set(data);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false),
    });
  }
}
```

---

## Paso 4: Rutas

Cada feature tiene su archivo de rutas:

```typescript
// features/peliculas/peliculas.routes.ts
import { Routes } from '@angular/router';

export const PELICULAS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/listado-peliculas/listado-peliculas.component').then(
        (c) => c.ListadoPeliculasComponent
      ),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/detalle-pelicula/detalle-pelicula.component').then(
        (c) => c.DetallePeliculaComponent
      ),
  },
];
```

Y se registran en `app.routes.ts` con lazy loading (actualmente con rutas para carga diferida):

```typescript
// app.routes.ts
export const routes: Routes = [
  { path: '', redirectTo: '/inicio', pathMatch: 'full' },
  {
    path: 'inicio',
    loadComponent: () =>
      import('./features/home/home-page.component').then(c => c.HomePageComponent)
  },
  {
    path: 'genero/:nombre',
    loadComponent: () =>
      import('./features/genero/genero-page.component').then(c => c.GeneroPageComponent)
  }
];
```

---

## Configurar HttpClient

> ✅ `HttpClient` ya está configurado en `app.config.ts` con `provideHttpClient(withFetch())` y el interceptor global de errores.

```typescript
import { provideHttpClient, withFetch } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withFetch()),
    provideRouter(routes),
    provideBrowserGlobalErrorListeners(),
  ],
};
```

No olvides instalar `@angular/common/http` si no está (debería venir incluido en `@angular/common`).

---

## Angular Material

> ✅ Angular Material ya está instalado (v22) con un tema M3 custom oscuro (paleta roja Netflix, fondo #141414). El tema se configura en `src/styles.scss` mediante `mat.define-theme()`.

Componentes que se usan actualmente:

| Componente | Uso |
|---|---|
| `<mat-toolbar>` | Barra de navegación (NavbarComponent) |
| `<mat-card>` | Tarjetas de película (MovieCardComponent) |
| `<mat-menu>` | Menú de géneros desktop + hamburguesa móvil |
| `<mat-chip-set>` / `<mat-chip>` | Etiquetas de género en héroe y cards |
| `<mat-icon>` | Iconos (search, play_arrow, account_circle, home, etc.) |
| `<mat-divider>` | Separadores en footer y menú hamburguesa |
| `<mat-button>` / `<mat-icon-button>` / `<mat-raised-button>` / `<mat-stroked-button>` | Botones de navegación y acciones |
| `<mat-dialog>` | Modal de registro de usuario (RegisterDialogComponent) |
| `<mat-snack-bar>` | Notificaciones toast en login y registro exitosos/fallidos |
| `<mat-form-field>` + `<mat-input>` | Campos de formulario con validación en login y registro |

---

## Errores frecuentes

| Error | Causa | Solución |
|---|---|---|---|
| `NullInjectorError: No provider for HttpClient` | No llamaste a `provideHttpClient()` en `app.config.ts` | Añadirlo |
| `Cannot find a differ supporting object '...'` | Usaste `*ngFor` con un signal en lugar de llamarlo como función | Usar `@for` de Angular 17+ o `peliculas()` |
| 404 al llamar a la API | La URL del backend es incorrecta o CORS no permite el origen | Verificar puerto (`7154`) y policy CORS en `Program.cs` |
| El componente no aparece | No está importado en el template o la ruta no está registrada | Comprobar `imports` del componente y `app.routes.ts` |
| `ng test` falla | Angular 22 usa Vitest por defecto | Asegúrate de tener `@angular/build` actualizado; el comando es `ng test` sin flags |

---

## Comandos útiles *(todos en Terminal normal)*

| Comando | Qué hace |
|---|---|
| `ng serve` | Arrancar servidor de desarrollo (`http://localhost:4200`) |
| `ng g c features/peliculas/pages/listado-peliculas` | Generar componente (standalone por defecto) |
| `ng g s features/peliculas/services/pelicula` | Generar servicio |
| `ng add @angular/material` | Añadir Angular Material |

---

### Después del merge

Cuando tu PR se fusione, borra la rama y crea una nueva desde `main` actualizado.
Ver [`CONTRIBUTING.md`](../CONTRIBUTING.md#despu%C3%A9s-del-merge-c%C3%B3mo-empezar-la-siguiente-iteraci%C3%B3n).
