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
- Router de Angular con lazy loading + guards funcionales
- Tests: Vitest (integrados por defecto en Angular CLI 22)

---

## Estructura de carpetas (actual)

Cada feature se organiza con subcarpetas `pages/` (componentes que son rutas) y `components/` (subcomponentes de UI):

```
src/app/
├── core/                          # Singleton services, guards, interceptors, layout
│   ├── guards/
│   │   ├── auth.guard.ts          # Redirige a /login si no hay sesión
│   │   └── guest.guard.ts         # Redirige a /inicio si ya hay sesión
    │   ├── interceptors/
    │   │   ├── auth-error.interceptor.ts   # Log de errores HTTP + logout en 401 (excepto auth)
    │   │   └── auth.interceptor.ts         # Añade Bearer token a las peticiones
│   ├── layout/
│   │   ├── shell.component.ts     # Layout con navbar + <router-outlet /> + footer
│   │   └── navbar.component.ts    # Navbar con menú géneros, auth, routerLink
│   └── services/
│       ├── movie.service.ts       # CRUD películas con paginación y endpoints específicos
│       ├── genero.service.ts      # CRUD géneros
│       ├── movie-state.service.ts # Estado global: todas las películas, héroe, filas por género
│       ├── auth.service.ts        # Login/register/logout + localStorage + signal currentUser
│       ├── favorito.service.ts    # HTTP para favoritos (getAll, add, remove)
│       ├── favorito-state.service.ts  # Estado global de favoritos (Set de IDs + toggle optimista)
│       └── valoracion.service.ts  # CRUD valoraciones
├── shared/                        # Componentes reutilizables, pipes, utilidades
    │   ├── components/
    │   │   ├── star-rating.component.ts      # Valoración 1-5 estrellas
    │   │   ├── favorito-button.component.ts  # Botón corazón con toggle optimista
    │   │   └── confirm-dialog.component.ts   # Diálogo de confirmación (cerrar sesión)
│   ├── pipes/
│   │   ├── truncate.pipe.ts
│   │   └── rating-percent.pipe.ts
│   ├── utils/
│   │   └── track-by.ts
│   ├── types/
│   │   └── index.ts              # AuthResponse, LoginDto, RegisterDto
│   └── constants.ts
├── features/                      # Carpetas por funcionalidad
│   ├── auth/
│   │   ├── pages/
│   │   │   └── login-page.component.ts
│   │   └── components/
│   │       └── register-dialog.component.ts
│   ├── home/
│   │   ├── pages/
│   │   │   └── home-page.component.ts
│   │   └── components/
│   │       ├── hero-section.component.ts
│   │       ├── movie-row.component.ts
│   │       └── movie-card.component.ts
│   ├── genero/
│   │   ├── pages/
│   │   │   └── genero-page.component.ts
│   │   └── components/
│   │       └── genre-banner.component.ts
│   ├── peliculas/
│   │   ├── pages/
│   │   │   ├── movie-detail-page.component.ts
│   │   │   └── favoritos-page.component.ts
│   │   └── components/
│   │       └── trailer-dialog.component.ts
│   └── ui/
│       └── skeleton.component.ts
├── models/                        # Interfaces TypeScript (Movie, Genero, PaginatedResponse)
├── app.component.ts               # Bootstrap: solo <router-outlet />
├── app.config.ts                  # Providers globales (router, http, animaciones)
└── app.routes.ts                  # Configuración de rutas con lazy loading
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
// models/pelicula.model.ts
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

## Paso 2: Servicio (`core/services/`)

Los servicios HTTP se colocan en `core/services/` y usan `inject()` con `providedIn: 'root'`:

```typescript
// core/services/pelicula.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Pelicula } from '../../models/pelicula.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PeliculaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrlBase}/Peliculas`;

  getAll(): Observable<Pelicula[]> {
    return this.http.get<Pelicula[]>(this.baseUrl);
  }

  getById(id: number): Observable<Pelicula> {
    return this.http.get<Pelicula>(`${this.baseUrl}/${id}`);
  }

  create(data: CreatePelicula): Observable<Pelicula> {
    return this.http.post<Pelicula>(this.baseUrl, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
```

> ⚠️ La ruta de importación de `environment` es `../../../environments/environment` desde `core/services/`.

---

## Paso 3: Componente

Cada página que es una ruta va dentro de `pages/` y los subcomponentes reutilizables dentro de `components/`.

Usa **señales** para el estado local:

```typescript
// features/peliculas/pages/listado-peliculas/listado-peliculas.component.ts
import { Component, inject, signal, OnInit } from '@angular/core';
import { PeliculaService } from '../../../core/services/pelicula.service';
import { Pelicula } from '../../../models/pelicula.model';

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

### Convención de importaciones

| El archivo está en... | Ruta a `models/` | Ruta a `core/services/` | Ruta a `shared/` |
|---|---|---|---|
| `features/xxx/pages/` | `../../../models/` | `../../../core/services/` | `../../../shared/` |
| `features/xxx/components/` | `../../../models/` | `../../../core/services/` | `../../../shared/` |
| `core/layout/` | `../../models/` | `../services/` | `../../shared/` |

---

## Paso 4: Rutas

Las rutas siguen un patrón de **shell + children** con guards de autenticación:

```typescript
// app.routes.ts
export const routes: Routes = [
  { path: '', redirectTo: '/inicio', pathMatch: 'full' },
  {
    path: 'login',
    canActivate: [guestGuard],          // Solo accesible sin sesión
    loadComponent: () =>
      import('./features/auth/pages/login-page.component').then(c => c.LoginPageComponent),
  },
  {
    path: '',
    loadComponent: () =>
      import('./core/layout/shell.component').then(c => c.ShellComponent),
    canActivate: [authGuard],           // Requiere sesión
    children: [
      {
        path: 'inicio',
        loadComponent: () =>
          import('./features/home/pages/home-page.component').then(c => c.HomePageComponent),
      },
      {
        path: 'genero/:nombre',
        loadComponent: () =>
          import('./features/genero/pages/genero-page.component').then(c => c.GeneroPageComponent),
      },
      {
        path: 'pelicula/:id',
        loadComponent: () =>
          import('./features/peliculas/pages/movie-detail-page.component').then(c => c.MovieDetailPageComponent),
      },
      {
        path: 'favoritos',
        loadComponent: () =>
          import('./features/peliculas/pages/favoritos-page.component').then(c => c.FavoritosPageComponent),
      },
      { path: '**', redirectTo: '/inicio' },
    ],
  },
];
```

### Cómo funciona el flujo de autenticación

1. **Usuario no logueado** intenta acceder a `/inicio` → `authGuard` redirige a `/login`
2. **Usuario no logueado** intenta acceder a `/login` → `guestGuard` permite el acceso
3. **Usuario logueado** inicia sesión → `LoginPageComponent` llama a `router.navigate(['/inicio'])`
4. **Usuario logueado** accede a `/login` → `guestGuard` redirige a `/inicio`
5. **Usuario logueado** cierra sesión → `AuthService.logout()` navega a `/login`

### Guards

- **`authGuard`**: Comprueba `auth.isLoggedIn()`. Si no hay token, redirige a `/login`.
- **`guestGuard`**: Comprueba que NO haya sesión. Si hay token, redirige a `/inicio`.

### ShellComponent

`ShellComponent` proporciona el layout común (navbar + router-outlet + footer) para todas las rutas autenticadas:

```html
<div class="app-shell">
  <app-navbar />
  <router-outlet />
  <mat-divider />
  <footer>...</footer>
</div>
```

### lazy loading + params de ruta

Gracias a `withComponentInputBinding()` en `app.config.ts`, los parámetros de ruta se vinculan automáticamente a inputs del componente:

```typescript
// Ruta: /pelicula/:id
// MovieDetailPageComponent recibe automáticamente:
readonly id = input.required<string>();    // "id" del route param
```

---

## Auth Interceptor

El interceptor `authInterceptor` (en `core/interceptors/`) añade el token JWT a cada petición y fuerza logout si recibe un 401:

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.getToken();

  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(req).pipe(
    catchError((err) => {
      if (err.status === 401) auth.logout();
      return throwError(() => err);
    })
  );
};
```

---

## Configurar HttpClient

> ✅ `HttpClient` ya está configurado en `app.config.ts` con `provideHttpClient(withFetch(), withInterceptors([...]))`.

```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding(), withInMemoryScrolling({ scrollPositionRestoration: 'top' })),
    provideHttpClient(withFetch(), withInterceptors([authErrorInterceptor, authInterceptor])),
    provideAnimations(),
  ],
};
```

---

## Angular Material

> ✅ Angular Material ya está instalado (v22) con un tema M3 custom oscuro (paleta roja Netflix, fondo #141414). El tema se configura en `src/styles.scss` mediante `mat.define-theme()`.

Componentes que se usan actualmente:

| Componente | Uso |
|---|---|
| `<mat-toolbar>` | Barra de navegación (NavbarComponent) |
| `<mat-card>` | Tarjetas de película (MovieCardComponent, favoritos) |
| `<mat-menu>` | Menú de géneros desktop + hamburguesa móvil |
| `<mat-chip-set>` / `<mat-chip>` | Etiquetas de género en héroe y cards |
| `<mat-icon>` | Iconos (search, play_arrow, account_circle, home, etc.) |
| `<mat-divider>` | Separadores en footer y menú hamburguesa |
| `<mat-button>` / `<mat-icon-button>` / `<mat-raised-button>` / `<mat-stroked-button>` | Botones de navegación y acciones |
| `<mat-dialog>` | Modal de registro (RegisterDialogComponent) y tráiler (TrailerDialogComponent) |
| `<mat-snack-bar>` | Notificaciones toast en login, registro, errores de favoritos |
| `<mat-form-field>` + `<mat-input>` | Campos de formulario con validación en login y registro |
| `<mat-tooltip>` | Tooltips en botones de acción |

---

## Errores frecuentes

| Error | Causa | Solución |
|---|---|---|
| `NullInjectorError: No provider for HttpClient` | No llamaste a `provideHttpClient()` en `app.config.ts` | Añadirlo |
| `Cannot find a differ supporting object '...'` | Usaste `*ngFor` con un signal en lugar de llamarlo como función | Usar `@for` de Angular 17+ o `peliculas()` |
| 404 al llamar a la API | La URL del backend es incorrecta o CORS no permite el origen | Verificar puerto (`7154`) y policy CORS en `Program.cs` |
| El componente no aparece | No está importado en el template o la ruta no está registrada | Comprobar `imports` del componente y `app.routes.ts` |
| `ng test` falla | Angular 22 usa Vitest por defecto | Asegúrate de tener `@angular/build` actualizado; el comando es `ng test` sin flags |
| Error 401 en peticiones autenticadas | Token expirado o no enviado | El `authInterceptor` renueva/limpia la sesión automáticamente |
| El navbar no muestra los géneros | `GeneroService` en navbar hace la petición al cargar el shell | Verificar que el endpoint `GET /api/Generos` funciona |

---

## Comandos útiles *(todos en Terminal normal)*

| Comando | Qué hace |
|---|---|
| `ng serve` | Arrancar servidor de desarrollo (`http://localhost:4200`) |
| `ng g c features/peliculas/pages/mi-pagina` | Generar página (componente de ruta) |
| `ng g c features/peliculas/components/mi-componente` | Generar subcomponente de UI |
| `ng g s core/services/mi-servicio` | Generar servicio en core |

---

### Después del merge

Cuando tu PR se fusione, borra la rama y crea una nueva desde `main` actualizado.
Ver [`CONTRIBUTING.md`](../CONTRIBUTING.md#despu%C3%A9s-del-merge-c%C3%B3mo-empezar-la-siguiente-iteraci%C3%B3n).
