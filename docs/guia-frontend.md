# Guía de Frontend — MovieHub

Rol asignado: **[@claauudiiaacr](https://github.com/claauudiiaacr)** — rama `feature/frontend`

---

> 💡 **¿Package Manager Console o Terminal normal?** Esta guía usa solo la **Terminal normal** (View → Terminal en VS, Ctrl+` en VS Code, PowerShell, CMD o Windows Terminal).

## Tecnologías

- Angular 22 (standalone components, sin NgModules)
- Angular Material (instalar: `ng add @angular/material`)
- SCSS para estilos
- Señales (Signals) para estado reactivo

---

## Estructura de carpetas

```
src/app/
├── core/                      # Singleton services, interceptors, guards
│   ├── services/
│   │   └── api.service.ts     # HttpClient base
│   └── core.providers.ts
│
├── shared/                    # Componentes reutilizables (sin lógica de negocio)
│   ├── components/
│   │   ├── estrella-valoracion/
│   │   └── buscador/
│   └── pipes/
│
├── features/                  # Carpetas por funcionalidad
│   ├── peliculas/
│   │   ├── pages/
│   │   │   ├── listado-peliculas/
│   │   │   └── detalle-pelicula/
│   │   ├── components/
│   │   ├── services/
│   │   │   └── pelicula.service.ts
│   │   ├── models/
│   │   │   └── pelicula.model.ts
│   │   └── peliculas.routes.ts
│   ├── generos/
│   ├── auth/
│   └── favoritos/
│
├── app.component.ts
├── app.config.ts
└── app.routes.ts
```

---

## Orden de creación (para una nueva feature)

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

Y se registran en `app.routes.ts` con lazy loading:

```typescript
// app.routes.ts
export const routes: Routes = [
  { path: '', redirectTo: '/peliculas', pathMatch: 'full' },
  {
    path: 'peliculas',
    loadChildren: () =>
      import('./features/peliculas/peliculas.routes').then((r) => r.PELICULAS_ROUTES),
  },
  {
    path: 'generos',
    loadChildren: () =>
      import('./features/generos/generos.routes').then((r) => r.GENEROS_ROUTES),
  },
];
```

---

## Configurar HttpClient (NO lo olvides)

En `app.config.ts`:

```typescript
import { provideHttpClient, withFetch } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withFetch()),
    provideRouter(routes),
    // ...
  ],
};
```

---

## Angular Material

Instálalo si no lo está:

```bash
ng add @angular/material   # (Terminal normal)
```

Elige un tema, incluye tipografía y animaciones.

Componentes que seguramente uses:

| Componente | Uso |
|---|---|
| `<mat-toolbar>` | Barra de navegación |
| `<mat-card>` | Ficha de película |
| `<mat-form-field>` + `<mat-select>` | Filtros por género |
| `<mat-table>` | Listados |
| `<mat-icon>` | Iconos (estrella para valoración) |
| `<mat-dialog>` | Formularios de alta/edición |
| `<mat-progress-spinner>` | Cargando... |

---

## Errores frecuentes

| Error | Causa | Solución |
|---|---|---|
| `NullInjectorError: No provider for HttpClient` | No llamaste a `provideHttpClient()` en `app.config.ts` | Añadirlo |
| `Cannot find a differ supporting object '...'` | Usaste `*ngFor` con un signal en lugar de llamarlo como función | Usar `@for` de Angular 17+ o `peliculas()` |
| 404 al llamar a la API | La URL del backend es incorrecta o CORS no permite el origen | Verificar puerto en `launchSettings.json` y policy CORS en `Program.cs` |
| El componente no aparece | No está importado en el template o la ruta no está registrada | Comprobar `imports` del componente y `app.routes.ts` |

---

## Comandos útiles *(todos en Terminal normal)*

| Comando | Qué hace |
|---|---|
| `ng serve` | Arrancar servidor de desarrollo (`http://localhost:4200`) |
| `ng g c features/peliculas/pages/listado-peliculas --standalone` | Generar componente |
| `ng g s features/peliculas/services/pelicula` | Generar servicio |
| `ng add @angular/material` | Añadir Angular Material |

---

### Después del merge

Cuando tu PR se fusione, borra la rama y crea una nueva desde `main` actualizado.
Ver [`CONTRIBUTING.md`](../CONTRIBUTING.md#despu%C3%A9s-del-merge-c%C3%B3mo-empezar-la-siguiente-iteraci%C3%B3n).
