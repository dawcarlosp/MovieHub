namespace MovieHubAPI.DTOs;

public record EstadisticasDto(
    int TotalPeliculas,
    double PuntuacionMediaGlobal,
    int TotalGeneros,
    int TotalValoraciones
);
