# Guía de Backend — MovieHub

Rol asignado: **[@dawcarlosp](https://github.com/dawcarlosp)** — rama `feature/backend`

---

> 💡 **¿Package Manager Console o Terminal normal?** Esta guía usa solo la **Terminal normal** (View → Terminal en VS, Ctrl+` en VS Code, PowerShell, CMD o Windows Terminal). La Package Manager Console solo la necesita el integrante de Base de Datos para migraciones.

## Pipeline de creación (orden correcto)

Cada nueva entidad sigue siempre estos pasos en este orden:

```
Modelo → DTO → Interface → Servicio → Controlador → Registro en Program.cs
```

No te saltes pasos ni los reordenes.

---

## Paso 1: Modelo (`Models/`)

Los modelos reales del proyecto usan el sufijo `Model` y relaciones con tabla intermedia explícita:

```csharp
public class PeliculaModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Director { get; set; }
    public int Anio { get; set; }              // año de estreno
    public int Duracion { get; set; }          // minutos
    public string? PosterUrl { get; set; }     // imagen/póster
    public double PuntuacionMedia { get; set; }

    // Relaciones (tabla intermedia PeliculaGeneroModel)
    public ICollection<PeliculaGeneroModel> PeliculaGeneros { get; set; }
    public ICollection<ValoracionModel> Valoraciones { get; set; }
    public ICollection<FavoritoModel> Favoritos { get; set; }
}
```

> ⚠️ Los `DbSet<>` ya están en `DbContext` (añadidos por Base de Datos).
> No modifiques el DbContext directamente sin coordinar con el integrante de BD.

---

## Paso 2: DTO (`DTOs/`)

Nunca expongas los modelos EF Core directamente. Usa **records** con Mapster:

```csharp
// MovieHubAPI/DTOs/Pelicula/PeliculaDto.cs
public record PeliculaDto(
    int Id,
    string Titulo,
    string Descripcion,
    int Duracion,
    int AnioEstreno,
    string Director,
    string? Imagen,
    double PuntuacionMedia,
    List<string> Generos
);
```

Los DTOs existentes se organizan por carpeta de entidad: `Pelicula/`, `Genero/`, `Usuario/`.

### Mapeo con Mapster (ya instalado)

Para convertir entre entidad y DTO usa Mapster en el servicio:

```csharp
using Mapster;

// En el servicio:
var peliculaDto = pelicula.Adapt<PeliculaDto>();
var genero = dto.Adapt<GeneroModel>();
```

Para configuraciones personalizadas, existe `MappingConfig.cs`:

```csharp
TypeAdapterConfig<PeliculaModel, PeliculaDto>.NewConfig()
    .Map(dest => dest.AnioEstreno, src => src.Anio)
    .Map(dest => dest.Imagen, src => src.PosterUrl)
    .Map(dest => dest.Generos, src => src.PeliculaGeneros.Select(pg => pg.Genero.Nombre).ToList());
```

> ✅ `MappingConfig.Configure()` ya está activo en `Program.cs`. Si añades un nuevo mapeo personalizado, añádelo en `MappingConfig.cs`.

---

## Paso 3: Interface (`Interfaces/`)

Define el contrato del servicio. Todas las interfaces están bajo el namespace `MovieHubAPI.Interfaces`:

```csharp
// IPeliculaService.cs
public interface IPeliculaService
{
    Task<PaginadosDto<PeliculaDto>> GetAllPaginadoAsync(int page, int pageSize);
    Task<PeliculaDto?> GetByIdAsync(int id);
    Task<PeliculaDto> CreateAsync(CreatePeliculaDto dto);
    Task<PeliculaDto?> UpdateAsync(int id, UpdatePeliculaDto dto);
    Task<bool> DeleteAsync(int id);
}
```

Interfaces existentes: `IPeliculaService`, `IGeneroService`, `IUsuarioService`.

---

## Paso 4: Servicio (`Services/`)

Toda la lógica de negocio va aquí. Los controladores **no** tienen lógica. Los servicios existentes usan `DbContext` (clase real del proyecto) y Mapster:

```csharp
public class PeliculaService : IPeliculaService
{
    private readonly MovieHubDbContext _context;

    public PeliculaService(MovieHubDbContext context) => _context = context;

    public async Task<PaginadosDto<PeliculaDto>> GetAllPaginadoAsync(int page, int pageSize)
    {
        var query = _context.Peliculas
            .Include(p => p.PeliculaGeneros)
            .ThenInclude(pg => pg.Genero)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var peliculas = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginadosDto<PeliculaDto>(
            peliculas.Adapt<List<PeliculaDto>>(),
            page,
            pageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        );
    }

    // ... resto de métodos
}
```

Servicios existentes: `PeliculaService`, `GeneroService`, `UsuarioService`.

---

## Paso 5: Controlador (`Controllers/`)

Los controladores son solo adaptadores HTTP. No contienen lógica de negocio:

```csharp
[ApiController]
[Route("api/[controller]")]
public class PeliculasController : ControllerBase
{
    private readonly IPeliculaService _peliculaService;

    public PeliculasController(IPeliculaService peliculaService)
    {
        _peliculaService = peliculaService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginadosDto<PeliculaDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageSize > 50) pageSize = 50;
        if (page < 1) page = 1;
        var resultado = await _peliculaService.GetAllPaginadoAsync(page, pageSize);
        return Ok(resultado);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PeliculaDto>> GetById(int id)
    {
        var pelicula = await _peliculaService.GetByIdAsync(id);
        if (pelicula is null) return NotFound();
        return Ok(pelicula);
    }

    [HttpPost]
    public async Task<ActionResult<PeliculaDto>> Create(CreatePeliculaDto dto)
    {
        var pelicula = await _peliculaService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = pelicula.Id }, pelicula);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PeliculaDto>> Update(int id, UpdatePeliculaDto dto)
    {
        var pelicula = await _peliculaService.UpdateAsync(id, dto);
        if (pelicula is null) return NotFound();
        return Ok(pelicula);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _peliculaService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
```

---

## Paso 6: Registrar en `Program.cs`

El registro del DbContext ya está hecho:

```csharp
builder.Services.AddDbContext<DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MovieHubConnection")));
```

Registra tus servicios:

```csharp
builder.Services.AddScoped<IPeliculaService, PeliculaService>();
builder.Services.AddScoped<IGeneroService, GeneroService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
```

> ✅ `MappingConfig.Configure()` ya está activo en `Program.cs`. Los mapeos personalizados
> (`Anio→AnioEstreno`, `PosterUrl→Imagen`, `PeliculaGeneros→List<string>`) se aplican automáticamente.

### Autenticación JWT

El proyecto incluye autenticación JWT con Identity. Actualmente está activo:
- Sección `Jwt` (Key/Issuer/Audience) en `appsettings.json`
- `AddIdentityCore<UsuarioModel>` en `Program.cs`
- Endpoints públicos de registro y login (`/api/Usuarios/register`, `/api/Usuarios/login`)

**Pendiente de activar** (cuando el equipo lo decida):

```csharp
// builder.Services.AddAuthentication(...)
// builder.Services.AddAuthorization(...)
// app.UseAuthentication();
```

Los endpoints `/me` (`GET`, `PUT`, `DELETE`) usan `?userId=` temporalmente. Cuando se active `[Authorize]`, se extraerá del JWT y la query param desaparecerá.

---

## Validación con FluentValidation

### Implementación actual

Los validadores se registran automáticamente via `AddValidatorsFromAssemblyContaining<Program>()` en `Program.cs` y se ejecutan mediante un filtro global `ValidationFilter` que:

1. Resuelve el `IValidator<T>` correspondiente al DTO recibido
2. Si falla la validación, devuelve `400 Bad Request` con errores agrupados por campo en formato `ValidationProblemDetails`

### Validadores existentes

| Validador | DTO | Reglas |
|-----------|-----|--------|
| `Validators/Pelicula/CreatePeliculaValidator.cs` | `CreatePeliculaDto` | Titulo (NotEmpty, max 200), Director (NotEmpty, max 150), Duracion (>0), AnioEstreno (1888-2030), Imagen (max 500), GeneroIds (NotEmpty, IDs >0) |
| `Validators/Pelicula/UpdatePeliculaValidator.cs` | `UpdatePeliculaDto` | Mismas reglas que Create |
| `Validators/Genero/CreateGeneroValidator.cs` | `CreateGeneroDto` | Nombre (NotEmpty, max 50) |
| `Validators/Genero/UpdateGeneroValidator.cs` | `UpdateGeneroDto` | Mismas reglas que Create |
| `Validators/Usuario/RegisterValidator.cs` | `RegisterDto` | UserName (NotEmpty, max 50), Email (NotEmpty, EmailAddress, max 100), Password (NotEmpty, min 6, al menos una mayúscula, minúscula y dígito) |
| `Validators/Usuario/LoginValidator.cs` | `LoginDto` | Email (NotEmpty, EmailAddress), Password (NotEmpty) |

### Cómo añadir un validador nuevo

```csharp
public class CreatePeliculaValidator : AbstractValidator<CreatePeliculaDto>
{
    public CreatePeliculaValidator()
    {
        RuleFor(p => p.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(p => p.Duracion).GreaterThan(0);
        RuleFor(p => p.AnioEstreno).InclusiveBetween(1888, 2030);
    }
}
```

No hace falta registrarlo manualmente — `AddValidatorsFromAssemblyContaining<Program>()` lo descubre automáticamente y el `ValidationFilter` lo ejecuta en cada request.

---

## ExceptionHandlingMiddleware

Middleware global que captura cualquier excepción no controlada en el pipeline y devuelve una respuesta JSON con formato ProblemDetails.

### Formato de respuesta

```json
{
  "status": 500,
  "title": "Error interno del servidor",
  "detail": "Ha ocurrido un error inesperado. Inténtalo de nuevo más tarde.",
  "traceId": "00-abc123..."
}
```

### Mapeo de excepciones

| Excepción | Status HTTP |
|-----------|-------------|
| `ArgumentException` | 400 |
| `KeyNotFoundException` | 404 |
| Cualquier otra | 500 |

### Comportamiento por entorno

- **Development:** el campo `detail` incluye el mensaje de la excepción y el stack trace completo.
- **Producción:** solo mensaje genérico, sin filtrar información interna.

### Ubicación

- `Middleware/ExceptionHandlingMiddleware.cs` — implementación.
- `Program.cs` — registrado al inicio del pipeline con `app.UseMiddleware<ExceptionHandlingMiddleware>()`.

---

## Errores frecuentes

| Error | Causa | Solución |
|---|---|---|---|
| `No service for type X has been registered` | Olvidaste registrar el servicio en Program.cs | Añadir `builder.Services.AddScoped<IX, X>()` |
| `Object reference not set to an instance of an object` | No hiciste `Include()` + `ThenInclude()` de las relaciones | Añadir `.Include(p => p.PeliculaGeneros).ThenInclude(pg => pg.Genero)` |
| `A possible object cycle was detected` | Relación circular en JSON | Usar DTOs planos (nunca expongas entidades EF directamente) |
| Swagger no carga | No está configurado | Verificar `app.UseSwagger()` y `UseSwaggerUI()` en Program.cs |
| 405 Method Not Allowed | Usaste `[HttpPost]` pero envías GET | Revisar el verbo HTTP en el endpoint |
| MappingConfig deja de tener efecto | Alguien borró la línea en `Program.cs` | Asegurar que `MappingConfig.Configure()` está antes de `var app = builder.Build();` |
| Error de compilación con `DbContext` | Conflicto con `System.Data.Common.DbContext` | Usar el namespace global o alias; nuestro `DbContext` no tiene namespace |
| El endpoint lanza excepción pero devuelve 500 genérico | El middleware captura errores no controlados | Revisar el log del backend para ver el detalle real |
| `POST /register` devuelve 400 con `"Passwords must have at least one uppercase"` | La contraseña no cumple las políticas de Identity | Usar al menos 6 caracteres, 1 mayúscula, 1 minúscula y 1 dígito |
| `POST /register` devuelve 400 con `"Username 'X' is already taken"` | El nombre de usuario ya existe en la BD | Elegir otro nombre de usuario |
| `POST /login` devuelve 401 | Credenciales inválidas o el usuario no existe | Verificar email y contraseña |
| Swagger muestra el botón Authorize pero el token no funciona | La autenticación JWT está comentada en Program.cs | El botón Authorize es solo UI; la validación real se activará cuando el equipo descomente `AddAuthentication` |
| `ArgumentException` con mensaje de Identity en la respuesta | El servicio lanza `ArgumentException` con los errores de Identity | El `ExceptionHandlingMiddleware` lo captura y devuelve 400. En producción el mensaje es genérico. |

---

## Cómo probar tus endpoints

1. Ejecuta la API: pulsa **▶️ IIS Express / MovieHubAPI** en la toolbar de Visual Studio, o escribe `dotnet run` *(Terminal normal)*
2. Abre Swagger: `https://localhost:7154/swagger`
3. Prueba cada endpoint con datos reales
4. Verifica que los códigos HTTP son correctos (200, 201, 404, 400)

---

### Después del merge

Cuando tu PR se fusione, borra la rama y crea una nueva desde `main` actualizado.
Ver [`CONTRIBUTING.md`](../CONTRIBUTING.md#despu%C3%A9s-del-merge-c%C3%B3mo-empezar-la-siguiente-iteraci%C3%B3n).
