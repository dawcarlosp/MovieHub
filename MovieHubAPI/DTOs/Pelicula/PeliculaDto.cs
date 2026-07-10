namespace MovieHubAPI.DTOs.Pelicula;

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
