using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
                return CreatedAtAction(nameof(GetMe), new { userId = result.UserId }, result);
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

        [Authorize]
        [HttpGet("me")]
        [EndpointSummary("Obtener perfil del usuario")]
        [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> GetMe()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _usuarioService.GetProfileAsync(userId);
            if (result is null) return NotFound();
            return Ok(result);
        }

        [Authorize]
        [HttpPut("me")]
        [EndpointSummary("Actualizar perfil del usuario")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMe(UpdateProfileDto dto)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _usuarioService.UpdateProfileAsync(userId, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("me")]
        [EndpointSummary("Eliminar cuenta del usuario")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteMe()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _usuarioService.DeleteProfileAsync(userId);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
