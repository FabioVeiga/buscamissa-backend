using BuscaMissa.Constants;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.ControleDto;
using BuscaMissa.DTOs.v1.EmailHtmlGenerator;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Services.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CodigoValidadorController(ILogger<CodigoValidadorController> logger, EmailService emailService, UsuarioService usuarioService,
    IgrejaService igrejaService, ControleService controleService, CodigoValidacaoService codigoValidacaoService, IgrejaTemporariaService igrejaTemporariaService,
    IConfiguration configuration)
    : ControllerBase
    {
        private string FrontendBaseUrl => configuration["FrontendBaseUrl"] ?? "https://buscamissa.com.br";
        
        [HttpPost]
        [Route("validar-igreja")]
        [Authorize(Roles = "App")]
        public async Task<IActionResult> Validar([FromBody] CodigoValidadorRequest request)
        {
            try
            {
                var mensagemTela = string.Empty;
                var usuario = await usuarioService.BuscarPorEmailAsync(request.Email);
                if (usuario == null) return NotFound(new ApiResponse<dynamic>(new { mensagemTela = "Usuário não encontrado!" }));
                if (usuario.Bloqueado) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Usuário bloqueado! Entrar em contato pelo suporte@buscamissa.com.br!" }));
                var controle = await controleService.BuscarPorIdAsync(request.ControleId);
                if (controle is null || controle.Igreja is null) return NotFound(new ApiResponse<dynamic>(new { mensagemTela = "Não existe este controle!" }));
                var codigo = await codigoValidacaoService.BuscarPorCodigoTokenAsync(request.CodigoValidador);
                if (codigo == null) return NotFound(new ApiResponse<dynamic>(new { mensagemTela = "Código não encontrado" }));
                if (controle.Status == Enums.StatusEnum.Finalizado) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Não há validação para esta operação!" }));
                var codigoValidador = codigoValidacaoService.Validar(request, codigo);
                if (string.IsNullOrEmpty(codigoValidador))
                {
                    switch (controle.Status)
                    {
                        case Enums.StatusEnum.Igreja_Criacao_Aguardando_Codigo_Validador:
                            await igrejaService.AtivarAsync(controle, usuario);
                            if(controle.Igreja.Contato is not null)
                                await EnviarEmail(controle.Igreja);
                            break;
                        case Enums.StatusEnum.Igreja_Atualizacao_Aguardando_Codigo_Validador:
                            var temporaria = await igrejaTemporariaService.BuscarPorIgrejaIdAsync(controle.Igreja.Id);
                            await igrejaService.EditarPorTemporariaAsync(controle.Igreja, temporaria!);
                            await igrejaService.AtivarAsync(controle, usuario);
                            if(controle.Igreja.Contato is not null)
                                await EnviarEmail(controle.Igreja);
                            break;
                        default:
                            return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Não há validação para esta operação!" }));
                    }
                }
                if (codigoValidador.Contains("Enviado email com o código"))
                {
                    codigo = await codigoValidacaoService.EditarAsync(codigo);
                    var responseEmail = await emailService.EnviarEmail(
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
                await controleService.EditarStatusAsync(controle.Status, controle.Id);
                var igreja = await igrejaService.BuscarPorIdAsync(controle.Igreja.Id);
                return Ok(new ApiResponse<dynamic>(new
                {
                    mensagemTela = "Igreja atualizada com sucesso!",
                    cep = igreja!.Endereco.Cep,
                }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        
        private async Task EnviarEmail(Igreja igreja, bool criacao = true)
        {
            try
            {
                var url = string.Concat(FrontendBaseUrl, "/paroquia/", igreja.Endereco.Uf.ToLower(), "/", igreja.Endereco.CidadeSlug, "/", igreja.NomeUnico);
                string assunto;
                string htmlEmail;
                if (criacao)
                {
                    // Gerar o HTML para criação
                    htmlEmail = EmailHtmlGenerator.GerarHtmlEmailCriacao(
                        igreja.Nome, 
                        igreja.Endereco.Logradouro, 
                        igreja.Endereco.Numero, 
                        igreja.Endereco.Bairro, 
                        igreja.Endereco.Localidade, 
                        igreja.Endereco.Estado,
                        igreja.Paroco,
                        url
                    );
                    assunto= $"Sua Igreja {igreja.Nome} foi cadastrada no Busca Missa!";
                }
                else
                {
                    // Gerar o HTML para alteração
                    htmlEmail = EmailHtmlGenerator.GerarHtmlEmailCriacao(
                        igreja.Nome, 
                        igreja.Endereco.Logradouro, 
                        igreja.Endereco.Numero, 
                        igreja.Endereco.Bairro, 
                        igreja.Endereco.Localidade, 
                        igreja.Endereco.Estado,
                        igreja.Paroco,
                        url
                    );
                    assunto = $"Atualização das informações da igreja {igreja.Nome} no Busca Missa";

                }
            
                await emailService.EnviarEmail(new[] { igreja.Contato?.EmailContato! }, assunto, htmlEmail);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
        }
    }
}

