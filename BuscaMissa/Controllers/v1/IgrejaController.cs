using Asp.Versioning;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Services.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class IgrejaController(ILogger<IgrejaController> logger, EmailService emailService, IgrejaService igrejaService, 
    ControleService controleService, ViaCepService viaCepService, IgrejaTemporariaService igrejaTemporariaService, ImagemService imagemService,
    EnderecoService enderecoService, ContatoService contatoService, IgrejaReportarProblemaService igrejaReportarProblemaService)
    : ControllerBase
    {
        private readonly EmailService _emailService = emailService;
        private readonly ContatoService _contatoService = contatoService;

        [HttpPost]
        [Authorize(Roles = "App")]
        public async Task<IActionResult> CriarIgreja([FromBody] CriacaoIgrejaRequest request)
        {
            try
            {
                request.Endereco.Cep = CepHelper.FormatarCep(request.Endereco.Cep);
                var igrejaResponse = await igrejaService.BuscarPorCepAsync(request.Endereco.Cep);
                if (igrejaResponse is not null) return NotFound(new ApiResponse<dynamic>(new { igrejaResponse, messagemAplicacao = "Carregar página com dados da igreja!" }));

                if(!ModelState.IsValid) return BadRequest();

                var igreja = await igrejaService.InserirAsync(request);
                var controle = new Controle() { Igreja = igreja, Status = Enums.StatusEnum.Igreja_Criacao };
                controle = await controleService.InserirAsync(controle);
                
                if (!string.IsNullOrEmpty(request.Imagem)){
                    igreja.ImagemUrl= $"{igreja.Id}{ImageHelper.BuscarExtensao(request.Imagem)}";
                    var urlTemp = imagemService.UploadAzure(request.Imagem, "igreja", igreja.ImagemUrl);
                    await igrejaService.EditarAsync(igreja);
                }

                var response = new CriacaoIgrejaReponse() { ControleId = controle.Id };
                return Ok(new ApiResponse<dynamic>(new { response, messagemAplicacao = "Seguir para usuário para criar código validador!" }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("buscar-por-cep")]
        //[Authorize(Roles = "App,Admin")]
        public async Task<ActionResult> BuscarPorCep(string cep)
        {
            try
            {
                var temIgreja = await igrejaService.BuscarPorCepAsync(CepHelper.FormatarCep(cep));
                if (temIgreja == null)
                {
                    var endereco = await viaCepService.ConsultarCepAsync(CepHelper.FormatarCep(cep).ToString());
                    if (endereco is null)
                        return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Liberar campos do endereço para realizar o cadastro!" }));
                    return NotFound(new ApiResponse<dynamic>(new { endereco, messagemAplicacao = "Preencher campos do endereço!" }));
                }
                if(!string.IsNullOrEmpty(temIgreja.ImagemUrl))
                {
                    temIgreja.ImagemUrl = imagemService.ObterUrlAzureBlob($"igreja/{temIgreja.ImagemUrl}");
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
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("buscar-por-filtro")]
        [Authorize(Roles = "App,Admin")]
        public async Task<ActionResult> BuscarPorFiltro([FromQuery] FiltroIgrejaRequest filtro)
        {
            try
            {
                var resultado = await igrejaService.BuscarPorFiltros(filtro);
                if (resultado.TotalItems == 0) return NotFound();
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPut]
        [Authorize(Roles = "App")]
        public async Task<IActionResult> Atualizar([FromBody] AtualicaoIgrejaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var temIgreja = await igrejaService.BuscarPorIdAsync(request.Id);
                if (temIgreja is null) return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Igreja não encontrada!" }));
                var controle = await controleService.BuscarPorIgrejaIdAsync(request.Id);
                controle ??= await controleService.InserirAsync(new Controle() { Status = Enums.StatusEnum.Igreja_Atualizacao_Temporaria_Inserido, IgrejaId = temIgreja.Id });
               
                if(request.Imagem is not null)
                {
                    if(temIgreja.ImagemUrl is not null && request.Imagem.Contains(temIgreja.ImagemUrl))
                        request.Imagem = null;
                }

                var resultado = await igrejaTemporariaService.InserirAsync(request);
                if (!resultado) return UnprocessableEntity();
                controle.Status = Enums.StatusEnum.Igreja_Atualizacao_Temporaria_Inserido;
                await controleService.EditarStatusAsync(controle.Status, controle.Id);
                var response = new CriacaoIgrejaReponse() { ControleId = controle.Id };
                return Ok(new ApiResponse<dynamic>(new { response, messagemAplicacao = "Seguir para usuário para criar código validador!" }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
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
                var igreja = await igrejaService.BuscarPorIdAsync(igrejaId);
                if(igreja is null) return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Igreja não encontrada!" }));
                
                var controle = await controleService.BuscarPorIgrejaIdAsync(igrejaId);
                var resultado = await igrejaTemporariaService.BuscarPorIgrejaIdAsync(igrejaId);
                if (resultado is null) 
                {
                    resultado = new AtualizacaoIgrejaResponse(){
                        Id = igreja.Id,
                        Nome = igreja.Nome,
                        Paroco = igreja.Paroco,
                        Endereco = new EnderecoIgrejaResponse()
                        {
                            Id = igreja.Endereco!.Id,
                            Cep = igreja.Endereco.Cep,
                            Logradouro = igreja.Endereco.Logradouro,
                            Complemento = igreja.Endereco.Complemento,
                            Bairro = igreja.Endereco.Bairro,
                            Localidade = igreja.Endereco.Localidade,
                            Uf = igreja.Endereco.Uf,
                            Estado = igreja.Endereco.Estado,
                            Regiao = igreja.Endereco.Regiao,
                            Numero = igreja.Endereco.Numero,
                        },
                        MissasTemporaria = [.. igreja.Missas.Select(x => new MissaResponse(){
                            Id = x.Id,
                            DiaSemana = x.DiaSemana,
                            Horario = x.Horario.ToString(),
                            Observacao = x.Observacao
                        })]
                    };
                }
                if(!string.IsNullOrEmpty(resultado.ImagemUrl))
                {
                    resultado.ImagemUrl = $"data:image/png;base64,{resultado.ImagemUrl}";
                }else{
                    if(igreja.ImagemUrl is not null){
                        resultado.ImagemUrl = imagemService.ObterUrlAzureBlob($"igreja/{igreja.ImagemUrl}");
                    }else{
                        resultado.ImagemUrl = null;
                    }
                }
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("infos")]
        [Authorize(Roles = "App")]
        public ActionResult InformacoesGerais()
        {
            try
            {
                var resultado = igrejaService.InformacoesGeraisResponse();
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    
        [HttpPost]
        [Route("reportar-problema/{igrejaId}")]
        [Authorize(Roles = "App")]
        public async Task<ActionResult> ReportarProblema(int igrejaId, [FromBody] ReportarProblemaRequest request){
            try
            {
                if(!ModelState.IsValid) return BadRequest();
                var resultado = await igrejaService.BuscarPorIdAsync(igrejaId);
                if(resultado == null) return NotFound();
                request.IgrejaId = igrejaId;
                var problemaReportado = await igrejaReportarProblemaService.BuscarPorIgrejaIdAsync(igrejaId);
                var resultadoReportarProblema = false;
                if(problemaReportado is not null && !string.IsNullOrEmpty(problemaReportado.AcaoRealizada))
                {
                    problemaReportado.AcaoRealizada = null;
                    problemaReportado.Descricao = request.Descricao;
                    problemaReportado.Nome = request.Nome;
                    problemaReportado.Email = request.Email;
                    resultadoReportarProblema = await igrejaReportarProblemaService.AtualizarAsync(problemaReportado);
                }else{
                    resultadoReportarProblema = await igrejaReportarProblemaService.InserirAsync(request);
                }

                return Ok(new ApiResponse<dynamic>(new { resultadoReportarProblema, messagemAplicacao = "Aguarde a resposta do administrador!" }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    
        [HttpGet]
        [Route("obter-enderecos")]
        [Authorize(Roles = "App")]
        public async Task<ActionResult> ObterDadosDeBuscaAsync([FromQuery] EnderecoIgrejaBuscaRequest request)
        {   
            try
            {
                if(!ModelState.IsValid) return BadRequest();
                var resultado = await enderecoService.BuscarDadosBuscaAsync(request);
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("v2/obter-enderecos")]
        [Authorize(Roles = "App, Admin")]
        public async Task<ActionResult> ObterDadosDeBuscaAsync()
        {   
            try
            {
                var resultado = await enderecoService.OrganizarEnderecosAsync();
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    
    }
}

