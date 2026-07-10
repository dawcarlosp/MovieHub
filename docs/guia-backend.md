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

> ⚠️ **Importante:** Actualmente `MappingConfig.Configure()` **no se llama** en `Program.cs`.
> Si añades un nuevo mapeo personalizado, asegúrate de invocarlo al arrancar la app.

---

## Paso 3: Interface (`Interfaces/`)

Define el contrato del servicio. Todas las interfaces están bajo el namespace `MovieHubAPI.Interfaces`:

```csharp
// IPeliculaService.cs
public interface IPeliculaService
{
    Task<List<PeliculaDto>> GetAllAsync();
    Task<PeliculaDto?> GetByIdAsync(int id);
    Task<PeliculaDto> CreateAsync(CreatePeliculaDto dto);
    Task<PeliculaDto?> UpdateAsync(int id, UpdatePeliculaDto dto);
    Task<bool> DeleteAsync(int id);
}
```

Interfaces existentes: `IPeliculaService`, `IGeneroService`, `IUsuarioService` (esta última pendiente de implementar).

---

## Paso 4: Servicio (`Services/`)

Toda la lógica de negocio va aquí. Los controladores **no** tienen lógica. Los servicios existentes usan `DbContext` (clase real del proyecto) y Mapster:

```csharp
public class PeliculaService : IPeliculaService
{
    private readonly DbContext _context;

    public PeliculaService(DbContext context) => _context = context;

    public async Task<List<PeliculaDto>> GetAllAsync()
    {
        var peliculas = await _context.Peliculas
            .Include(p => p.PeliculaGeneros)
            .ThenInclude(pg => pg.Genero)
            .ToListAsync();

        return peliculas.Adapt<List<PeliculaDto>>();
    }

    // ... resto de métodos
}
```

Servicios existentes: `PeliculaService`, `GeneroService`. `UsuarioService` está pendiente de implementar.

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
    public async Task<ActionResult<List<PeliculaDto>>> GetAll()
    {
        var peliculas = await _peliculaService.GetAllAsync();
        return Ok(peliculas);
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
// IUsuarioService pendiente de registrar
```

> ⚠️ **Aviso:** `MappingConfig.Configure()` actualmente **no se invoca** en `Program.cs`.
> Si trabajas con Mapster y necesitas mapeos personalizados, añádelo antes de `var app = builder.Build();`.

### Autenticación (futura)

El proyecto incluye los paquetes JWT e Identity, pero la autenticación está **comentada** en `Program.cs`:

```csharp
// builder.Services.AddAuthentication(...)
// builder.Services.AddAuthorization(...)
// app.UseAuthentication();
```

Cuando se active, descomentar esos bloques y configurar `Jwt:Key`, `Jwt:Issuer` y `Jwt:Audience` en `appsettings.json`.

---

## Validación con FluentValidation

El paquete `FluentValidation.DependencyInjectionExtensions` está instalado pero **no hay validadores creados aún**.

Cuando los crees, sigue este patrón:

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

Y regístralos en `Program.cs`:

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreatePeliculaValidator>();
```

---

## Errores frecuentes

| Error | Causa | Solución |
|---|---|---|---|
| `No service for type X has been registered` | Olvidaste registrar el servicio en Program.cs | Añadir `builder.Services.AddScoped<IX, X>()` |
| `Object reference not set to an instance of an object` | No hiciste `Include()` + `ThenInclude()` de las relaciones | Añadir `.Include(p => p.PeliculaGeneros).ThenInclude(pg => pg.Genero)` |
| `A possible object cycle was detected` | Relación circular en JSON | Usar DTOs planos (nunca expongas entidades EF directamente) |
| Swagger no carga | No está configurado | Verificar `app.UseSwagger()` y `UseSwaggerUI()` en Program.cs |
| 405 Method Not Allowed | Usaste `[HttpPost]` pero envías GET | Revisar el verbo HTTP en el endpoint |
| MappingConfig no tiene efecto | `Configure()` no se llama desde `Program.cs` | Añadir `MappingConfig.Configure()` antes de `var app = builder.Build();` |
| Error de compilación con `DbContext` | Conflicto con `System.Data.Common.DbContext` | Usar el namespace global o alias; nuestro `DbContext` no tiene namespace |

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
