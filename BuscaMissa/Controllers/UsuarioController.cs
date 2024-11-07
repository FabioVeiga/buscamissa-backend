using BuscaMissa.DTOs;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController(ILogger<UsuarioController> logger, UsuarioService usuarioService, IgrejaService igrejaService) : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger = logger;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly IgrejaService _igrejaService = igrejaService;

        [HttpPost]
        public async Task<IActionResult> Inserir([FromBody] CriacaoUsuarioRequest request)
        {
            try
            {
                if (!ModelState.IsValid) BadRequest();

                var usuarioCriado = await _usuarioService.InserirAsync(request);
                return Ok(new ApiResponse<dynamic>(new
                {
                    usuario = usuarioCriado
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost]
        [Route("inserir-igreja")]
        public async Task<IActionResult> InserirUsuarioPorIgreja([FromBody] IgrejaCriacaoUsuarioRequest request)
        {
            try
            {
                if (!ModelState.IsValid) BadRequest();
                var temIgreja = await _igrejaService.BuscarPorIdAsync(request.IgrejaId);
                if (temIgreja == null) return BadRequest(new ApiResponse<dynamic>(new { mensagemInterno = "Igreja não encontrada!" }));
                var usuarioCriado = await _usuarioService.InserirAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost]
        [Route("autenticar")]
        public async Task<IActionResult> Autenticar([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid) BadRequest();
                var usuario = await _usuarioService.BuscarPorEmailAsync(request.Email);
                if (usuario == null) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Usuário não existe!" }));
                var autenticado = await _usuarioService.AutenticarAsync(request, usuario);
                if (!autenticado) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "E-mail ou Senha invalido!" }));
                var usuarioResponse = _usuarioService.GerarTokenAsync(usuario);
                return Ok(new ApiResponse<dynamic>(new { usuario = usuarioResponse }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }


        // [HttpGet]
        // [Route("buscar-por-codigo/{codigo}")]
        // public async Task<IActionResult> BuscarPorCodigoAsync(int codigo)
        // {
        //     try
        //     {

        //         var usuario = await _usuarioService.BuscarPorCodigo(codigo);
        //         if (usuario == null)
        //             return NotFound();
        //         UsuarioResponse usuarioResponse = (UsuarioResponse)usuario;
        //         return Ok(new ApiResponse<dynamic>(new
        //         {
        //             usuario = usuarioResponse
        //         }));
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError("{Ex}", ex);
        //         var response = new ApiResponse<dynamic>(ex.Message);
        //         return StatusCode(StatusCodes.Status500InternalServerError, response);
        //     }
        // }
    }

}
