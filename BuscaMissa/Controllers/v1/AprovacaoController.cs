using BuscaMissa.DTOs;
using BuscaMissa.DTOs.ControleDto;
using BuscaMissa.Services.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers.v1
{
    // Fila de Aprovações Pendentes (Admin): visão sobre Controle/IgrejaTemporaria/MissaTemporaria
    // já existentes, sem nenhuma tabela de log nova.
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AprovacaoController(ILogger<AprovacaoController> logger, AprovacaoService aprovacaoService)
    : ControllerBase
    {
        [HttpGet]
        [Route("pendentes")]
        public async Task<IActionResult> BuscarPendentes([FromQuery] FiltroControleRequest filtro)
        {
            try
            {
                var resultado = await aprovacaoService.BuscarPendentesAsync(filtro);
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno));
            }
        }

        [HttpGet]
        [Route("{controleId}/detalhe")]
        public async Task<IActionResult> ObterDetalhe(int controleId)
        {
            try
            {
                var detalhe = await aprovacaoService.ObterDetalheAsync(controleId);
                if (detalhe is null) return NotFound(new ApiResponse<dynamic>(new { mensagemAplicacao = "Controle não encontrado." }));
                return Ok(new ApiResponse<dynamic>(detalhe));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno));
            }
        }

        [HttpPost]
        [Route("{controleId}/aprovar")]
        public async Task<IActionResult> Aprovar(int controleId)
        {
            try
            {
                var sucesso = await aprovacaoService.AprovarAsync(controleId);
                if (!sucesso) return BadRequest(new ApiResponse<dynamic>(new { mensagemAplicacao = "Não foi possível aprovar este item." }));
                return Ok(new ApiResponse<dynamic>(new { mensagemAplicacao = "Aprovado com sucesso." }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno));
            }
        }

        [HttpPost]
        [Route("{controleId}/ajustar")]
        public async Task<IActionResult> AjustarAlteracao(int controleId, [FromBody] AjustarAlteracaoRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var sucesso = await aprovacaoService.AjustarAlteracaoAsync(controleId, request);
                if (!sucesso) return BadRequest(new ApiResponse<dynamic>(new { mensagemAplicacao = "Não foi possível concluir o ajuste." }));
                return Ok(new ApiResponse<dynamic>(new { mensagemAplicacao = "Alteração ajustada e concluída com sucesso." }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno));
            }
        }

        [HttpPost]
        [Route("{controleId}/rejeitar")]
        public async Task<IActionResult> Rejeitar(int controleId)
        {
            try
            {
                var sucesso = await aprovacaoService.RejeitarAsync(controleId);
                if (!sucesso) return BadRequest(new ApiResponse<dynamic>(new { mensagemAplicacao = "Não foi possível rejeitar este item." }));
                return Ok(new ApiResponse<dynamic>(new { mensagemAplicacao = "Rejeitado." }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno));
            }
        }
    }
}
