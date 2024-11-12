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
    public class IgrejaController(ILogger<IgrejaController> logger, EmailService emailService, UsuarioService usuarioService, 
    IgrejaService igrejaService, ControleService controleService, CodigoValidacaoService codigoValidacaoService, EnderecoService enderecoService,
    ViaCepService viaCepService, AzureBlobStorageService azureBlobStorageService) : ControllerBase
    {
        private readonly ILogger<IgrejaController> _logger = logger;
        private readonly EmailService _emailService = emailService;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly IgrejaService _igrejaService = igrejaService;
        private readonly ControleService _controleService = controleService;
        private readonly CodigoValidacaoService _codigoValidacaoService = codigoValidacaoService;
        private readonly EnderecoService _enderecoService = enderecoService;
        private readonly ViaCepService _viaCepService = viaCepService;
        private readonly AzureBlobStorageService _azureBlobStorageService = azureBlobStorageService;

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
                var controle = new Controle(){Igreja = igreja, Status = Enums.StatusEnum.Igreja_Criacao };
                controle = await _controleService.InserirAsync(controle);
                var response = new CriacaoIgrejaReponse(){ControleId = controle.Id};
                return Ok(new ApiResponse<dynamic>(new { response }));
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        [HttpGet]
        [Route("buscar-por-cep")]
        public async Task<ActionResult> BuscarPorCep(string cep)
        {
            try
            {
                var temIgreja = await _igrejaService.BuscarPorCepAsync(cep);
                if (temIgreja == null)
                {
                    var endereco = await _viaCepService.ConsultarCepAsync(CepHelper.FormatarCep(cep).ToString());
                    if(endereco is null)
                        return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Liberar campos do endereço para realizar o cadastro!" }));
                    return NotFound(new ApiResponse<dynamic>(new { endereco, messagemAplicacao = "Preencher campos do endereço!" }));
                }
                var messagemAplicacao = string.Empty;
                IgrejaResponse igreja = temIgreja;
                if (!temIgreja.Ativo)
                    messagemAplicacao = "Habilitar para usuario editar e validar!";
                return Ok(new ApiResponse<dynamic>(new {
                    igreja,
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
    }
}

