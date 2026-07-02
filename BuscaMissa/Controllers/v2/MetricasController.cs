using Asp.Versioning;
using BuscaMissa.DTOs.v2.MetricasDto;
using BuscaMissa.Enums;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/metricas")]
[Authorize(Roles = "App")]
public class MetricasController(
    ILogger<MetricasController> logger,
    ServicoMetricas servicoMetricas) : ControllerBase
{
    [HttpPost("visualizacao-igreja")]
    public async Task<IActionResult> VisualizacaoIgreja([FromBody] MetricaRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest();
            await servicoMetricas.IncrementarAsync(TipoEntidadeMetricaEnum.Igreja, request.EntidadeId, TipoMetricaEnum.VisualizacaoIgreja);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("clique-rota")]
    public async Task<IActionResult> CliqueRota([FromBody] MetricaRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest();
            await servicoMetricas.IncrementarAsync(TipoEntidadeMetricaEnum.Igreja, request.EntidadeId, TipoMetricaEnum.CliqueRota);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("favorito")]
    public async Task<IActionResult> Favorito([FromBody] MetricaRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest();
            await servicoMetricas.IncrementarAsync(TipoEntidadeMetricaEnum.Igreja, request.EntidadeId, TipoMetricaEnum.Favorito);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("compartilhamento")]
    public async Task<IActionResult> Compartilhamento([FromBody] MetricaRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest();
            await servicoMetricas.IncrementarAsync(TipoEntidadeMetricaEnum.Igreja, request.EntidadeId, TipoMetricaEnum.Compartilhamento);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("clique-telefone")]
    public async Task<IActionResult> CliqueTelefone([FromBody] MetricaRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest();
            await servicoMetricas.IncrementarAsync(TipoEntidadeMetricaEnum.Igreja, request.EntidadeId, TipoMetricaEnum.CliqueTelefone);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("clique-instagram")]
    public async Task<IActionResult> CliqueInstagram([FromBody] MetricaRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest();
            await servicoMetricas.IncrementarAsync(TipoEntidadeMetricaEnum.Igreja, request.EntidadeId, TipoMetricaEnum.CliqueInstagram);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("sugestao-edicao")]
    public async Task<IActionResult> SugestaoEdicao([FromBody] MetricaRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest();
            await servicoMetricas.IncrementarAsync(TipoEntidadeMetricaEnum.Igreja, request.EntidadeId, TipoMetricaEnum.SugestaoEdicao);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
