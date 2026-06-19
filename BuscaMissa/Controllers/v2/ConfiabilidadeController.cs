using Asp.Versioning;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.v2.ConfiabilidadeDto;
using BuscaMissa.Services.v2;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ConfiabilidadeController(
    ConfiabilidadeService confiabilidadeService,
    ILogger<ConfiabilidadeController> logger)
    : ControllerBase
{
    private string ObterIp() =>
        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    /// <summary>
    /// Confirma que os horários de uma igreja estão corretos.
    /// Retorna 409 se o usuário já confirmou anteriormente (dedup por fingerprint + IP).
    /// </summary>
    [HttpPost("{igrejaId:int}/confirmar")]
    public async Task<IActionResult> ConfirmarHorariosAsync(
        int igrejaId,
        [FromBody] ConfirmarHorarioRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest();

            var confirmou = await confiabilidadeService.ConfirmarHorariosAsync(
                igrejaId, request, ObterIp());

            if (!confirmou)
                return Conflict(new ApiResponse<string>("Você já confirmou os horários desta paróquia."));

            return Ok(new ApiResponse<string>("Obrigado! Sua confirmação ajuda outras pessoas da comunidade."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao confirmar horários IgrejaId={IgrejaId}", igrejaId);
            return StatusCode(500, new ApiResponse<string>(ex.Message));
        }
    }

    /// <summary>
    /// Resumo de prova social: quantas pessoas confirmaram os horários
    /// nos últimos 90 dias e quando foi a confirmação mais recente.
    /// </summary>
    [HttpGet("{igrejaId:int}/resumo")]
    public async Task<IActionResult> ObterResumoAsync(int igrejaId)
    {
        try
        {
            var (total, ultima) = await confiabilidadeService.ObterResumoConfirmacoesAsync(igrejaId);
            return Ok(new ApiResponse<dynamic>(new
            {
                totalConfirmacoes = total,
                ultimaConfirmacao = ultima
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter resumo de confirmações IgrejaId={IgrejaId}", igrejaId);
            return StatusCode(500, new ApiResponse<string>(ex.Message));
        }
    }
}
