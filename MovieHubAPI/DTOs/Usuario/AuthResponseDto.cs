namespace MovieHubAPI.DTOs.Usuario
{
    public record AuthResponseDto(
     string Token,
     DateTime Expiration,
     long UserId,
     string UserName,
     string Email
 );
}
