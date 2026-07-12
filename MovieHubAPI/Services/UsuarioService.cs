using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MovieHubAPI.DTOs.Usuario;
using MovieHubAPI.Interfaces;

namespace MovieHubAPI.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly UserManager<UsuarioModel> _userManager;
        private readonly IConfiguration _configuration;

        public UsuarioService(UserManager<UsuarioModel> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            var usuario = new UsuarioModel
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FechaRegistro = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(usuario, dto.Password);
            if (!result.Succeeded)
                throw new ArgumentException(
                    string.Join("; ", result.Errors.Select(e => e.Description)));

            return await GenerateAuthResponseAsync(usuario);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var usuario = await _userManager.FindByEmailAsync(dto.Email);
            if (usuario is null) return null;

            var validPassword = await _userManager.CheckPasswordAsync(usuario, dto.Password);
            if (!validPassword) return null;

            return await GenerateAuthResponseAsync(usuario);
        }

        public async Task<UserProfileDto?> GetProfileAsync(long userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId.ToString());
            if (usuario is null) return null;

            return new UserProfileDto(
                Id: usuario.Id,
                UserName: usuario.UserName!,
                Email: usuario.Email!,
                FechaRegistro: usuario.FechaRegistro
            );
        }

        public async Task<bool> UpdateProfileAsync(long userId, UpdateProfileDto dto)
        {
            var usuario = await _userManager.FindByIdAsync(userId.ToString());
            if (usuario is null) return false;

            usuario.UserName = dto.UserName;
            usuario.Email = dto.Email;

            var result = await _userManager.UpdateAsync(usuario);
            return result.Succeeded;
        }

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(UsuarioModel usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = jwtSettings["Key"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;

            var tokenHandler = new JsonWebTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.UserName!),
                    new Claim(ClaimTypes.Email, usuario.Email!)
                ]),
                Expires = DateTime.UtcNow.AddMinutes(4),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var expiration = tokenDescriptor.Expires!.Value;

            return new AuthResponseDto(
                Token: token,
                Expiration: expiration,
                UserId: usuario.Id,
                UserName: usuario.UserName!,
                Email: usuario.Email!
            );
        }
    }
}
