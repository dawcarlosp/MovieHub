## Descripción

Se corrige la indicación visual del enlace activo en la barra de navegación. Al cambiar de vista (Inicio, Películas, Mi lista), el botón correspondiente ahora se marca como activo tanto en la barra de escritorio como en el menú hamburguesa.

## Cambios

### 1. `navbar.component.html`
- **Escritorio**: se agregó `[class.navbar__link--active]` al enlace "Mi lista" (faltaba).
- **Menú hamburguesa**: se agregó `[class.hamburger__item--active]` a los botones de Inicio, Películas y Mi lista para que reflejen la vista activa.

### 2. `navbar.component.scss`
- Se agregó la clase `.hamburger__item--active` con estilo visual para los items del menú hamburguesa (fondo semitransparente, texto blanco, negrita).

## Commits

| Commit | Archivo | Descripción |
|--------|---------|-------------|
| `3a5d198` | `navbar.component.html` | Agrega estado activo a "Mi lista" en escritorio y hamburguesa |
| `d0de436` | `navbar.component.html` | Agrega estado activo a Inicio y Películas en hamburguesa |
| `f42123b` | `navbar.component.scss` | Agrega estilo `.hamburger__item--active` |
