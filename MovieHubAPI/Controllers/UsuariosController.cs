using Microsoft.AspNetCore.Mvc;
using MovieHubAPI.DTOs.Usuario;
using MovieHubAPI.Interfaces;

namespace MovieHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("register")]
        [EndpointSummary("Registrar un nuevo usuario")]
        [ProducesResponseType<AuthResponseDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            try
            {
                var result = await _usuarioService.RegisterAsync(dto);
                return CreatedAtAction(nameof(GetProfile), new { userId = result.UserId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        [EndpointSummary("Iniciar sesión")]
        [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var result = await _usuarioService.LoginAsync(dto);
            if (result is null)
                return Unauthorized(new { message = "Credenciales inválidas." });

            return Ok(result);
        }

        [HttpGet("profile/{userId}")]
        [EndpointSummary("Obtener perfil de usuario")]
        [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetProfile(long userId)
        {
            var result = await _usuarioService.GetProfileAsync(userId);
            if (result is null) return NotFound();
            return Ok(result);
        }

        [HttpPut("profile/{userId}")]
        [EndpointSummary("Actualizar perfil de usuario")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(long userId, UpdateProfileDto dto)
        {
            var result = await _usuarioService.UpdateProfileAsync(userId, dto);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
