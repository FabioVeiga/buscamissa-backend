using BuscaMissa.Constants;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.ControleDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CodigoValidadorController(ILogger<CodigoValidadorController> logger, EmailService emailService, UsuarioService usuarioService,
    IgrejaService igrejaService, ControleService controleService, CodigoValidacaoService codigoValidacaoService, IgrejaTemporariaService igrejaTemporariaService,
    ImagemService imagemService)
    : ControllerBase
    {
        private readonly ILogger<CodigoValidadorController> _logger = logger;
        private readonly EmailService _emailService = emailService;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly IgrejaService _igrejaService = igrejaService;
        private readonly ControleService _controleService = controleService;
        private readonly CodigoValidacaoService _codigoValidacaoService = codigoValidacaoService;
        private readonly IgrejaTemporariaService _igrejaTemporariaService = igrejaTemporariaService;
        private readonly ImagemService _imagemService = imagemService;

        [HttpPost]
        [Route("validar-igreja")]
        [Authorize(Roles = "App")]
        public async Task<IActionResult> Validar([FromBody] CodigoValidadorRequest request)
        {
            try
            {
                var mensagemTela = string.Empty;
                var usuario = await _usuarioService.BuscarPorEmailAsync(request.Email);
                if (usuario == null) return NotFound(new ApiResponse<dynamic>(new { mensagemTela = "Usuário não encontrado!" }));
                if (usuario.Bloqueado) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Usuário bloqueado! Entrar em contato pelo suporte@buscamissa.com.br!" }));
                var controle = await _controleService.BuscarPorIdAsync(request.ControleId);
                if (controle is null || controle.Igreja is null) return NotFound(new ApiResponse<dynamic>(new { mensagemTela = "Não existe este controle!" }));
                var codigo = await _codigoValidacaoService.BuscarPorCodigoTokenAsync(request.CodigoValidador);
                if (codigo == null) return NotFound(new ApiResponse<dynamic>(new { mensagemTela = "Código não encontrado" }));
                if (controle.Status == Enums.StatusEnum.Finalizado) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Não há validação para esta operação!" }));
                var codigoValidador = _codigoValidacaoService.Validar(request, codigo);
                if (string.IsNullOrEmpty(codigoValidador))
                {
                    switch (controle.Status)
                    {
                        case Enums.StatusEnum.Igreja_Criacao_Aguardando_Codigo_Validador:
                            await _igrejaService.AtivarAsync(controle, usuario);
                            mensagemTela = "Igreja ativada com sucesso!";
                            break;
                        case Enums.StatusEnum.Igreja_Atualizacao_Aguardando_Codigo_Validador:
                            var temporaria = await _igrejaTemporariaService.BuscarPorIgrejaIdAsync(controle.Igreja.Id);
                            var alterado = await _igrejaService.EditarPorTemporariaAsync(controle.Igreja, temporaria!);
                            await _igrejaService.AtivarAsync(controle, usuario);
                            mensagemTela = "Igreja atualizada com sucesso!";
                            break;
                        default:
                            return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Não há validação para esta operação!" }));
                    }
                }
                if (codigoValidador.Contains("Enviado email com o código"))
                {
                    codigo = await _codigoValidacaoService.EditarAsync(codigo);
                    var responseEmail = await _emailService.EnviarEmail(
                        [usuario.Email],
                        $"Código para Validação",
                        Contant.EmailValidacaoToken
                        .Replace("{nome}", usuario.Nome)
                        .Replace("{token}", codigo.CodigoToken.ToString())
                        .Replace("{ano}", DataHoraHelper.Ano())
                    );
                    Console.WriteLine(@"Email enviado: {responseEmail}" ?? "Email não enviado!");
                    if (string.IsNullOrEmpty(responseEmail)) return BadRequest(new ApiResponse<dynamic>(new { mensagemInterno = "Problema no envio do email" }));
                    return Ok(new ApiResponse<dynamic>(new
                    {
                        mensagemTela = "Novo código validador enviado código para o email!",
#if DEBUG
                        codigoValidador = codigo.CodigoToken
#endif
                    }));
                }
                controle.Status = Enums.StatusEnum.Finalizado;
                await _controleService.EditarStatusAsync(controle.Status, controle.Id);
                var igreja = await _igrejaService.BuscarPorIdAsync(controle.Igreja.Id);
                return Ok(new ApiResponse<dynamic>(new
                {
                    mensagemTela = "Igreja atualizada com sucesso!",
                    cep = igreja!.Endereco.Cep,
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

