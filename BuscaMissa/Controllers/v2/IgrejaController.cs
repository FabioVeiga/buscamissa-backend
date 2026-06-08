using Asp.Versioning;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.v2.IgrejaDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Services.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class IgrejaController(
        ILogger<IgrejaController> logger,
        BuscaMissa.Services.v2.IgrejaService igrejaServiceV2,
        IgrejaService igrejaService,
        ControleService controleService,
        ViaCepService viaCepService,
        ImagemService imagemService,
        IConfiguration configuration)
    : ControllerBase
    {
        private string FrontendBaseUrl => configuration["FrontendBaseUrl"] ?? "https://buscamissa.com.br";

        // Item 7 (v2): retorna 409 Conflict quando NomeUnico já existe, com dados da igreja existente
        [HttpPost]
        [Authorize(Roles = "App")]
        public async Task<IActionResult> CriarIgrejaAsync([FromBody] PostIgrejaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();

                var igrejaExistente = await igrejaServiceV2.TemNomeNomeUnicoAsync(request.NomeUnico);
                if (igrejaExistente is not null)
                    return Conflict(new ApiResponse<dynamic>(new
                    {
                        igrejaExistente = (IgrejaResponse)igrejaExistente,
                        messagemAplicacao = "Nome único já existe. Carregar página com dados da igreja!"
                    }));

                request.Endereco.Cep = CepHelper.FormatarCep(request.Endereco.Cep);

                var igreja = await igrejaServiceV2.InserirAsync(request);
                var controle = new Controle() { Igreja = igreja, Status = Enums.StatusEnum.Igreja_Criacao };
                controle = await controleService.InserirAsync(controle);

                // Item 8 (v2): imagem armazenada como blob, nunca como base64 na resposta
                if (!string.IsNullOrEmpty(request.Imagem))
                {
                    igreja.ImagemUrl = $"{igreja.Id}{ImageHelper.BuscarExtensao(request.Imagem)}";
                    imagemService.UploadAzure(request.Imagem, "igreja", igreja.ImagemUrl);
                    await igrejaService.EditarAsync(igreja);
                }

                var response = new CriacaoIgrejaReponse() { ControleId = controle.Id };
                return Ok(new ApiResponse<dynamic>(new { response, messagemAplicacao = "Seguir para usuário para criar código validador!" }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(ex.Message));
            }
        }

        [HttpGet("validar-por-nome-unico/{nomeUnico}")]
        [Authorize(Roles = "App,Admin")]
        public async Task<ActionResult> TemNomeUnicoAsync(string nomeUnico)
        {
            try
            {
                var temNomeUnico = await igrejaServiceV2.TemNomeNomeUnicoAsync(nomeUnico);
                return Ok(temNomeUnico is not null ? new ApiResponse<dynamic>(true) : new ApiResponse<dynamic>(false));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(ex.Message));
            }
        }

        [HttpGet("buscar-por-cep/{cep}")]
        [Authorize(Roles = "App,Admin")]
        public async Task<ActionResult> BuscarPorCepAsync(string cep)
        {
            try
            {
                var temIgreja = await igrejaServiceV2.BuscarPorCepAsync(CepHelper.FormatarCep(cep));
                var endereco = await viaCepService.ConsultarCepAsync(CepHelper.FormatarCep(cep));
                if (!temIgreja.Any())
                {
                    return NotFound(endereco is null
                        ? new ApiResponse<dynamic>(new { messagemAplicacao = "Liberar campos do endereço para realizar o cadastro!" })
                        : new ApiResponse<dynamic>(new { endereco, messagemAplicacao = "Preencher campos do endereço!" }));
                }

                return Ok(new ApiResponse<dynamic>(
                    temIgreja.Select(x => new
                    {
                        x.Id, x.Nome, x.NomeUnico, dadosEndereco = endereco
                    })));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(ex.Message));
            }
        }

        [HttpGet]
        [Route("buscar-por-filtro")]
        public async Task<ActionResult> BuscarPorFiltro([FromQuery] FiltroIgrejaV2Request filtro)
        {
            try
            {
                var resultado = await igrejaServiceV2.BuscarPorFiltros(filtro);
                if (resultado.TotalItems == 0) return NotFound();
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(ex.Message));
            }
        }

        // Item 2: endpoint público para busca por NomeUnico (sem autenticação, para SEO)
        // Item 5: retorna metadados de SEO junto com os dados da igreja
        [HttpGet("{nomeUnico}")]
        public async Task<ActionResult> BuscarPorNomeUnicoAsync(string nomeUnico)
        {
            try
            {
                var igreja = await igrejaServiceV2.BuscarPorNomeUnicoAsync(nomeUnico);
                if (igreja is null) return NotFound();

                var seo = igrejaServiceV2.GerarSeoMetadata(igreja, FrontendBaseUrl);

                return Ok(new ApiResponse<dynamic>(new { igreja, seo }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(ex.Message));
            }
        }
    }
}
