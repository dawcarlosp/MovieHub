namespace MovieHubAPI.DTOs.Usuario
{
    public record RegisterDto(
       string UserName,
       string Email,
       string Password
   );
}
