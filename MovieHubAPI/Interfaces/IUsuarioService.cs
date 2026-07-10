using MovieHubAPI.DTOs.Usuario;

namespace MovieHubAPI.Interfaces
{
    public interface IUsuarioService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<UserProfileDto?> GetProfileAsync(long userId);
        Task<bool> UpdateProfileAsync(long userId, UpdateProfileDto dto);
    }
}
