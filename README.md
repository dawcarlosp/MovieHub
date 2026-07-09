# MovieHub

Plataforma web para gestionar un catálogo de películas y series: exploración, valoraciones, favoritos, búsquedas y estadísticas.

## Estado actual del proyecto

> La pipeline de CI (`.github/workflows/`) ya está creada, pero los tests todavía no están implementados (pendiente del integrante de Calidad). Hasta entonces, cada Pull Request debe probarse manualmente antes de fusionar.

## Descripción

MovieHub permite:

- Explorar un catálogo de películas y series, con ficha individual de cada obra
- Valorar obras (1-5 estrellas), con cálculo automático de la puntuación media
- Marcar y eliminar obras de favoritos
- Buscar por título y filtrar por género o productora
- Consultar rankings y estadísticas (mejor valoradas, más recientes, ranking de productoras...)

## Tecnologías

- **Frontend:** Angular, Angular Material
- **Backend:** ASP.NET Core Web API, Entity Framework Core
- **Base de datos:** SQL Server
- **CI/CD:** Git, GitHub, GitHub Actions

## Estructura del repositorio

```
MovieHub/
├── MovieHubAPI/        # Backend - ASP.NET Core Web API
├── MovieHubAngular/    # Frontend - Angular
└── README.md
```

## Equipo y reparto de trabajo

| Rol | Integrante | Rama | Responsabilidad |
|---|---|---|---|
| Backend | [@dawcarlosp](https://github.com/dawcarlosp) | `feature/backend` | Entidades, DTOs, servicios, controladores, validaciones, endpoints REST |
| Base de datos | [@P4bM4rx](https://github.com/P4bM4rx) | `feature/database` | Modelo relacional, migraciones, relaciones, restricciones, seed |
| Frontend | [@claauudiiaacr](https://github.com/claauudiiaacr) | `feature/frontend` | Componentes, routing, formularios, consumo de la API, diseño |
| Funcionalidades | [@Anua472](https://github.com/Anua472) | `feature/features` | Favoritos, valoraciones, filtros, búsquedas, rankings, estadísticas (toca Angular y ASP.NET) |
| Calidad | [@pablorequinto95-dotcom](https://github.com/pablorequinto95-dotcom) | `feature/testing` | Tests, documentación, GitHub Actions, revisión de PRs |

> No se trabaja directamente sobre `main`. Todo el desarrollo se realiza en la rama correspondiente y se integra mediante Pull Request. El flujo detallado y las normas para evitar conflictos están en [`CONTRIBUTING.md`](./CONTRIBUTING.md).

## Modelo de datos / funcionalidades mínimas

### Obra (película o serie)

| Campo | Detalle |
|---|---|
| título | — |
| descripción | — |
| tipo | película / serie |
| género(s) | relación N:M con Género |
| director/creador | — |
| productora | relación N:1 con Productora |
| año de estreno | — |
| duración | minutos, o nº de temporadas/episodios si es serie |
| puntuación media | calculada a partir de las valoraciones |
| imagen | póster |

CRUD completo: crear, modificar, eliminar, consultar.

### Productoras

nombre, país, año de fundación, descripción. CRUD completo.

### Géneros

Ejemplos: Acción, Comedia, Drama, Terror, Ciencia ficción, Documental, Animación. CRUD completo.

### Usuarios

Registro, modificación de perfil, marcar obras como favoritas, valorar obras. Valorar y gestionar favoritos requiere usuario autenticado.

### Valoraciones

1 a 5 estrellas por usuario y obra. La puntuación media se recalcula automáticamente.

### Favoritos

Añadir o eliminar obras de la lista de favoritos del usuario.

### Consultas mínimas

- mejor valoradas
- más recientes (por año de estreno)
- por género
- por productora
- búsqueda por título
- ranking de productoras con mejor valoración
- estadísticas generales (total de obras, media global, distribución por tipo)

## Frontend Angular

- Página principal
- Menú de navegación
- Listado de obras (películas/series)
- Ficha individual
- Listado de productoras
- Listado de géneros
- Formulario de alta y edición
- Buscador y filtros
- Favoritos
- Valoración mediante estrellas

Se valorará el uso de Angular Material.

## Base de datos

- Obra – Productora: N:1
- Obra – Género: N:M
- Usuario – Valoración: 1:N
- Usuario – Favoritos: N:M
- Claves foráneas correctas, migraciones y datos iniciales (seed)

## Requisitos previos

- .NET SDK 8.0 o superior
- Node.js 18+ y npm
- Angular CLI (`npm install -g @angular/cli`)
- SQL Server (local o en contenedor Docker)

## Instalación y ejecución

### 1. Clonar el repositorio

```bash
git clone https://github.com/<usuario>/MovieHub.git
cd MovieHub
```

### 2. Backend (MovieHubAPI)

```bash
cd MovieHubAPI
```

Configurar la cadena de conexión en `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=MovieHubDB;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Aplicar las migraciones (crea la base de datos y los datos iniciales):

```bash
dotnet ef database update
```

Ejecutar la API:

```bash
dotnet run
```

Disponible en `https://localhost:xxxx` (puerto definido en `launchSettings.json`).

- 🌐 **Documentación de la API (Swagger Classic):** [https://localhost:XXXX/swagger](https://localhost:XXXX/swagger) *(Recuerda cambiar `XXXX` por tu puerto HTTPS local)*

### 3. Frontend (MovieHubAngular)

```bash
cd MovieHubAngular
npm install
ng serve
```

Disponible en `http://localhost:4200`.

## Tests

```bash
cd MovieHubAPI
dotnet test
```

Cubrirán: CRUD de obras, validaciones, consultas, favoritos y valoraciones. *(Pendiente de implementación — ver "Estado actual".)*

## Flujo de trabajo con Git

Resumen rápido:

1. Crear rama desde `main` (o usar la ya asignada).
2. Commits frecuentes con mensajes claros.
3. `git push origin <tu-rama>`.
4. Abrir Pull Request (descripción, capturas, pruebas realizadas).
5. Revisión cruzada por otro integrante.
6. Merge en `main` y eliminación de la rama.

Para las normas detalladas y cómo evitar conflictos, ver [`CONTRIBUTING.md`](./CONTRIBUTING.md).
