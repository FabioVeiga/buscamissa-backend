using BuscaMissa.DTOs;
using BuscaMissa.DTOs.SettingsDto;
using BuscaMissa.DTOs.SolicitacaoDto;
using BuscaMissa.Enums;
using BuscaMissa.Models;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitacaoController(ILogger<SolicitacaoController> logger, SolicitacaoService solicitacaoService) 
    : ControllerBase
    {
        private readonly ILogger<SolicitacaoController> _logger = logger;
        private readonly SolicitacaoService _solicitacaoService = solicitacaoService;

        [HttpGet("tipos")]
        [Authorize(Roles = "App")]
        public IActionResult BuscarTodosTipos()
        {
            var lista = new List<TipoEnum>();
            foreach (var tipo in Enum.GetValues(typeof(TipoSolicitacaoEnum)))
            {
                var description = Helpers.EnumHelper.GetDescription((TipoSolicitacaoEnum)tipo);
                lista.Add(new TipoEnum((int)tipo, description));
            }
            return Ok(lista);
        }

        [HttpPost]
        [Authorize(Roles = "App")]
        public async Task<IActionResult> Validar([FromBody] SolicitacaoUsuarioRequest request)
        {
            try
            {
                if(!ModelState.IsValid) return BadRequest(ModelState);
                var model = (Solicitacao)request;
                await _solicitacaoService.InserirAsync(model);
                return Ok(new ApiResponse<dynamic>(new {NumeroSolicitacao = model.Numero}));
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

