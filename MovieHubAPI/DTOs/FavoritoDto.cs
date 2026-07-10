namespace MovieHubAPI.DTOs;

public record FavoritoDto(
    int PeliculaId,
    string Titulo,
    string? Imagen,
    double PuntuacionMedia
);
