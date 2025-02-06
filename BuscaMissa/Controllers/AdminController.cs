using BuscaMissa.DTOs;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController(
        ILogger<AdminController> logger,
        UsuarioService usuarioService
        ) : ControllerBase
    {
        private readonly ILogger<AdminController> _logger = logger;
        private readonly UsuarioService _usuarioService = usuarioService;

        #region Usuario
        [HttpGet]
        [Route("{codigo}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BuscarPorCodigoAsync(int codigo)
        {
            try
            {

                var usuario = await _usuarioService.BuscarPorCodigo(codigo);
                if (usuario == null)
                    return NotFound();
                UsuarioResponse usuarioResponse = (UsuarioResponse)usuario;
                return Ok(new ApiResponse<dynamic>(new
                {
                    usuario = usuarioResponse
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("buscar-por-filtro")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BuscarPorCodigoAsync([FromQuery] UsuarioFiltroRequest filtro)
        {
            try
            {

                var usuarios = await _usuarioService.BuscarPorFiltroAsync(filtro);
                if (usuarios == null)
                    return NotFound();
                return Ok(new ApiResponse<dynamic>(new
                {
                    usuarios
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        
        #endregion
    }
}