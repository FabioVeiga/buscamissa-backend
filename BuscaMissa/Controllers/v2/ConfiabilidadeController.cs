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
    /// Retorna 409 se o usuário já confirmou anteriormente.
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
    /// Registra um reporte de erro nos horários.
    /// Retorna 409 se o mesmo usuário já reportou essa igreja nas últimas 24h.
    /// </summary>
    [HttpPost("{igrejaId:int}/reportar")]
    public async Task<IActionResult> ReportarHorarioAsync(
        int igrejaId,
        [FromBody] ReportarHorarioRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest();

            var reportou = await confiabilidadeService.ReportarHorarioAsync(
                igrejaId, request, ObterIp());

            if (!reportou)
                return Conflict(new ApiResponse<string>("Você já enviou um reporte para esta paróquia recentemente. Aguarde 24h para enviar outro."));

            return Ok(new ApiResponse<string>("Reporte recebido! Sua contribuição será analisada antes de qualquer publicação."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao reportar horário IgrejaId={IgrejaId}", igrejaId);
            return StatusCode(500, new ApiResponse<string>(ex.Message));
        }
    }
}
