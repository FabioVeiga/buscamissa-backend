using BuscaMissa.DTOs;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.MissaDto;
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
    ControleService controleService, ViaCepService viaCepService, IgrejaTemporariaService igrejaTemporariaService, ImagemService imagemService,
    EnderecoService enderecoService, ContatoService contatoService, IgrejaDenunciaService igrejaDenunciaService) 
    : ControllerBase
    {
        private readonly ILogger<IgrejaController> _logger = logger;
        private readonly EmailService _emailService = emailService;
        private readonly IgrejaService _igrejaService = igrejaService;
        private readonly ControleService _controleService = controleService;
        private readonly ViaCepService _viaCepService = viaCepService;
        private readonly IgrejaTemporariaService _igrejaTemporariaService = igrejaTemporariaService;
        private readonly ImagemService _imagemService = imagemService;
        private readonly EnderecoService _enderecoService = enderecoService;
        private readonly ContatoService _contatoService = contatoService;
        private readonly IgrejaDenunciaService _igrejaDenunciaService = igrejaDenunciaService;

        [HttpPost]
        [Authorize(Roles = "App")]
        public async Task<IActionResult> CriarIgreja([FromBody] CriacaoIgrejaRequest request)
        {
            try
            {
                request.Endereco.Cep = CepHelper.FormatarCep(request.Endereco.Cep);
                var igrejaResponse = await _igrejaService.BuscarPorCepAsync(request.Endereco.Cep);
                if (igrejaResponse is not null) return NotFound(new ApiResponse<dynamic>(new { igrejaResponse, messagemAplicacao = "Carregar página com dados da igreja!" }));

                if(!ModelState.IsValid) return BadRequest();

                var igreja = await _igrejaService.InserirAsync(request);
                var controle = new Controle() { Igreja = igreja, Status = Enums.StatusEnum.Igreja_Criacao };
                controle = await _controleService.InserirAsync(controle);
                
                if (!string.IsNullOrEmpty(request.Imagem)){
                    igreja.ImagemUrl= $"{igreja.Id}{ImageHelper.BuscarExtensao(request.Imagem)}";
                    var urlTemp = _imagemService.UploadAzure(request.Imagem, "igreja", igreja.ImagemUrl);
                    await _igrejaService.EditarAsync(igreja);
                }

                var response = new CriacaoIgrejaReponse() { ControleId = controle.Id };
                return Ok(new ApiResponse<dynamic>(new { response, messagemAplicacao = "Seguir para usuário para criar código validador!" }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("buscar-por-cep")]
        [Authorize(Roles = "App,Admin")]
        public async Task<ActionResult> BuscarPorCep(string cep)
        {
            try
            {
                var temIgreja = await _igrejaService.BuscarPorCepAsync(CepHelper.FormatarCep(cep));
                if (temIgreja == null)
                {
                    var endereco = await _viaCepService.ConsultarCepAsync(CepHelper.FormatarCep(cep).ToString());
                    if (endereco is null)
                        return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Liberar campos do endereço para realizar o cadastro!" }));
                    return NotFound(new ApiResponse<dynamic>(new { endereco, messagemAplicacao = "Preencher campos do endereço!" }));
                }
                if(!string.IsNullOrEmpty(temIgreja.ImagemUrl))
                {
                    temIgreja.ImagemUrl = _imagemService.ObterUrlAzureBlob($"igreja/{temIgreja.ImagemUrl}");
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
        [Authorize(Roles = "App,Admin")]
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
        [Authorize(Roles = "App")]
        public async Task<IActionResult> Atualizar([FromBody] AtualicaoIgrejaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var temIgreja = await _igrejaService.BuscarPorIdAsync(request.Id);
                if (temIgreja is null) return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Igreja não encontrada!" }));
                var controle = await _controleService.BuscarPorIgrejaIdAsync(request.Id);
                controle ??= await _controleService.InserirAsync(new Controle() { Status = Enums.StatusEnum.Igreja_Atualizacao_Temporaria_Inserido, IgrejaId = temIgreja.Id });
               
                if(request.Imagem is not null)
                {
                    if(temIgreja.ImagemUrl is not null && request.Imagem.Contains(temIgreja.ImagemUrl))
                        request.Imagem = null;
                }

                var resultado = await _igrejaTemporariaService.InserirAsync(request);
                if (!resultado) return UnprocessableEntity();
                controle.Status = Enums.StatusEnum.Igreja_Atualizacao_Temporaria_Inserido;
                await _controleService.EditarStatusAsync(controle.Status, controle.Id);
                var response = new CriacaoIgrejaReponse() { ControleId = controle.Id };
                return Ok(new ApiResponse<dynamic>(new { response, messagemAplicacao = "Seguir para usuário para criar código validador!" }));
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
                var igreja = await _igrejaService.BuscarPorIdAsync(igrejaId);
                if(igreja is null) return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Igreja não encontrada!" }));
                
                var controle = await _controleService.BuscarPorIgrejaIdAsync(igrejaId);
                var resultado = await _igrejaTemporariaService.BuscarPorIgrejaIdAsync(igrejaId);
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
                        resultado.ImagemUrl = _imagemService.ObterUrlAzureBlob($"igreja/{igreja.ImagemUrl}");
                    }else{
                        resultado.ImagemUrl = null;
                    }
                }
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
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
                var resultado = _igrejaService.InformacoesGeraisResponse();
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
    
        [HttpPost]
        [Route("denunciar/{igrejaId}")]
        [Authorize(Roles = "App")]
        public async Task<ActionResult> Denunciar(int igrejaId, [FromBody] DenunciarIgrejaRequest request){
            try
            {
                if(!ModelState.IsValid) return BadRequest();
                var resultado = await _igrejaService.BuscarPorIdAsync(igrejaId);
                if(resultado == null) return NotFound();
                request.IgrejaId = igrejaId;
                var denuncia = await _igrejaDenunciaService.BuscarPorIgrejaIdAsync(igrejaId);
                var resultadoDenuncia = false;
                if(denuncia is not null && !string.IsNullOrEmpty(denuncia.AcaoRealizada))
                {
                    denuncia.AcaoRealizada = null;
                    denuncia.Descricao = request.Descricao;
                    denuncia.Titulo = request.Titulo;
                    denuncia.NomeDenunciador = request.NomeDenunciador;
                    denuncia.EmailDenunciador = request.EmailDenunciador;
                    resultadoDenuncia = await _igrejaDenunciaService.AtualizarAsync(denuncia);
                }else{
                    resultadoDenuncia = await _igrejaDenunciaService.InserirAsync(request);
                }

                //return Ok(new ApiResponse<dynamic>(resultadoDenuncia));
                return Ok(new ApiResponse<dynamic>(new { resultadoDenuncia, messagemAplicacao = "Aguarde a resposta do administrador!" }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
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
                var resultado = await _enderecoService.BuscarDadosBuscaAsync(request);
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("v2/obter-enderecos")]
        [Authorize(Roles = "App")]
        public async Task<ActionResult> ObterDadosDeBuscaAsync()
        {   
            try
            {
                var resultado = await _enderecoService.OrganizarEnderecosAsync();
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

