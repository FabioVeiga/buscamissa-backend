using BuscaMissa.DTOs;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IgrejaController(ILogger<IgrejaController> logger, EmailService emailService, IgrejaService igrejaService, 
    ControleService controleService, ViaCepService viaCepService, IgrejaTemporariaService igrejaTemporariaService) 
    : ControllerBase
    {
        private readonly ILogger<IgrejaController> _logger = logger;
        private readonly EmailService _emailService = emailService;
        private readonly IgrejaService _igrejaService = igrejaService;
        private readonly ControleService _controleService = controleService;
        private readonly ViaCepService _viaCepService = viaCepService;
        private readonly IgrejaTemporariaService _igrejaTemporariaService = igrejaTemporariaService;

        [HttpPost]
        [Authorize(Roles = "Admin,App")]
        public async Task<IActionResult> CriarIgreja([FromBody] CriacaoIgrejaRequest request)
        {
            try
            {
                request.Endereco.Cep = CepHelper.FormatarCep(request.Endereco.Cep);
                var igrejaResponse = await _igrejaService.BuscarPorCepAsync(request.Endereco.Cep);
                if (igrejaResponse is not null) return NotFound(new ApiResponse<dynamic>(new { igrejaResponse, messagemAplicacao = "Carregar página com dados da igreja!" }));
                var igreja = await _igrejaService.InserirAsync(request);
                var controle = new Controle() { Igreja = igreja, Status = Enums.StatusEnum.Igreja_Criacao };
                controle = await _controleService.InserirAsync(controle);
                var response = new CriacaoIgrejaReponse() { ControleId = controle.Id };
                return Ok(new ApiResponse<dynamic>(new { response, messagemAplicacao = "Seguir para usuário para criar códifgo validador!" }));
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        [Route("buscar-por-cep")]
        [Authorize(Roles = "App")]
        public async Task<ActionResult> BuscarPorCep(string cep)
        {
            try
            {
                var temIgreja = await _igrejaService.BuscarPorCepAsync(cep);
                if (temIgreja == null)
                {
                    var endereco = await _viaCepService.ConsultarCepAsync(CepHelper.FormatarCep(cep).ToString());
                    if (endereco is null)
                        return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Liberar campos do endereço para realizar o cadastro!" }));
                    return NotFound(new ApiResponse<dynamic>(new { endereco, messagemAplicacao = "Preencher campos do endereço!" }));
                }
                var messagemAplicacao = string.Empty;
                IgrejaResponse response = temIgreja;
                if (!temIgreja.Ativo)
                    messagemAplicacao = "Habilitar para usuario editar e validar!";
                return Ok(new ApiResponse<dynamic>(new
                {
                    response,
                    messagemAplicacao
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("buscar-por-filtro")]
        [Authorize(Roles = "App")]
        public async Task<ActionResult> BuscarPorFiltro([FromQuery] FiltroIgrejaRequest filtro)
        {
            try
            {
                var resultado = await _igrejaService.BuscarPorFiltros(filtro);
                if (resultado.TotalItems == 0) return NotFound();
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPut]
        [Authorize(Roles = "App,Admin")]
        public async Task<IActionResult> Atualizar([FromBody] AtualicaoIgrejaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var temIgreja = await _igrejaService.BuscarPorIdAsync(request.Id);
                if (temIgreja is null) return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Igreja não encontrada!" }));
                var controle = await _controleService.BuscarPorIgrejaIdAsync(request.Id);
                if (controle == null) return NotFound();
                var resultado = await _igrejaTemporariaService.InserirAsync(request);
                if (!resultado) return UnprocessableEntity();
                controle.Status = Enums.StatusEnum.Igreja_Atualizacao_Temporaria_Inserido;
                await _controleService.EditarAsync(controle);
                var response = new CriacaoIgrejaReponse() { ControleId = controle.Id };
                return Ok(new ApiResponse<dynamic>(new { response, messagemAplicacao = "Seguir para usuário para criar códifgo validador!" }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    
        [HttpGet]
        [Route("buscar-por-atualizacoes/{igrejaId}")]
        [Authorize(Roles = "App")]
        public async Task<ActionResult> BuscarPorIgrejaIdAsync(int igrejaId)
        {
            try
            {
                var resultado = await _igrejaTemporariaService.BuscarPorIgrejaIdAsync(igrejaId);
                if (resultado is null) return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Não tem nenhuma atualização!" }));
                return Ok(new ApiResponse<dynamic>(resultado));
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

