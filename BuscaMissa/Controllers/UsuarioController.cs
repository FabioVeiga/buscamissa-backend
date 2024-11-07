using BuscaMissa.DTOs;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController(ILogger<UsuarioController> logger, UsuarioService usuarioService, IgrejaService igrejaService, ControleService controleService,
    CodigoValidacaoService codigoValidacaoService) : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger = logger;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly IgrejaService _igrejaService = igrejaService;
        private readonly ControleService _controleService = controleService;
        private readonly CodigoValidacaoService _codigoValidacaoService = codigoValidacaoService;


        [HttpPost]
        [AllowAnonymous]
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
        [Route("inserir-controle")]
        [Authorize(Roles = "Admin,App")]
        public async Task<IActionResult> InserirUsuarioPorIgreja([FromBody] IgrejaCriacaoUsuarioRequest request)
        {
            try
            {
                if (!ModelState.IsValid) BadRequest();
                var controle = await _controleService.BuscarPorIdAsync(request.ControleId);
                if (controle == null) return BadRequest(new ApiResponse<dynamic>(new { mensagemInterno = "Controle não encontrada!" }));
                var usuarioCriado = await _usuarioService.InserirAsync(request);
                var codigoValidador = await _codigoValidacaoService.InserirAsync(controle);
                controle.Status = Enums.StatusEnum.Igreja_Criacao_Aguardando_Codigo_Validador;
                await _controleService.EditarAsync(controle);
                //enviar email
                return Ok(new ApiResponse<dynamic>(new
                {
                    mensagemTela = "Usuário criado com sucesso e enviado código para o email!",
                    #if DEBUG
                    codigoValidador = codigoValidador.CodigoToken
                    #endif
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
        [Route("autenticar")]
        [AllowAnonymous]
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
    }

}
