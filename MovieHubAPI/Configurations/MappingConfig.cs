using Mapster;
using MovieHubAPI.DTOs;

namespace MovieHubAPI.Configurations
{
    public static class MappingConfig
    {
        public static void Configure()
        {
            TypeAdapterConfig<PeliculaModel, PeliculaDto>.NewConfig()
                .Map(dest => dest.AnioEstreno, src => src.Anio)
                .Map(dest => dest.Imagen, src => src.PosterUrl)
                .Map(dest => dest.Generos, src => src.PeliculaGeneros.Select(pg => pg.Genero.Nombre).ToList());
        }
    }
}
