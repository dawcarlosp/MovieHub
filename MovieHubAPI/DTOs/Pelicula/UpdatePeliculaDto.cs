namespace MovieHubAPI.DTOs;

public record UpdatePeliculaDto(
    string Titulo,
    string Descripcion,
    int Duracion,
    int AnioEstreno,
    string Director,
    string? Imagen,
    List<int> GeneroIds
);
