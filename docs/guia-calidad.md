# Guía de Calidad — MovieHub

Rol asignado: **[@pablorequinto95-dotcom](https://github.com/pablorequinto95-dotcom)** — rama `feature/testing`

---

## Tus responsabilidades

1. Tests (backend y frontend)
2. Documentación
3. GitHub Actions (CI/CD)
4. Revisión de Pull Requests

---

## 1. Tests del backend

### Tecnología

- **xUnit** (framework de tests)
- **Moq** (mocking de dependencias)
- El proyecto de tests se crea aparte: `dotnet new xunit -n MovieHubAPI.Tests`

### Estructura

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
    public async Task GetAllAsync_CuandoExistenPeliculas_RetornaLista()
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
        var result = await service.GetAllAsync();

        // Assert
        Assert.Single(result);
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

Por defecto Angular usa **Karma + Jasmine**, pero se recomienda migrar a **Jest** (más rápido, mejor DX).

Para configurar Jest *(Terminal normal)*:

```bash
npm remove karma karma-chrome-launcher karma-jasmine karma-jasmine-html-reporter
npm install --save-dev jest @types/jest
```

O puedes mantener Jasmine si el equipo prefiere no cambiar.

### Qué testear

| Tipo | Ejemplo |
|---|---|
| **Componentes** | "Al hacer clic en estrella, llama al servicio" |
| **Servicios** | "getAll() devuelve lista de películas mockeadas" |
| **Pipes/Filtros** | "filtroPorGenero filtra correctamente" |

### Ejemplo con Jasmine

```typescript
describe('ListadoPeliculasComponent', () => {
  let component: ListadoPeliculasComponent;
  let fixture: ComponentFixture<ListadoPeliculasComponent>;
  let mockService: jasmine.SpyObj<PeliculaService>;

  beforeEach(() => {
    mockService = jasmine.createSpyObj('PeliculaService', ['getAll']);
    mockService.getAll.and.returnValue(of([{ id: 1, titulo: 'Test' }]));

    TestBed.configureTestingModule({
      imports: [ListadoPeliculasComponent],
      providers: [{ provide: PeliculaService, useValue: mockService }],
    });

    fixture = TestBed.createComponent(ListadoPeliculasComponent);
    component = fixture.componentInstance;
  });

  it('should load peliculas on init', () => {
    fixture.detectChanges();
    expect(component.peliculas().length).toBe(1);
  });
});
```

### Cómo ejecutar los tests

**Desde Visual Studio:** Test Explorer también detecta tests de Angular si configuras la integración.

**Por terminal** *(Terminal normal — View → Terminal en VS, Ctrl+` en VS Code, PowerShell, CMD o Windows Terminal)*:

```bash
ng test                    # Karma
npx jest                   # Si migraste a Jest
```

---

## 3. GitHub Actions (CI/CD)

La pipeline ya está creada en `.github/workflows/`. Revísala y completa lo que falte.

### Estructura típica

```yaml
name: CI

on:
  push:
    branches: [main, feature/*]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: '22'
      
      - name: Restore dependencies
        run: dotnet restore MovieHubAPI
      
      - name: Build
        run: dotnet build MovieHubAPI --no-restore
      
      - name: Test
        run: dotnet test MovieHubAPI --no-build --verbosity normal
```

### Checklist para la pipeline

- [ ] Compila el backend (`dotnet build` *(Terminal normal)*)
- [ ] Compila el frontend (`ng build` *(Terminal normal)*)
- [ ] Pasan los tests del backend (`dotnet test` *(Terminal normal)* o Test Explorer)
- [ ] Pasan los tests del frontend (`ng test --watch=false --browsers=ChromeHeadless` *(Terminal normal)*)

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
|---|---|---|
| `dotnet test MovieHubAPI.Tests` | Terminal normal | Ejecutar tests backend |
| `ng test --watch=false` | Terminal normal | Ejecutar tests frontend |
| `dotnet ef migrations has-pending-model-changes` | Terminal normal | Verificar si faltan migraciones |
| `act` | Terminal normal | Ejecutar pipeline GitHub Actions en local |

---

### Después del merge

Cuando tu PR se fusione, borra la rama y crea una nueva desde `main` actualizado.
Ver [`CONTRIBUTING.md`](../CONTRIBUTING.md#despu%C3%A9s-del-merge-c%C3%B3mo-empezar-la-siguiente-iteraci%C3%B3n).
