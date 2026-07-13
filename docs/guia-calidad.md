# Guía de Calidad — MovieHub

Rol asignado: **[@pablorequinto95-dotcom](https://github.com/pablorequinto95-dotcom)** — rama `feature/testing`

---

## Tus responsabilidades

1. Tests (backend y frontend)
2. Documentación
3. GitHub Actions (CI/CD)
4. Revisión de Pull Requests

---

## 1. Tests del backend — **Pendiente de crear**

> ⚠️ El proyecto `MovieHubAPI.Tests` **no existe aún**. Está por crear.

### Tecnología (propuesta)

- **xUnit** (framework de tests)
- **Moq** (mocking de dependencias)
- Crear el proyecto: `dotnet new xunit -n MovieHubAPI.Tests`

### Estructura (propuesta)

```
MovieHubAPI.Tests/
├── Services/
│   ├── PeliculaServiceTests.cs
│   └── ValoracionServiceTests.cs
├── Controllers/
│   ├── PeliculasControllerTests.cs
│   └── ...
└── Validators/
    └── CreatePeliculaValidatorTests.cs
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
cd MovieHubAPI.Tests
dotnet test
dotnet test --filter "FullyQualifiedName~Valoracion"   # Filtrar por nombre
dotnet test --verbosity detailed                       # Más información en fallos
```

---

## 2. Tests del frontend

### Tecnología

Angular 22 usa **Vitest** por defecto (el `tsconfig.spec.json` referencia `vitest/globals`). No hace falta instalar Karma, Jasmine ni Jest.

### Qué testear

| Tipo | Ejemplo |
|---|---|
| **Componentes** | "Al hacer clic en estrella, llama al servicio" |
| **Servicios** | "getAll() devuelve lista de películas mockeadas" |
| **Pipes/Filtros** | "filtroPorGenero filtra correctamente" |

### Ejemplo con Vitest

```typescript
import { describe, it, expect, beforeEach } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ListadoPeliculasComponent } from './listado-peliculas.component';

describe('ListadoPeliculasComponent', () => {
  let component: ListadoPeliculasComponent;
  let fixture: ComponentFixture<ListadoPeliculasComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListadoPeliculasComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ListadoPeliculasComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
```

### Cómo ejecutar los tests

**Por terminal** *(Terminal normal)*:

```bash
ng test
```

---

## 3. GitHub Actions (CI/CD) — Configurado con SonarCloud

> ✅ Ya existe el archivo `.github/workflows/build.yml` con análisis de SonarCloud para el backend.
> Se ejecuta en pushes y PRs a `main` usando `windows-latest`, .NET 10 y Node.js 24.

### Pipeline actual (`.github/workflows/build.yml`)

El workflow actual analiza el backend con SonarCloud en cada push/PR a `main`:

```yaml
name: SonarCloud Full Stack Analysis
on:
  push: { branches: [main] }
  pull_request: { branches: [main] }
jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-java@v3 { with: { java-version: 17 } }
      - uses: actions/setup-dotnet@v3 { with: { dotnet-version: '10.0.x' } }
      - uses: actions/setup-node@v3 { with: { node-version: '24' } }
      - run: dotnet restore MovieHubAPI/MovieHubAPI.slnx
      - run: dotnet build MovieHubAPI/MovieHubAPI.slnx
      - uses: SonarSource/sonarcloud-github-action
```

> 💡 Pendiente añadir test coverage y compilación del frontend a la pipeline.

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
| `dotnet test MovieHubAPI.Tests` | Terminal normal | Ejecutar tests backend (cuando exista el proyecto) |
| `ng test` | Terminal normal | Ejecutar tests frontend (Vitest) |
| `dotnet ef migrations has-pending-model-changes` | Package Manager Console | Verificar si faltan migraciones |

---

### Después del merge

Cuando tu PR se fusione, borra la rama y crea una nueva desde `main` actualizado.
Ver [`CONTRIBUTING.md`](../CONTRIBUTING.md#despu%C3%A9s-del-merge-c%C3%B3mo-empezar-la-siguiente-iteraci%C3%B3n).
