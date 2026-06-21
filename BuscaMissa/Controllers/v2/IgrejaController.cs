using Asp.Versioning;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.v2.IgrejaDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Context;
using BuscaMissa.Enums;
using BuscaMissa.Helpers;
using BuscaMissa.Services;
using BuscaMissa.Services.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class IgrejaController(
        ILogger<IgrejaController> logger,
        BuscaMissa.Services.v2.IgrejaService igrejaServiceV2,
        BuscaMissa.Services.v2.ProximasMissasService proximasMissasService,
        GeocodingService geocodingService,
        ApplicationDbContext dbContext,
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
        public async Task<ActionResult> BuscarPorFiltro(
            [FromQuery] FiltroIgrejaV2Request filtro,
            [FromQuery] string? dia,
            [FromQuery] string? periodo)
        {
            try
            {
                // Suporte a querystring SEO: ?dia=domingo&periodo=noite
                // Resolve slugs legíveis antes da validação do model
                if (!string.IsNullOrEmpty(dia) && filtro.DiaDaSemana is null)
                    filtro.DiaDaSemana = ParseDiaSlug(dia);

                if (!string.IsNullOrEmpty(periodo) && filtro.Periodo is null)
                    filtro.Periodo = PeriodoHelper.ParseSlug(periodo);

                if (!ModelState.IsValid) return BadRequest(ModelState);

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

        private static DiaDaSemanaEnum? ParseDiaSlug(string slug) => slug.ToLowerInvariant() switch
        {
            "domingo"        => DiaDaSemanaEnum.Domingo,
            "segunda"        or "segunda-feira" => DiaDaSemanaEnum.SegundaFeira,
            "terca"          or "terça"          or "terca-feira" or "terça-feira" => DiaDaSemanaEnum.TercaFeira,
            "quarta"         or "quarta-feira"   => DiaDaSemanaEnum.QuartaFeira,
            "quinta"         or "quinta-feira"   => DiaDaSemanaEnum.QuintaFeira,
            "sexta"          or "sexta-feira"    => DiaDaSemanaEnum.SextaFeira,
            "sabado"         or "sábado"         => DiaDaSemanaEnum.Sabado,
            _ => null
        };

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

        // Página de cidade: lista paróquias + SEO (alvo "missa em {cidade}")
        [HttpGet("cidade/{uf}/{cidadeSlug}")]
        public async Task<ActionResult> BuscarPorCidadeAsync(string uf, string cidadeSlug)
        {
            try
            {
                var igrejas = await igrejaServiceV2.BuscarPorCidadeAsync(uf, cidadeSlug);
                if (igrejas.Count == 0) return NotFound();

                var cidadeNome = igrejas[0].Endereco.Localidade;
                var seo = igrejaServiceV2.GerarSeoMetadataCidade(
                    cidadeNome, uf.ToUpper(), cidadeSlug.ToLower(), igrejas.Count, FrontendBaseUrl);

                return Ok(new ApiResponse<dynamic>(new { cidade = cidadeNome, uf = uf.ToUpper(), igrejas, seo }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(ex.Message));
            }
        }

        // Paróquia individual pela URL canônica nova
        [HttpGet("paroquia/{uf}/{cidadeSlug}/{slug}")]
        public async Task<ActionResult> BuscarPorCidadeESlugAsync(string uf, string cidadeSlug, string slug)
        {
            try
            {
                var igreja = await igrejaServiceV2.BuscarPorCidadeESlugAsync(uf, cidadeSlug, slug);
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
        // Diagnóstico do endpoint proximas-missas — retorna contagens intermediárias
        [HttpGet("proximas-missas/diagnostico")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DiagnosticarProximasMissasAsync([FromQuery] ProximasMissasRequest request)
        {
            try
            {
                var todasIgrejas = await dbContext.Igrejas.CountAsync(x => x.Ativo);
                var comCoordenadas = await dbContext.Igrejas.CountAsync(x => x.Ativo && x.Endereco.Latitude != null && x.Endereco.Longitude != null);
                var comMissas = await dbContext.Igrejas.CountAsync(x => x.Ativo && x.Missas.Any());
                var prontas = await dbContext.Igrejas.CountAsync(x =>
                    x.Ativo && x.Endereco.Latitude != null && x.Endereco.Longitude != null && x.Missas.Any());

                // Carrega só as que têm coordenada E pelo menos uma missa — universo real do proximas-missas
                var candidatas = await dbContext.Igrejas
                    .Include(x => x.Endereco)
                    .AsNoTracking()
                    .Where(x => x.Ativo && x.Endereco.Latitude != null && x.Endereco.Longitude != null && x.Missas.Any())
                    .Select(x => new
                    {
                        x.Nome,
                        x.Endereco.Localidade,
                        x.Endereco.Uf,
                        Lat = x.Endereco.Latitude!.Value,
                        Lng = x.Endereco.Longitude!.Value,
                        TotalMissas = x.Missas.Count
                    })
                    .ToListAsync();

                // As 10 igrejas com missa MAIS PRÓXIMAS da coordenada informada
                var maisProximas = candidatas
                    .Select(x => new
                    {
                        x.Nome,
                        x.Localidade,
                        x.Uf,
                        x.Lat,
                        x.Lng,
                        x.TotalMissas,
                        distanciaKm = Math.Round(GeoHelper.DistanciaKm(
                            (double)request.Lat, (double)request.Lng, (double)x.Lat, (double)x.Lng), 1)
                    })
                    .OrderBy(x => x.distanciaKm)
                    .Take(10)
                    .ToList();

                var dentroDoRaio = maisProximas.Count(x => x.distanciaKm <= (double)request.RaioKm);

                return Ok(new ApiResponse<dynamic>(new
                {
                    totalAtivas = todasIgrejas,
                    comCoordenadas,
                    comMissas,
                    prontas, // com coordenada E missa — universo que o proximas-missas enxerga
                    semCoordenadas = todasIgrejas - comCoordenadas,
                    raioKm = request.RaioKm,
                    horas = request.Horas,
                    dentroDoRaio,
                    dica = dentroDoRaio == 0
                        ? "Nenhuma igreja com missa dentro do raio. Use o lat/lng da 'igrejaMaisProxima' abaixo para testar."
                        : "Há igrejas com missa no raio. Se proximas-missas ainda voltar vazio, é a janela de horas.",
                    igrejaMaisProxima = maisProximas.FirstOrDefault(),
                    dezMaisProximasComMissa = maisProximas
                }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(ex.Message));
            }
        }

        // Lista as igrejas ativas que continuam sem coordenadas — para diagnóstico
        [HttpGet("sem-coordenadas")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListarSemCoordenadasAsync()
        {
            try
            {
                var pendentes = await dbContext.Igrejas
                    .Include(x => x.Endereco)
                    .AsNoTracking()
                    .Where(x => x.Ativo && x.Endereco.Latitude == null)
                    .Select(x => new
                    {
                        x.Id,
                        x.Nome,
                        x.Endereco.Logradouro,
                        x.Endereco.Numero,
                        x.Endereco.Bairro,
                        x.Endereco.Localidade,
                        x.Endereco.Uf,
                        x.Endereco.Cep
                    })
                    .OrderBy(x => x.Uf).ThenBy(x => x.Localidade)
                    .ToListAsync();

                return Ok(new ApiResponse<dynamic>(new { total = pendentes.Count, igrejas = pendentes }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(ex.Message));
            }
        }

        // Geocodifica retroativamente igrejas sem coordenadas — manutenção admin
        [HttpPost("geocodificar-pendentes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GeocodificarPendentesAsync()
        {
            try
            {
                var pendentes = await dbContext.Igrejas
                    .Include(x => x.Endereco)
                    .Where(x => x.Ativo && x.Endereco.Latitude == null)
                    .ToListAsync();

                int geocodificadas = 0;
                foreach (var igreja in pendentes)
                {
                    await geocodingService.GeocodeAsync(igreja.Endereco);
                    if (igreja.Endereco.Latitude is not null)
                        geocodificadas++;

                    // Nominatim: máximo 1 req/segundo
                    await Task.Delay(1100);
                }

                await dbContext.SaveChangesAsync();

                return Ok(new ApiResponse<dynamic>(new
                {
                    total = pendentes.Count,
                    geocodificadas,
                    semCoordenadas = pendentes.Count - geocodificadas
                }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(ex.Message));
            }
        }

        // Missas que começam nas próximas horas próximas ao usuário — alimenta Home e tela /missa-agora
        [HttpGet("proximas-missas")]
        public async Task<IActionResult> BuscarProximasMissasAsync([FromQuery] ProximasMissasRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var resultado = await proximasMissasService.BuscarAsync(request);
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<dynamic>(ex.Message));
            }
        }
    }
}
