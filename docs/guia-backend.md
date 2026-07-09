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

```csharp
public class Pelicula
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Duracion { get; set; }  // minutos
    public int AnioEstreno { get; set; }
    public string Director { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public double PuntuacionMedia { get; set; }

    // Relaciones
    public ICollection<Genero> Generos { get; set; } = new List<Genero>();
}
```

> ⚠️ El `DbSet<Pelicula>` lo añade el integrante de Base de Datos en `MovieHubContext`.
> Tú solo creas la clase y le dices a Base de Datos que la revise.

---

## Paso 2: DTO (`DTOs/`)

Nunca expongas los modelos EF Core directamente. Usa **records** con Mapster:

```csharp
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

### Mapeo con Mapster (ya instalado)

Para convertir entre entidad y DTO usa Mapster en el servicio:

```csharp
using Mapster;

// En el servicio:
var peliculaDto = pelicula.Adapt<PeliculaDto>();
var pelicula = dto.Adapt<Pelicula>();
```

Para configuraciones personalizadas, crea un archivo `MappingConfig.cs`:

```csharp
public static class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Pelicula, PeliculaDto>.NewConfig()
            .Map(dest => dest.Generos, src => src.Generos.Select(g => g.Nombre).ToList());
    }
}
```

Y llámalo desde `Program.cs` al arrancar.

---

## Paso 3: Interface (`Interfaces/`)

Define el contrato del servicio:

```csharp
public interface IPeliculaService
{
    Task<List<PeliculaDto>> GetAllAsync();
    Task<PeliculaDto?> GetByIdAsync(int id);
    Task<PeliculaDto> CreateAsync(CreatePeliculaDto dto);
    Task<PeliculaDto?> UpdateAsync(int id, UpdatePeliculaDto dto);
    Task<bool> DeleteAsync(int id);
}
```

---

## Paso 4: Servicio (`Services/`)

Toda la lógica de negocio va aquí. Los controladores **no** tienen lógica:

```csharp
public class PeliculaService : IPeliculaService
{
    private readonly MovieHubContext _context;

    public PeliculaService(MovieHubContext context)
    {
        _context = context;
    }

    public async Task<List<PeliculaDto>> GetAllAsync()
    {
        var peliculas = await _context.Peliculas
            .Include(p => p.Generos)
            .ToListAsync();

        return peliculas.Adapt<List<PeliculaDto>>();
    }

    // ... resto de métodos
}
```

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

Descomenta y completa el registro del DbContext:

```csharp
builder.Services.AddDbContext<MovieHubContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MovieHubConnection")));
```

Registra tus servicios:

```csharp
builder.Services.AddScoped<IPeliculaService, PeliculaService>();
builder.Services.AddScoped<IGeneroService, GeneroService>();
// etc.
```

Configura Mapster si tienes MappingConfig:

```csharp
MappingConfig.Configure();
```

---

## Validación con FluentValidation

Crea validadores para tus DTOs de entrada:

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

Regístralos en `Program.cs`:

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreatePeliculaValidator>();
```

---

## Errores frecuentes

| Error | Causa | Solución |
|---|---|---|
| `No service for type X has been registered` | Olvidaste registrar el servicio en Program.cs | Añadir `builder.Services.AddScoped<IX, X>()` |
| `Object reference not set to an instance of an object` | No hiciste `Include()` de las relaciones | Añadir `.Include(p => p.Generos)` |
| `A possible object cycle was detected` | Relación circular en JSON | Configurar `ReferenceHandler.IgnoreCycles` en Program.cs o usar DTOs planos |
| Swagger no carga | No está configurado | Verificar `app.UseSwagger()` y `UseSwaggerUI()` en Program.cs |
| 405 Method Not Allowed | Usaste `[HttpPost]` pero envías GET | Revisar el verbo HTTP en el endpoint |

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
