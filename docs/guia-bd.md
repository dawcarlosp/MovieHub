# Guía de Base de Datos — MovieHub

Rol asignado: **[@P4bM4rx](https://github.com/P4bM4rx)** — rama `feature/database`

---

## ⚠️ Regla de oro (que te ahorrará dolores de cabeza)

**Solo tú generas migraciones.** Nadie más ejecuta comandos de migración. Si alguien necesita cambiar una entidad, te avisa y tú generas la migración.

---

## Tecnologías

- .NET 10 + EF Core 10 + SQL Server
- Identity (`IdentityUser<long>`, `IdentityRole<long>`, `IdentityDbContext`)
- Migraciones desde la **Package Manager Console** de Visual Studio
- Connection string: `MovieHubConnection` en `appsettings.json`

---

## ¿Package Manager Console o Terminal normal?

Toda la guía usa dos consolas distintas. No las confundas:

| Consola | Para qué | Cómo se abre |
|---|---|---|
| **Package Manager Console** | Migraciones de EF Core | Tools → NuGet Package Manager → Package Manager Console |
| **Terminal normal** (VS, VS Code, PowerShell, CMD, Windows Terminal) | Git, `dotnet run`, `dotnet build`, etc. | View → Terminal (VS), Ctrl+` (VS Code), o cualquier terminal externa |

En los comandos de esta guía, cada uno lleva una etiqueta que indica dónde escribirlo.

---

## Flujo diario paso a paso

### 1. Antes de empezar, sincroniza *(Terminal normal)*

```bash
git checkout main
git pull origin main
git checkout feature/database
git merge main
```

### 2. Modifica la entidad

Edita el modelo en `MovieHubAPI/Models/` (ej: `Pelicula.cs`, `Genero.cs`).

**Visual Studio:** Solution Explorer → `Models/` → botón derecho → Add → Class

### 3. Genera la migración *(Package Manager Console)*

Asegúrate de que el proyecto seleccionado en la consola sea `MovieHubAPI`.

```
Add-Migration NombreDescriptivo
```

> **Nombre descriptivo** — ej: `AddDuracionToPelicula`, `CreateGeneroTable`. Así el historial se entiende de un vistazo.

### 4. **REVISA** el archivo generado (esto es obligatorio)

EF Core a veces interpreta mal los cambios. Abre el archivo en `Migrations/` desde Solution Explorer y verifica:

| Situación | Qué mirar |
|---|---|
| Renombrar columna | EF genera `DROP` + `ADD` (pérdida de datos). Cámbialo por `RenameColumn()`. |
| Eliminar columna | Asegúrate de que no haya datos que perder. |
| Tipo de dato | Comprueba que `nvarchar(max)` no se cuela donde debería ir `nvarchar(100)`. |

### 5. Aplica la migración en local *(Package Manager Console)*

```
Update-Database
```

Si falla, lee el error, corrige y repite desde el paso 3 (primero haz `Remove-Migration`).

### 6. Commit de todo (migración + cambios en entidad) *(Terminal normal)*

```bash
git add .
git commit -m "feat: add Duracion field to Pelicula"
git push origin feature/database
```

---

## Relaciones del modelo actual

El DbContext (`MovieHubDbContext.cs`) configura las relaciones explícitamente en `OnModelCreating`:

| Relación | Tipo | Implementación real |
|---|---|---|
| Película – Género | N:M | Tabla intermedia `PeliculaGeneroModel` con clave compuesta `(PeliculaId, GeneroId)` |
| Usuario – Valoración | 1:N | FK `UsuarioId` en `ValoracionModel`, índice único `(UsuarioId, PeliculaId)` para evitar duplicados |
| Usuario – Favoritos | N:M | Tabla intermedia `FavoritoModel` con clave compuesta `(UsuarioId, PeliculaId)` |

### Modelos actuales

```csharp
// PeliculaModel  →  PeliculaGeneroModel  ←  GeneroModel
// UsuarioModel (IdentityUser<long>)  →  ValoracionModel  ←  PeliculaModel
// UsuarioModel (IdentityUser<long>)  →  FavoritoModel  ←  PeliculaModel
```

> La herencia de `IdentityUser<long>` hace que `UsuarioModel` tenga el Id como `long`.
> El DbContext hereda de `IdentityDbContext<UsuarioModel, IdentityRole<long>, long>`.
>
> **Director:** No se normaliza en tabla separada por falta de tiempo y porque el proyecto no requiere gestión independiente de directores (CRUD, búsqueda, etc.). Se mantiene como `string` en `PeliculaModel` con `[MaxLength(500)]`.

---

## Primera migración desde cero

La carpeta `Migrations/` se eliminó del repositorio para partir de un estado limpio. La primera migración debe generarla el integrante de Base de Datos:

### Paso a paso

1. `git checkout main && git pull`
2. **Eliminar la BD local** (SSMS o Package Manager Console):
   ```
   DROP DATABASE MovieHubDB;
   ```
3. En la Package Manager Console (proyecto: `MovieHubAPI`):
   ```
   Add-Migration InitialCreate
   ```
4. Revisar el archivo generado en `Migrations/`:
   - Buscar `InsertData` → deben aparecer ~1105 filas (245 películas, 30 géneros, ~830 relaciones)
   - Buscar `DropColumn` o `DropTable` → **NO debe aparecer**. Si aparece, avisar.
5. Aplicar:
   ```
   Update-Database
   ```
6. Verificar:
   ```sql
   SELECT COUNT(*) FROM Peliculas;       → 245
   SELECT COUNT(*) FROM Generos;         → 30
   SELECT COUNT(*) FROM PeliculaGeneros; → ~830
   ```

### Posibles errores

| Error | Causa | Solución |
|-------|-------|----------|
| `Update-Database` falla porque la BD ya existe | No se eliminó antes | Ejecutar `DROP DATABASE MovieHubDB;` y repetir |
| `InsertData` con menos filas | Archivos incompletos | Avisar al equipo |
| Migración contiene `DropColumn` | Migración anterior interfiere | Borrar BD y regenerar |
| Cualquier otro error | — | **Avisar antes de intentar arreglarlo solo** |

---

## Errores frecuentes y cómo resolverlos

### Error: conflicto en el snapshot al hacer `git merge main`

**Contexto actual:** Desde que se eliminó `Migrations/` del repo, este error **ya no debería ocurrir**. Si en el futuro alguien modifica una entidad y genera una migración en otra rama, el conflicto podría reaparecer.

**Solución si ocurre:**

1. Resuelve el conflicto en `DbContextModelSnapshot.cs` manualmente (elige los cambios de ambas partes si son compatibles)
2. Verifica que el snapshot refleja el estado real del modelo:
   ```
   Add-Migration CheckMigration
   ```
   Si genera algo vacío, bórrala con `Remove-Migration`.
3. Si hay cambios pendientes reales, mantenlos y commitea.

### Error: "No executable found matching command dotnet-ef" *(Terminal normal)*

```bash
dotnet tool install --global dotnet-ef
```

(Solo si alguien quiere usar la CLI en vez de la PMC, cosa que no se recomienda en este equipo).

### Error: "An error occurred while accessing the database"

- ¿SQL Server está corriendo? (comprueba con `Services.msc` o Docker)
- ¿La connection string en `appsettings.json` apunta a `localhost`?

---

## Comandos útiles (resumen)

| Comando | Consola | Cuándo usarlo |
|---|---|---|
| `Add-Migration Nombre` | Package Manager Console | Nueva migración |
| `Remove-Migration` | Package Manager Console | Quitar la última migración (no aplicada) |
| `Update-Database` | Package Manager Console | Aplicar migraciones pendientes |
| `Update-Database NombreMigracion` | Package Manager Console | Revertir hasta una migración concreta |
| `Get-Migration` | Package Manager Console | Ver historial de migraciones |
| `Script-Migration` | Package Manager Console | Generar script SQL (para producción) |

---

## Seed data

Implementado mediante `HasData()` en archivos de configuración separados dentro de `Data/Configurations/`:

- `GeneroConfig.cs` — 30 géneros (IDs 1–30)
- `PeliculaConfig.cs` — 245 películas con título, descripción, director, año, duración, póster y puntuación (2.5–5.0)
- `PeliculaGeneroConfig.cs` — ~830 relaciones película-género

Las configuraciones se registran automáticamente vía `ApplyConfigurationsFromAssembly` en el DbContext:

```csharp
// MovieHubDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(MovieHubDbContext).Assembly);
}
```

Cada archivo de configuración implementa `IEntityTypeConfiguration<T>` y usa `HasData()`:

```csharp
// Ejemplo: PeliculaConfig.cs
public class PeliculaConfig : IEntityTypeConfiguration<PeliculaModel>
{
    public void Configure(EntityTypeBuilder<PeliculaModel> builder)
    {
        builder.HasData(
            new PeliculaModel { Id = 1, Titulo = "Inception", ... },
            new PeliculaModel { Id = 2, Titulo = "The Matrix", ... }
        );
    }
}
```

> `HasData()` requiere que especifiques la clave primaria explícitamente.
> No hace falta generar una migración aparte — inclúyelo en la migración donde crees la tabla.

---

### Después del merge

Cuando tu PR se fusione, borra la rama y crea una nueva desde `main` actualizado.
Ver [`CONTRIBUTING.md`](../CONTRIBUTING.md#despu%C3%A9s-del-merge-c%C3%B3mo-empezar-la-siguiente-iteraci%C3%B3n).
