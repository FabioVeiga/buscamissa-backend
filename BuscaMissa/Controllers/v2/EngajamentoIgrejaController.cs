using Asp.Versioning;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.DTOs.v2.IgrejaDto;
using BuscaMissa.Services.v2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class EngajamentoIgrejaController(
    ILogger<EngajamentoIgrejaController> logger,
    ServicoEngajamentoIgreja servicoEngajamentoIgreja,
    IgrejaService igrejaService
    ) : ControllerBase
{
    [HttpPost("{id}/curtir")]
    [Authorize(Roles = "App")]
    public async Task<IActionResult> Curtir(int id, [FromBody] CurtidaRequest request)
    {
        try
        {
            if(!ModelState.IsValid) return BadRequest();
            var igreja = await igrejaService.ObterPorIdAsync(id);
            if(igreja == null) return NotFound();
        
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            request.IgrejaId = id;

            await servicoEngajamentoIgreja.Curtir(request, ip);
        
            return Ok(new ApiResponse<dynamic>(new { messagemAplicacao = "Curtida realizada com sucesso!" }));
        }
        catch (ApplicationException ex)
        {
            logger.LogError("{Ex}", ex);
            var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
            return StatusCode(StatusCodes.Status400BadRequest, response);
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
       
    }

    [HttpPost("{id}/avaliar")]
    [Authorize(Roles = "App")]
    public async Task<IActionResult> Avaliar(int id, [FromBody] AvaliacaoRequest request)
    {
        try
        {
            if(!ModelState.IsValid) return BadRequest();
            var igreja = await igrejaService.ObterPorIdAsync(id);
            if(igreja == null) return NotFound();
            
            request.IgrejaId = id;

            await servicoEngajamentoIgreja.AvaliarAsync(request);
        
            return Ok(new ApiResponse<dynamic>(new { messagemAplicacao = "Avaliada com sucesso!" }));
        }
        catch (ApplicationException ex)
        {
            logger.LogError("{Ex}", ex);
            var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
            return StatusCode(StatusCodes.Status400BadRequest, response);
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }

    [HttpPost("{id}/comentar")]
    [Authorize(Roles = "App")]
    public async Task<IActionResult> Comentar(int id, [FromBody] ComentarioRequest request)
    {
        try
        {
            if(!ModelState.IsValid) return BadRequest();
            var igreja = await igrejaService.ObterPorIdAsync(id);
            if(igreja == null) return NotFound();
            
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            request.IgrejaId = id;

            await servicoEngajamentoIgreja.ComentarAsync(request, ip);

            return Ok(new ApiResponse<dynamic>(new { messagemAplicacao = "Comentario enviado com sucesso!" }));
        }
        catch (ApplicationException ex)
        {
            logger.LogError("{Ex}", ex);
            var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
            return StatusCode(StatusCodes.Status400BadRequest, response);
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
        
    }
    
    [HttpGet("{id}/comentarios")]
    [Authorize(Roles = "App")]
    public async Task<IActionResult> ObterComentarios(int id, [FromQuery] PaginacaoRequest paginacao)
    {
        try
        {
            var igreja = await igrejaService.ObterPorIdAsync(id);
            if (igreja == null) return NotFound();

            var comentarios = await servicoEngajamentoIgreja.ObterComentarios(id, paginacao);

            return Ok(new ApiResponse<dynamic>(comentarios));
        }
        catch (ApplicationException ex)
        {
            logger.LogError("{Ex}", ex);
            var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
            return StatusCode(StatusCodes.Status400BadRequest, response);
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }
    
    [HttpPost("{id}/visualizar")]
    [Authorize(Roles = "App")]
    public async Task<IActionResult> RegistrarVisualizacao(int id, [FromBody] VisualizacaoRequest request)
    {
        try
        {
            if(!ModelState.IsValid) return BadRequest();

            var igreja = await igrejaService.ObterPorIdAsync(id);
            if (igreja == null) return NotFound();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            await servicoEngajamentoIgreja.RegistrarVisualizacao(id, request.Fingerprint, ip);

            return Ok(new ApiResponse<dynamic>(new { mensagemAplicacao = "Visualização registrada com sucesso!" }));
        }
        catch (ApplicationException ex)
        {
            logger.LogError("{Ex}", ex);
            var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
            return StatusCode(StatusCodes.Status400BadRequest, response);
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }

    
    [HttpGet("{id}/engajamento")]
    [Authorize(Roles = "App")]
    public async Task<IActionResult> ObterEngajamento(int id)
    {
        var resultado = await servicoEngajamentoIgreja.ObterEstatisticas(id);

        return Ok(resultado);
    }

    [HttpGet("trending")]
    [Authorize(Roles = "App")]
    public async Task<IActionResult> Trending()
    {
        var resultado = await servicoEngajamentoIgreja.ObterTrending();

        return Ok(resultado);
    }
}