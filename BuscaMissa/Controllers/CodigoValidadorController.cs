using BuscaMissa.DTOs;
using BuscaMissa.DTOs.ControleDto;
using BuscaMissa.Models;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CodigoValidadorController(ILogger<CodigoValidadorController> logger, EmailService emailService, UsuarioService usuarioService, 
    IgrejaService igrejaService, ControleService controleService, CodigoValidacaoService codigoValidacaoService,  AzureBlobStorageService azureBlobStorageService) 
    : ControllerBase
    {
        private readonly ILogger<CodigoValidadorController> _logger = logger;
        private readonly EmailService _emailService = emailService;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly IgrejaService _igrejaService = igrejaService;
        private readonly ControleService _controleService = controleService;
        private readonly CodigoValidacaoService _codigoValidacaoService = codigoValidacaoService;
        private readonly AzureBlobStorageService _azureBlobStorageService = azureBlobStorageService;

        [HttpPost]
        [Route("validar-igreja")]
        [Authorize(Roles = "App")]
        public async Task<IActionResult> Validar([FromBody] CodigoValidadorRequest request)
        {
            try
            {
                var usuario = await _usuarioService.BuscarPorEmailAsync(request.Email);
                if (usuario == null) return NotFound(new ApiResponse<dynamic>(new { mensagemTela = "Usuário não encontrado!" }));

                var controle = await _controleService.BuscarPorIdAsync(request.ControleId);
                if (controle is null || controle.Igreja is null) return NotFound(new ApiResponse<dynamic>(new { mensagemTela = "Não existe este controle!" }));
                var codigo = await _codigoValidacaoService.BuscarPorCodigoTokenAsync(request.CodigoValidador);
                if (codigo == null) return NotFound(new ApiResponse<dynamic>(new { mensagemTela = "Código não encontrado" }));
                if(controle.Status == Enums.StatusEnum.Finalizado) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Não há validação para esta operação!" }));
                var validarCodigo = _codigoValidacaoService.Validar(request, codigo);
                if(string.IsNullOrEmpty(validarCodigo))
                {
                    switch (controle.Status)
                    {
                        case Enums.StatusEnum.Igreja_Criacao_Aguardando_Codigo_Validador:
                            var response = await _igrejaService.AtivarAsync(controle, usuario);
                            return Ok(new ApiResponse<dynamic>(new { mensagemTela = "Igreja ativada com sucesso!" }));
                        default:
                            break;
                    }
                    return Ok();
                }
                if(validarCodigo.Contains("Enviado email com o código"))
                {
                    codigo = await _codigoValidacaoService.EditarAsync(codigo);
                    //enviar email
                }
                return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = validarCodigo }));
            }
            catch (Exception)
            {
                
                throw;
            }
        }


        private async Task<string?> EnviarEmailComCodigoToken(CodigoPermissao codigo, Usuario usuario)
        {
            var dic = EmailService.DicionarioParaEnvioDoCodigo(usuario.Nome,codigo.CodigoToken, codigo.ValidoAte);
            #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var enviarEmail = await _emailService.EnviarCodigoValidacao([usuario.Email], "BuscaMissa - Código para validar", null, dic);
            #pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return enviarEmail;
        }
    }
}

