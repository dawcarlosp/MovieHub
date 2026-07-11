using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class MovieHubDbContext : IdentityDbContext<UsuarioModel, IdentityRole<long>, long>
{
    public MovieHubDbContext(DbContextOptions<MovieHubDbContext> options)
        : base(options) { }

    public DbSet<PeliculaModel> Peliculas { get; set; }
    public DbSet<GeneroModel> Generos { get; set; }
    public DbSet<PeliculaGeneroModel> PeliculaGeneros { get; set; }
    public DbSet<ValoracionModel> Valoraciones { get; set; }
    public DbSet<FavoritoModel> Favoritos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Clave compuesta: PeliculaGenero
        builder.Entity<PeliculaGeneroModel>()
            .HasKey(pg => new { pg.PeliculaId, pg.GeneroId });

        builder.Entity<PeliculaGeneroModel>()
            .HasOne(pg => pg.Pelicula)
            .WithMany(p => p.PeliculaGeneros)
            .HasForeignKey(pg => pg.PeliculaId);

        builder.Entity<PeliculaGeneroModel>()
            .HasOne(pg => pg.Genero)
            .WithMany(g => g.PeliculaGeneros)
            .HasForeignKey(pg => pg.GeneroId);

        // Clave compuesta: Favorito
        builder.Entity<FavoritoModel>()
            .HasKey(f => new { f.UsuarioId, f.PeliculaId });

        builder.Entity<FavoritoModel>()
            .HasOne(f => f.Usuario)
            .WithMany(u => u.Favoritos)
            .HasForeignKey(f => f.UsuarioId);

        builder.Entity<FavoritoModel>()
            .HasOne(f => f.Pelicula)
            .WithMany(p => p.Favoritos)
            .HasForeignKey(f => f.PeliculaId);

        // Valoracion: relaciones explícitas
        builder.Entity<ValoracionModel>()
            .HasOne(v => v.Usuario)
            .WithMany(u => u.Valoraciones)
            .HasForeignKey(v => v.UsuarioId);

        builder.Entity<ValoracionModel>()
            .HasOne(v => v.Pelicula)
            .WithMany(p => p.Valoraciones)
            .HasForeignKey(v => v.PeliculaId);

        // Restricción opcional: un usuario solo puede valorar una vez la misma película
        builder.Entity<ValoracionModel>()
            .HasIndex(v => new { v.UsuarioId, v.PeliculaId })
            .IsUnique();

        //Constructor para que el Id de UsuarioModel sea autogenerado
        builder.Entity<UsuarioModel>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();
    }
}