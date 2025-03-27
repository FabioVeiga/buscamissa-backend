using BuscaMissa.Constants;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.SolicitacaoDto;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController(
        ILogger<AdminController> logger, UsuarioService usuarioService, IgrejaService igrejaService,
        ImagemService imagemService, RedeSociaisService redeSociaisService, ContatoService contatoService,
        IgrejaDenunciaService igrejaDenunciaService, EmailService emailService, SolicitacaoService solicitacaoService
        ) : ControllerBase
    {
        private readonly ILogger<AdminController> _logger = logger;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly IgrejaService _igrejaService = igrejaService;
        private readonly ImagemService _imagemService = imagemService;
        private readonly RedeSociaisService _redeSociaisService = redeSociaisService;
        private readonly ContatoService _contatoService = contatoService;
        private readonly IgrejaDenunciaService _igrejaDenunciaService = igrejaDenunciaService;
        private readonly EmailService _emailService = emailService;
        private readonly SolicitacaoService _solicitacaoService = solicitacaoService;

        #region Usuario
        [HttpGet]
        [Route("usuario/{codigo}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BuscarPorCodigoAsync(int codigo)
        {
            try
            {

                var usuario = await _usuarioService.BuscarPorCodigo(codigo);
                if (usuario == null)
                    return NotFound();
                UsuarioResponse usuarioResponse = (UsuarioResponse)usuario;
                return Ok(new ApiResponse<dynamic>(new
                {
                    usuario = usuarioResponse
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
        [Route("usuario/buscar-por-filtro")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BuscarPorCodigoAsync([FromQuery] UsuarioFiltroRequest filtro)
        {
            try
            {

                var usuarios = await _usuarioService.BuscarPorFiltroAsync(filtro);
                if (usuarios == null)
                    return NotFound();
                return Ok(new ApiResponse<dynamic>(new
                {
                    usuarios
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("usuario/criar")]
        public async Task<IActionResult> Inserir([FromBody] CriacaoUsuarioRequest request)
        {
            try
            {
                if (!ModelState.IsValid) BadRequest();
                var temUsuario = await _usuarioService.BuscarPorEmailAsync(request.Email);
                if (temUsuario is not null) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Email já cadastrado!" }));

                var usuarioCriado = await _usuarioService.InserirAsync(request);
                return Ok(new ApiResponse<dynamic>(new
                {
                    usuario = usuarioCriado
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPut]
        [Route("igreja/bloquear-desbloquear/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> BloquearDesbloquearAsync(int id, [FromBody] UsuarioBloqueadoRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var model = await _usuarioService.BuscarPorCodigo(id);
                if (model == null) return NotFound(new ApiResponse<dynamic>("Usuário não encontrado"));
                model.Bloqueado = request.Bloqueado;
                model.MotivoBloqueio = request.MotivoBloqueio;
                var resultado = await _usuarioService.EditarAsync(model);
                return Ok(new ApiResponse<dynamic>(new { resultado }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        #endregion

        #region Igreja
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("igreja/criar")]
        public async Task<IActionResult> CriarIgreja([FromBody] CriacaoIgrejaRequest request)
        {
            try
            {
                request.Endereco.Cep = CepHelper.FormatarCep(request.Endereco.Cep);
                var igrejaResponse = await _igrejaService.BuscarPorCepAsync(request.Endereco.Cep);
                if (igrejaResponse is not null) return BadRequest(new ApiResponse<dynamic>(new { igrejaResponse, messagemAplicacao = "Igreja já cadastrada!" }));

                if (!ModelState.IsValid) return BadRequest();

                var igreja = await _igrejaService.InserirAsync(request);
                igreja.Ativo = true;

                if (!string.IsNullOrEmpty(request.Imagem))
                {
                    igreja.ImagemUrl = $"{igreja.Id}{ImageHelper.BuscarExtensao(request.Imagem)}";
                    var urlTemp2 = _imagemService.UploadAzure(request.Imagem, "igreja", igreja.ImagemUrl);
                }

                igreja = await _igrejaService.EditarAsync(igreja);
                var response = (IgrejaResponse)igreja;
                return Ok(new ApiResponse<dynamic>(new { response }));
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("igreja/atualizar")]
        public async Task<IActionResult> Atualizar([FromBody] AtualicaoIgrejaAdminRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var igreja = await _igrejaService.BuscarPorIdAsync(request.Id);
                if (igreja is null) return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Igreja não encontrada!" }));

                if (request.Contato is not null)
                {
                    var contato = (Contato)request.Contato;
                    contato.IgrejaId = igreja.Id;
                    if (igreja.Contato is not null)
                        contato.Id = igreja.Contato!.Id;
                    else
                        await _contatoService.InserirAsync(contato);
                }

                if (!string.IsNullOrEmpty(request.Imagem))
                {
                    igreja.ImagemUrl = $"{igreja.Id}{ImageHelper.BuscarExtensao(request.Imagem)}";
                    var urlTemp = _imagemService.UploadAzure(request.Imagem, "igreja", igreja.ImagemUrl);
                }

                if (request.RedeSociais is not null)
                {
                    foreach (var item in request.RedeSociais)
                    {
                        var temRede = igreja.RedesSociais.FirstOrDefault(x => x.TipoRedeSocial == item.TipoRedeSocial);
                        if (temRede is not null)
                        {
                            temRede.NomeDoPerfil = item.NomeDoPerfil;
                        }
                        else
                        {
                            var rede = (RedeSocial)item;
                            rede.IgrejaId = igreja.Id;
                            await _redeSociaisService.InserirAsync(rede);
                        }
                    }
                }


                if (request.Contato is not null)
                {
                    var contato = await _contatoService.ObterIgrejaIdAsync(igreja.Id);
                    if (contato is not null)
                    {
                        contato.DDD = request.Contato.DDD;
                        contato.Telefone = request.Contato.Telefone;
                        contato.DDDWhatsApp = request.Contato.DDDWhatsApp;
                        contato.TelefoneWhatsApp = request.Contato.TelefoneWhatsApp;
                        contato.EmailContato = request.Contato.EmailContato;
                    }
                    else
                    {
                        contato = (Contato)request.Contato;
                        contato.IgrejaId = igreja.Id;
                    }
                }

                await _igrejaService.EditarAsync(igreja, request);

                var response = (IgrejaResponse)igreja;
                return Ok(new ApiResponse<dynamic>(new { response }));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("igreja/infos")]
        [Authorize(Roles = "Admin")]
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

        [HttpPut]
        [Route("igreja/denunciar/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DenunciarIgreja(int id, [FromBody] DenunciarIgrejaAdminRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var model = await _igrejaDenunciaService.BuscarPorIdAsync(id);
                if (model == null) return NotFound(new ApiResponse<dynamic>("Denuncia não encontrada"));
                model.AcaoRealizada = request.Solucao;
                var response = await _igrejaDenunciaService.SolucaoAsync(model);
                if (request.EnviarEmailDenunciador)
                {
                    var responseEmail = await _emailService.EnviarEmail(
                        [model.EmailDenunciador],
                        $"Resposta da denuncia - {model.Igreja.Nome}",
                        Contant.EmailDenuncia
                        .Replace("{nomeDenunciador}", model.NomeDenunciador)
                        .Replace("{denuncia}", model.Descricao)
                        .Replace("{solução}", request.Solucao)
                        .Replace("{ano}", DataHoraHelper.Ano())
                        );
                    Console.WriteLine(@"Email enviado: {responseEmail}" ?? "Email não enviado!");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        #endregion

        #region Solicitacao
        [HttpPost]
        [Route("solicitacao/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SolicitacaoAdmin(int id, [FromBody] SolicitacaoAdminRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var model = await _solicitacaoService.BuscarPorId(id);
                if (model == null) return NotFound(new ApiResponse<dynamic>("Solicitação não encontrada"));
                model.Resposta = request.Resposta;
                model.Solucao = request.Solucao;
                model.Resolvido = request.Resolvido;
                model.EnviarResposta = request.EnviarResposta;
                await _solicitacaoService.EditarAsync(model);
                if (request.EnviarResposta)
                {
                    var responseEmail = await _emailService.EnviarEmail(
                        [model.EmailSolicitante],
                        $"Resposta da  solicitação - {model.Tipo}",
                        Contant.EmailSolicitacaoResposta
                        .Replace("{nomeUsuario}", model.NomeSolicitante)
                        .Replace("{numeroSolicitacao}", model.Numero)
                        .Replace("{assuntoSolicitacao}", model.Assunto)
                        .Replace("{mensagemSolicitacao}", model.Mensagem)
                        .Replace("{respostaSolicitacao}", model.Resposta)
                        .Replace("{ano}", DataHoraHelper.Ano())
                        );
                    Console.WriteLine(@"Email enviado: {responseEmail}" ?? "Email não enviado!");
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        #endregion

    }
}