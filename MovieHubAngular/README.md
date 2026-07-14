# MovieHub — Frontend Angular

Frontend de la plataforma MovieHub, desarrollado con Angular 22 standalone y Angular Material M3.

## Estado actual

Proyecto completo con las siguientes funcionalidades:

- **Catálogo:** Página principal con héroe (película mejor valorada) + filas agrupadas por género
- **Navegación:** Navbar con menú de géneros desktop y menú hamburguesa responsive
- **Autenticación:** Login/registro con JWT, guards, interceptor de auth
- **Detalle de película:** Ficha individual con valoración 1-5 estrellas, favoritos, tráiler
- **Favoritos:** Botón corazón en cards y detalle, página "Mi lista" con grid
- **Lazy loading:** Cada página se carga bajo demanda mediante el router de Angular
- **Señales (Signals):** Estado reactivo sin RxJS Subjects
- **Skeleton loading:** Placeholder animado mientras carga el catálogo
- **Responsive:** Adaptado a móvil con grid 2 columnas y hero reducido

## Comandos

```bash
ng serve          # Servidor de desarrollo → http://localhost:4200
ng build          # Build de producción
ng test           # Tests unitarios (Vitest)
```

## Estructura

```
src/app/
├── core/                  # Guards, interceptors, layout, servicios singleton
├── features/              # Carpetas por funcionalidad
│   ├── auth/              #   pages/ (login) + components/ (register dialog)
│   ├── home/              #   pages/ (home) + components/ (hero, cards, rows)
│   ├── genero/            #   pages/ (genero) + components/ (banner)
│   ├── peliculas/         #   pages/ (detalle, favoritos) + components/ (trailer dialog)
│   └── ui/                #   skeleton loading
├── models/                # Interfaces TypeScript
└── shared/                # Componentes reutilizables, pipes, utilidades
```

Ver [`docs/guia-frontend.md`](../docs/guia-frontend.md) para la guía completa.
