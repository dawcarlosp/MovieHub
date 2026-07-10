namespace MovieHubAPI.DTOs.Usuario
{
    public record UserProfileDto(
        long Id,
        string UserName,
        string Email,
        DateTime FechaRegistro
    );
}
