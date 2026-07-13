# Guía de Funcionalidades — MovieHub

Rol asignado: **[@Anua472](https://github.com/Anua472)** — rama `feature/features`

---

> 💡 **¿Package Manager Console o Terminal normal?** Esta guía usa la **Terminal normal** (View → Terminal en VS, Ctrl+` en VS Code, PowerShell, CMD o Windows Terminal) para todo. Los comandos de migraciones (`Add-Migration`, etc.) los ejecuta el integrante de Base de Datos desde la **Package Manager Console**.

## ¿Qué significa "Funcionalidades"?

Tus features tocan **Angular y ASP.NET a la vez**: valoraciones, favoritos, filtros, búsquedas, rankings y estadísticas. Eres el puente entre backend y frontend.

> 🔐 **Autenticación disponible** — El backend tiene JWT activo. Endpoints públicos: `POST /api/Usuarios/register` y `POST /api/Usuarios/login`. El resto requiere token Bearer. El `userId` se obtiene del response del login (`AuthResponseDto.UserId`). Consulta a @dawcarlosp para más detalles.

---

## Regla de oro para no romper nada

**No empieces el frontend hasta que el endpoint del backend esté probado en Swagger.**

Si haces las dos capas a la vez y algo falla, no sabrás si el error está en backend, frontend o la comunicación entre ambos.

---

## Flujo correcto para cada funcionalidad

```
1. Diseñar: ¿qué datos necesito?
       ↓
2. Backend: endpoint (con DTO) → probar en Swagger
       ↓
3. Frontend: servicio Angular → componente → ruta
       ↓
4. Probar el flujo completo
```

---

## Funcionalidades concretas del proyecto

### Valoraciones (1-5 estrellas) — **Backend implementado** 🟢

**Modelo existente:** `ValoracionModel` con campos `UsuarioId`, `PeliculaId`, `Puntuacion` (1-5) y `Fecha`.

**Backend (implementado):**

```csharp
// DTOs
public record CreateValoracionDto(int PeliculaId, int Puntuacion);
public record ValoracionDto(int Id, string UsuarioEmail, int Puntuacion, DateTime Fecha);

// Endpoints en ValoracionesController (requieren auth)
POST   /api/valoraciones           // crear valoración
PUT    /api/valoraciones/{id}      // modificar valoración
DELETE /api/valoraciones/{id}      // eliminar valoración
GET    /api/valoraciones/pelicula/{peliculaId}  // obtener valoraciones de una película
```

> La `PuntuacionMedia` de la película se recalcula automáticamente tras cada valoración.

**Frontend (pendiente):**

```
Componente estrella-valoracion (reutilizable)
  → Servicio consume endpoints de valoraciones
  → Al hacer clic en estrella → POST /api/valoraciones
  → Se refresca puntuacionMedia
```

### Favoritos — **Completado** ✅

**Modelo existente:** `FavoritoModel` con clave compuesta `(UsuarioId, PeliculaId)`.

**Backend:**
```csharp
GET    /api/favoritos                    // listar favoritos del usuario (requiere auth)
POST   /api/favoritos/{peliculaId}       // añadir a favoritos (requiere auth)
DELETE /api/favoritos/{peliculaId}       // quitar de favoritos (requiere auth)
```

**Frontend:**
- `FavoritoButtonComponent` — botón corazón en cards y detalle, toggle optimista con POST/DELETE
- `FavoritoStateService` — estado global con `Set<number>` de IDs, carga al iniciar sesión
- `FavoritosPageComponent` — página "Mi lista" con grid de favoritas, navegación desde navbar

### Búsqueda por título + filtros — **Backend implementado** 🟢

**Backend:**

```csharp
GET /api/peliculas?titulo=batman&generoId=3&orden=puntuacion&page=1&pageSize=10
```

Un solo endpoint con parámetros opcionales. El servicio aplica los filtros en memoria con LINQ.

> Este endpoint incluye paginación integrada. Consulta `docs/guia-backend.md` para más detalles sobre el formato de respuesta paginado.

**Frontend:**

```
Componente buscador (shared/)
  → Input de texto + selector de género (Angular Material)
  → Al escribir o seleccionar → llama al endpoint con query params
  → Muestra resultados en el listado
```

### Rankings y estadísticas — **Backend implementado** 🟢

**Backend (implementado):**

```csharp
GET /api/peliculas/mejor-valoradas   → top 10 por puntuacionMedia
GET /api/peliculas/mas-recientes     → ordenadas por anioEstreno DESC
GET /api/peliculas/estadisticas      → total películas, media global, total géneros, total valoraciones
```

**Frontend (pendiente):**

```
Secciones en página principal:
  → "Mejor valoradas" (grid de cards)
  → "Más recientes" (grid de cards)
  → Panel de estadísticas
```

---

## Cómo coordinar con el resto del equipo

| Si necesitas... | Habla con |
|---|---|
| Una entidad nueva o cambiar una existente | Base de datos (@P4bM4rx) |
| Un endpoint que ya debería existir | Backend (@dawcarlosp) |
| Un componente nuevo o cambiar el diseño | Frontend (@claauudiiaacr) |

---

## Errores frecuentes

| Error | Causa | Solución |
|---|---|---|---|
| El frontend manda datos que el backend no entiende | DTO desincronizado entre Angular y ASP.NET | Copia exacta de campos y tipos |
| 401 Unauthorized | El endpoint requiere auth y no envías token | La autenticación JWT ya está activa — obtén un token en `POST /api/Usuarios/login` y pégalo en Swagger → Authorize |
| CORS bloquea la petición | El backend no tiene configurado el origen | Ya está configurado para `http://localhost:4200`. Si cambias puerto, actualiza `Program.cs`. |
| La puntuación media no se actualiza | OLVIDASTE recalcularla en el servicio | Después de cada valoración, recalcula y guarda |

---

## Para probar tu funcionalidad

1. Arranca el backend: pulsa **▶️ IIS Express / MovieHubAPI** en Visual Studio, o escribe `dotnet run` *(Terminal normal)*
2. Abre Swagger: `https://localhost:7154/swagger`
3. Prueba el endpoint con datos reales
4. Arranca el frontend: `ng serve` *(Terminal normal)*
5. Prueba el flujo completo en el navegador

---

### Después del merge

Cuando tu PR se fusione, borra la rama y crea una nueva desde `main` actualizado.
Ver [`CONTRIBUTING.md`](../CONTRIBUTING.md#despu%C3%A9s-del-merge-c%C3%B3mo-empezar-la-siguiente-iteraci%C3%B3n).
