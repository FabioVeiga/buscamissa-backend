using BuscaMissa.Constants;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Helpers;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController(ILogger<UsuarioController> logger, UsuarioService usuarioService, ControleService controleService,
    CodigoValidacaoService codigoValidacaoService, EmailService emailService) 
    : ControllerBase
    {
        private readonly ILogger<UsuarioController> _logger = logger;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly ControleService _controleService = controleService;
        private readonly CodigoValidacaoService _codigoValidacaoService = codigoValidacaoService;
        private readonly EmailService _emailService = emailService;

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
                if (usuario.Bloqueado) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Usuário bloqueado!" }));
                var autenticado = _usuarioService.Autenticar(request, usuario);
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
    
        [HttpPost]
        [Route("gerar-codigo-validador")]
        [Authorize(Roles = "Admin,App")]
        public async Task<IActionResult> GerarCodigoValidador([FromBody] UsuarioGerarCodigoRequest request)
        {
            try
            {
                if (!ModelState.IsValid) BadRequest();
                var controle = await _controleService.BuscarPorIdAsync(request.ControleId);
                if (controle == null) return BadRequest(new ApiResponse<dynamic>(new { mensagemInterno = "Controle não encontrada!" }));
                if(controle.Status == Enums.StatusEnum.Finalizado) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela= "Igreja já ativada!" }));
                var usuario = await _usuarioService.BuscarPorEmailAsync(request.Email);
                usuario ??= await _usuarioService.InserirAsync(request);
                var codigoValidador = await _codigoValidacaoService.BuscarPorControleIdAsync(request.ControleId);
                if(codigoValidador is not null)
                    codigoValidador = await _codigoValidacaoService.EditarAsync(codigoValidador);
                else
                    codigoValidador = await _codigoValidacaoService.InserirAsync(controle);
                
                switch (controle.Status)
                {
                    case Enums.StatusEnum.Igreja_Criacao:
                        controle.Status = Enums.StatusEnum.Igreja_Criacao_Aguardando_Codigo_Validador;
                    break;
                    case Enums.StatusEnum.Igreja_Atualizacao_Temporaria_Inserido:
                        controle.Status = Enums.StatusEnum.Igreja_Atualizacao_Aguardando_Codigo_Validador;
                    break;
                }
                
                await _controleService.EditarStatusAsync(controle.Status, controle.Id);
                #if DEBUG
                    Console.WriteLine("DEBUG");
                #else
                    var responseEmail = await _emailService.EnviarEmail(
                            [usuario.Email], 
                            $"Código para Validação", 
                            Contant.EmailValidacaoToken
                            .Replace("{nome}",usuario.Nome)
                            .Replace("{token}",codigoValidador.CodigoToken.ToString())
                            .Replace("{ano}",DataHoraHelper.Ano())
                        );
                    Console.WriteLine(@"Email enviado: {responseEmail}" ?? "Email não enviado!");
                    if (string.IsNullOrEmpty(responseEmail)) return BadRequest(new ApiResponse<dynamic>(new { mensagemInterno = "Problema no envio do email" }));
                #endif

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
    }

}
