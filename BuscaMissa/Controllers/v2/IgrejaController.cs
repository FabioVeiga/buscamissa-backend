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
        EmailService emailService, 
        BuscaMissa.Services.v2.IgrejaService igrejaServiceV2, 
        IgrejaService igrejaService, 
    ControleService controleService, ViaCepService viaCepService, IgrejaTemporariaService igrejaTemporariaService, ImagemService imagemService,
    EnderecoService enderecoService, ContatoService contatoService, IgrejaDenunciaService igrejaDenunciaService) 
    : ControllerBase
    {
        private readonly EmailService _emailService = emailService;
        private readonly ContatoService _contatoService = contatoService;

        [HttpPost]
        [Authorize(Roles = "App")]
        public async Task<IActionResult> CriarIgrejaAsync([FromBody] PostIgrejaRequest request)
        {
            try
            {
                if(!ModelState.IsValid) return BadRequest();
                
                var nomeUnico = await igrejaServiceV2.TemNomeNomeUnicoAsync(request.NomeUnico);
                if(nomeUnico is not null)
                    return BadRequest(new ApiResponse<dynamic>(new { messagemAplicacao = "Nome unico já existe." }));
                
                request.Endereco.Cep = CepHelper.FormatarCep(request.Endereco.Cep);

                var igreja = await igrejaServiceV2.InserirAsync(request);
                var controle = new Controle() { Igreja = igreja, Status = Enums.StatusEnum.Igreja_Criacao };
                controle = await controleService.InserirAsync(controle);
                
                if (!string.IsNullOrEmpty(request.Imagem)){
                    igreja.ImagemUrl= $"{igreja.Id}{ImageHelper.BuscarExtensao(request.Imagem)}";
                    imagemService.UploadAzure(request.Imagem, "igreja", igreja.ImagemUrl);
                    await igrejaService.EditarAsync(igreja);
                }

                var response = new CriacaoIgrejaReponse() { ControleId = controle.Id };
                return Ok(new ApiResponse<dynamic>(new { response, messagemAplicacao = "Seguir para usuário para criar código validador!" }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
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
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
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
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        
        [HttpGet]
        [Route("buscar-por-filtro")]
        //[Authorize(Roles = "App,Admin")]
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
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        
        /*
               

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
                       var resultado = igrejaService.InformacoesGeraisResponse();
                       return Ok(new ApiResponse<dynamic>(resultado));
                   }
                   catch (Exception ex)
                   {
                       logger.LogError("{Ex}", ex);
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
                       var resultado = await igrejaService.BuscarPorIdAsync(igrejaId);
                       if(resultado == null) return NotFound();
                       request.IgrejaId = igrejaId;
                       var denuncia = await igrejaDenunciaService.BuscarPorIgrejaIdAsync(igrejaId);
                       var resultadoDenuncia = false;
                       if(denuncia is not null && !string.IsNullOrEmpty(denuncia.AcaoRealizada))
                       {
                           denuncia.AcaoRealizada = null;
                           denuncia.Descricao = request.Descricao;
                           denuncia.Titulo = request.Titulo;
                           denuncia.NomeDenunciador = request.NomeDenunciador;
                           denuncia.EmailDenunciador = request.EmailDenunciador;
                           resultadoDenuncia = await igrejaDenunciaService.AtualizarAsync(denuncia);
                       }else{
                           resultadoDenuncia = await igrejaDenunciaService.InserirAsync(request);
                       }

                       //return Ok(new ApiResponse<dynamic>(resultadoDenuncia));
                       return Ok(new ApiResponse<dynamic>(new { resultadoDenuncia, messagemAplicacao = "Aguarde a resposta do administrador!" }));
                   }
                   catch (Exception ex)
                   {
                       logger.LogError("{Ex}", ex);
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
                       var resultado = await enderecoService.BuscarDadosBuscaAsync(request);
                       return Ok(new ApiResponse<dynamic>(resultado));
                   }
                   catch (Exception ex)
                   {
                       logger.LogError("{Ex}", ex);
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
                       var resultado = await enderecoService.OrganizarEnderecosAsync();
                       return Ok(new ApiResponse<dynamic>(resultado));
                   }
                   catch (Exception ex)
                   {
                       logger.LogError("{Ex}", ex);
                       var response = new ApiResponse<dynamic>(ex.Message);
                       return StatusCode(StatusCodes.Status500InternalServerError, response);
                   }
               }
           */
    }
}

