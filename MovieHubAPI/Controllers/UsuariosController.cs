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
            var result = await _usuarioService.RegisterAsync(dto);
            if (result is null)
                return BadRequest(new { message = "El registro falló. El usuario o email puede ya existir." });

            return CreatedAtAction(nameof(Register), result);
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
    }
}
