# Guía de Calidad — MovieHub

Rol asignado: **[@pablorequinto95-dotcom](https://github.com/pablorequinto95-dotcom)** — rama `feature/testing`

---

## Tus responsabilidades

1. Tests (backend y frontend)
2. Documentación
3. GitHub Actions (CI/CD)
4. Revisión de Pull Requests

---

## 1. Tests del backend — **Completado** ✅

> ✅ El proyecto `MovieHubAPI.Tests` existe con **61 tests** (unitarios de servicios, validadores e integración de controladores). La ejecución se hace con `dotnet test MovieHubAPI/MovieHubAPI.Tests`.

---

### Tecnología (propuesta)

- **xUnit** (framework de tests)
- **Moq** (mocking de dependencias)
- Crear el proyecto: `dotnet new xunit -n MovieHubAPI.Tests`

### Estructura (propuesta)

```
MovieHubAPI/MovieHubAPI.Tests/
├── Unitarias/
│   ├── Services/
│   │   ├── PeliculaServiceTests.cs
│   │   ├── GeneroServiceTests.cs
│   │   ├── FavoritoServiceTests.cs
│   │   └── ValoracionServiceTests.cs
│   └── Validators/
│       ├── CreatePeliculaValidatorTests.cs
│       └── CreateGeneroValidatorTests.cs
└── Integracion/
    └── PeliculasControllerTests.cs
```

### Qué testear

| Tipo | Qué cubrir | Ejemplo |
|---|---|---|
| **CRUD** | Crear, leer, actualizar, eliminar | `CrearPelicula_ConDatosValidos_RetornaOk` |
| **Validaciones** | Errores de validación | `CrearPelicula_ConTituloVacio_RetornaBadRequest` |
| **Consultas** | Filtros, búsquedas | `BuscarPorTitulo_DevuelveResultados` |
| **Valoraciones** | Puntuación 1-5, media automática | `Valorar_ConPuntuacion6_RetornaError` |
| **Favoritos** | Añadir, eliminar, listar | `EliminarFavorito_NoExistente_RetornaNotFound` |

### Ejemplo de test

```csharp
public class PeliculaServiceTests
{
    [Fact]
    public async Task GetAllPaginadoAsync_CuandoExistenPeliculas_RetornaPagina()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MovieHubContext>()
            .UseInMemoryDatabase(databaseName: "TestDB")
            .Options;

        using var context = new MovieHubContext(options);
        context.Peliculas.Add(new Pelicula { Titulo = "Test" });
        await context.SaveChangesAsync();

        var service = new PeliculaService(context);

        // Act
        var result = await service.GetAllPaginadoAsync(1, 10);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
    }
}
```

> ⚠️ Para tests de integración no uses la BD real. Usa `UseInMemoryDatabase()`.

### Cómo ejecutar los tests

**Desde Visual Studio:** Test → Test Explorer → Run All, o botón derecho en el proyecto → Run Tests

**Por terminal** *(Terminal normal — View → Terminal en VS, Ctrl+` en VS Code, PowerShell, CMD o Windows Terminal)*:

```bash
cd MovieHubAPI/MovieHubAPI.Tests
dotnet test
dotnet test --filter "FullyQualifiedName~Valoracion"   # Filtrar por nombre
dotnet test --verbosity detailed                       # Más información en fallos
```

---

## 2. Tests del frontend — **En progreso** 🟡

### Tecnología

Angular 22 usa **Vitest** (instalado y configurado). El proyecto tiene `vitest.config.ts` con:
- `globals: true` (describe, it, expect disponibles globalmente)
- `environment: 'jsdom'` (simulación de navegador)
- `setupFiles: ['src/test-setup.ts']` (inicialización de TestBed)

### Tests existentes

| Archivo | Tests | Estado |
|---|---|---|
| `core/services/auth.service.spec.ts` | 6 tests: login, register, logout, token, persistencia | ✅ |

### Cómo ejecutar los tests

```bash
npx vitest run              # Una vez
npx vitest                  # Modo watch
npx vitest run --reporter verbose  # Con detalle
```

### Pendiente

- Tests de servicios (movie, genero, favorito, valoracion)
- Tests de componentes (star-rating, favorito-button, login, home, detail)
- Tests de pipes (truncate, rating-percent)

---

## 3. GitHub Actions (CI/CD) — Configurado

> ✅ Pipeline completa con **2 jobs**: backend (build + test) y frontend (build + test) en cada push/PR a `main`.

### Pipeline actual (`.github/workflows/build.yml`)

```yaml
name: CI
on:
  push: { branches: [main] }
  pull_request: { branches: [main] }
jobs:
  backend:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v3 { with: { dotnet-version: '10.0.x' } }
      - run: dotnet restore MovieHubAPI/MovieHubAPI.slnx
      - run: dotnet build MovieHubAPI/MovieHubAPI.slnx
      - run: dotnet test MovieHubAPI/MovieHubAPI.Tests --verbosity normal

  frontend:
    runs-on: windows-latest
    defaults: { run: { working-directory: MovieHubAngular } }
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v3 { with: { node-version: '22' } }
      - run: npm ci
      - run: npx ng build --configuration production
      - run: npx vitest run --reporter verbose
```

---

## 4. Revisión de Pull Requests

Eres el revisor de los PRs de tus compañeros. Usa este checklist:

### Checklist de revisión

**Código:**
- [ ] ¿Sigue la estructura del proyecto? (modelo → DTO → servicio → controlador)
- [ ] ¿Los nombres de clases, métodos y variables son descriptivos?
- [ ] ¿No hay código comentado o `console.log`?
- [ ] ¿Las rutas de Angular usan lazy loading?
- [ ] ¿Los endpoints devuelven los códigos HTTP correctos?
- [ ] ¿Los DTOs no exponen entidades EF Core?

**Seguridad:**
- [ ] ¿Los endpoints sensibles requieren autenticación?
- [ ] ¿No se exponen contraseñas o tokens en el código?

**Documentación:**
- [ ] ¿El PR tiene descripción clara?
- [ ] Si hay cambios visuales, ¿incluye capturas?

---

## 5. Documentación

Mantén actualizadas las guías en `docs/` y el README principal.

Si ves que una guía se queda obsoleta o alguien comete un error que no estaba cubierto, actualízala con el nuevo caso.

---

## Resumen de comandos

| Comando | Consola | Qué hace |
|---|---|---|---|
| `dotnet test MovieHubAPI/MovieHubAPI.Tests` | Terminal normal | Ejecutar tests backend |
| `ng test` | Terminal normal | Ejecutar tests frontend (Vitest) |
| `dotnet ef migrations has-pending-model-changes` | Package Manager Console | Verificar si faltan migraciones |

---

### Después del merge

Cuando tu PR se fusione, borra la rama y crea una nueva desde `main` actualizado.
Ver [`CONTRIBUTING.md`](../CONTRIBUTING.md#despu%C3%A9s-del-merge-c%C3%B3mo-empezar-la-siguiente-iteraci%C3%B3n).
