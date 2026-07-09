# Guía de Base de Datos — MovieHub

Rol asignado: **[@P4bM4rx](https://github.com/P4bM4rx)** — rama `feature/database`

---

## ⚠️ Regla de oro (que te ahorrará dolores de cabeza)

**Solo tú generas migraciones.** Nadie más ejecuta comandos de migración. Si alguien necesita cambiar una entidad, te avisa y tú generas la migración.

---

## Tecnologías

- .NET 10 + EF Core 10 + SQL Server
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

## Cómo configurar las relaciones del modelo actual

Según el README actualizado:

| Relación | Tipo | Cómo se hace en EF Core |
|---|---|---|
| Película – Género | N:M | `modelBuilder.Entity<Pelicula>().HasMany(...).WithMany(...)` |
| Usuario – Valoración | 1:N | FK `UsuarioId` en `Valoracion` |
| Usuario – Favoritos | N:M | Tabla intermedia `Favoritos` con `UserId` + `PeliculaId` |

---

## Errores frecuentes y cómo resolverlos

### Error: conflicto en el snapshot al hacer `git merge main`

**Por qué pasa:** Otro compañero tocó una entidad y su rama se fusionó en `main`. El archivo `MyContextModelSnapshot.cs` entra en conflicto.

**Solución:**

1. Resuelve el conflicto en `MyContextModelSnapshot.cs` manualmente (elige los cambios de ambas partes si son compatibles)
2. Una vez resuelto y commiteado el merge, verifica que el snapshot refleja el estado real del modelo *(Package Manager Console)*:

   ```
   HasPendingModelChanges
   ```

3. Si dice que hay cambios pendientes, regenera la migración *(Package Manager Console)*:

   ```
   Remove-Migration
   Add-Migration MiMigracion
   ```

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
| `HasPendingModelChanges` | Package Manager Console | Verificar si faltan migraciones |

---

## Seed data

Cuando añadas datos iniciales, usa `HasData()` dentro de `OnModelCreating` en el `MovieHubContext`:

```csharp
modelBuilder.Entity<Genero>().HasData(
    new Genero { Id = 1, Nombre = "Acción" },
    new Genero { Id = 2, Nombre = "Comedia" },
    new Genero { Id = 3, Nombre = "Drama" }
);
```

> `HasData()` requiere que especifiques la clave primaria explícitamente.
> No hace falta generar una migración aparte — inclúyelo en la migración donde crees la tabla.

---

### Después del merge

Cuando tu PR se fusione, borra la rama y crea una nueva desde `main` actualizado.
Ver [`CONTRIBUTING.md`](../CONTRIBUTING.md#despu%C3%A9s-del-merge-c%C3%B3mo-empezar-la-siguiente-iteraci%C3%B3n).
