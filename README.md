# MovieHub

Plataforma web para gestionar un catálogo de películas: exploración, valoraciones, favoritos, búsquedas y estadísticas.

## Estado actual del proyecto

### Implementado
- **Backend:** CRUD completo de Películas y Géneros (modelos, DTOs, servicios, controladores)
- **Base de datos:** Migración inicial con Identity (`UsuarioModel` hereda de `IdentityUser<long>`), tablas `AspNet*`, relaciones N:M con claves compuestas, seed data de 245 películas y 30 géneros
- **Mapping:** Configuración de Mapster para DTOs de Películas (`MappingConfig.cs`)
- **Validación:** FluentValidation con filtro global (`ValidationFilter`) y validadores para Películas y Géneros
- **Middleware:** Manejo global de excepciones con `ExceptionHandlingMiddleware` (errores inesperados devuelven JSON ProblemDetails)
- **Paginación:** Endpoint `GET /api/peliculas` con `?page=&pageSize=` y DTO genérico `PaginadosDto<T>`
- **Swagger:** Documentación interactiva disponible en `/swagger`
- **CORS:** Permitido para `http://localhost:4200`

### Pendiente
- **Autenticación JWT:** Paquetes instalados pero código comentado en `Program.cs`
- **Tests:** Proyecto de tests no creado
- **Pipeline CI:** `.github/workflows/` vacío
- **Frontend:** Proyecto Angular scaffolded pero sin componentes de negocio
- **Funcionalidades extra:** Valoraciones, Favoritos, Estadísticas, Búsqueda/Filtros — pendientes de implementar

> Hasta que la pipeline y los tests estén operativos, cada Pull Request debe probarse manualmente antes de fusionar.

## Descripción

MovieHub permite:

- Explorar un catálogo de películas, con ficha individual de cada una
- Valorar películas (1-5 estrellas), con cálculo automático de la puntuación media
- Marcar y eliminar películas de favoritos
- Buscar por título y filtrar por género
- Consultar rankings y estadísticas (mejor valoradas, más recientes...)

## Tecnologías

- **Frontend:** Angular 22 (standalone), SCSS, Vitest
- **Backend:** ASP.NET Core Web API (.NET 10), Entity Framework Core 10, Mapster, FluentValidation
- **Base de datos:** SQL Server, Identity (ASP.NET Core Identity)
- **Autenticación:** JWT (pendiente de activar)
- **Documentación API:** Swagger / Swashbuckle
- **CI/CD:** Git, GitHub, GitHub Actions (pendiente de configurar)

## Estructura del repositorio

```
MovieHub/
├── MovieHubAPI/          # Backend - ASP.NET Core Web API
├── MovieHubAngular/      # Frontend - Angular
├── docs/                 # Guías por rol
├── .github/workflows/    # CI/CD (pendiente)
├── README.md
└── CONTRIBUTING.md
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

### Película

| Campo | Detalle |
|---|---|
| título | — |
| descripción | — |
| género(s) | relación N:M con Género |
| director | — |
| año de estreno | — |
| duración | minutos |
| puntuación media | calculada a partir de las valoraciones |
| imagen | póster |

CRUD completo: crear, modificar, eliminar, consultar.

### Géneros

Ejemplos: Acción, Comedia, Drama, Terror, Ciencia ficción, Documental, Animación. CRUD completo.

### Usuarios

Registro, modificación de perfil, marcar películas como favoritas, valorar películas. Valorar y gestionar favoritos requiere usuario autenticado.

### Valoraciones

1 a 5 estrellas por usuario y película. La puntuación media se recalcula automáticamente.

### Favoritos

Añadir o eliminar películas de la lista de favoritos del usuario.

### Consultas mínimas

- mejor valoradas
- más recientes (por año de estreno)
- por género
- búsqueda por título
- estadísticas generales (total de películas, media global)

## Frontend Angular

- Página principal
- Menú de navegación
- Listado de películas
- Ficha individual
- Listado de géneros
- Formulario de alta y edición
- Buscador y filtros
- Favoritos
- Valoración mediante estrellas

Se valorará el uso de Angular Material.

## Base de datos

- Película – Género: N:M
- Usuario – Valoración: 1:N
- Usuario – Favoritos: N:M
- Claves foráneas correctas, migraciones y datos iniciales (seed)

## Requisitos previos

- .NET SDK 10.0 o superior
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

Asegúrate de tener SQL Server disponible. Las migraciones ya están aplicadas; si necesitas regenerarlas:

```bash
dotnet ef database update
```

Ejecutar la API:

```bash
dotnet run
```

Disponible en `https://localhost:7154`.

- 🌐 **Documentación de la API (Swagger):** [https://localhost:7154/swagger](https://localhost:7154/swagger)

### 3. Frontend (MovieHubAngular)

```bash
cd MovieHubAngular
npm install
ng serve
```

Disponible en `http://localhost:4200`.

## Tests

*(Pendiente de implementación — ver "Estado actual del proyecto".)*

El proyecto de tests está por crear. Se usarán:
- **Backend:** xUnit + Moq
- **Frontend:** Vitest (integrados en Angular CLI)

## Flujo de trabajo con Git

Resumen rápido:

1. Crear rama desde `main` (o usar la ya asignada).
2. Commits frecuentes con mensajes claros.
3. `git push origin <tu-rama>`.
4. Abrir Pull Request (descripción, capturas, pruebas realizadas).
5. Revisión cruzada por otro integrante.
6. Merge en `main` y eliminación de la rama.

Para las normas detalladas y cómo evitar conflictos, ver [`CONTRIBUTING.md`](./CONTRIBUTING.md).

---

📖 **Guías por rol** → [`docs/`](./docs/)

| Guía | Para | Contenido |
|---|---|---|
| [`guia-bd.md`](./docs/guia-bd.md) | Base de datos | Migraciones EF Core, relaciones, seed, errores típicos |
| [`guia-backend.md`](./docs/guia-backend.md) | Backend | Pipeline modelo→DTO→servicio→controlador, validación |
| [`guia-frontend.md`](./docs/guia-frontend.md) | Frontend | Estructura Angular, componentes, servicios, rutas |
| [`guia-funcionalidades.md`](./docs/guia-funcionalidades.md) | Funcionalidades | Features que tocan ambas capas (valoraciones, favoritos...) |
| [`guia-calidad.md`](./docs/guia-calidad.md) | Calidad | Tests, CI/CD, revisión de PRs |
