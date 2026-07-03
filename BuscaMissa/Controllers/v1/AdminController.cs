using Asp.Versioning;
using BuscaMissa.Constants;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.SolicitacaoDto;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.DTOs.v1.EmailHtmlGenerator;
using BuscaMissa.DTOs.v1.EnderecoDto;
using BuscaMissa.DTOs.v1.IgrejaDto;
using BuscaMissa.Enums;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using BuscaMissa.Services.v1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BuscaMissa.DTOs.v1.EmailEventoIgrejaDto;
using BuscaMissa.DTOs.v1.DivulgacaoDto;

namespace BuscaMissa.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AdminController(
        ILogger<AdminController> logger, UsuarioService usuarioService, IgrejaService igrejaService,
        ImagemService imagemService, RedeSociaisService redeSociaisService, ContatoService contatoService,
        IgrejaReportarProblemaService igrejaReportarProblemaService, EmailService emailService, SolicitacaoService solicitacaoService,
        ControleService controleService,
        ViaCepService viaCepService,
        IConfiguration configuration,
        EmailEventoIgrejaService emailEventoIgrejaService,
        DivulgacaoService divulgacaoService,
        BuscaMissa.Services.ServicoConsultaMetricas servicoConsultaMetricas
        ) : ControllerBase
    {
        private readonly ControleService _controleService = controleService;
        private string FrontendBaseUrl => configuration["FrontendBaseUrl"] ?? BuscaMissa.Constants.Constants.FrontendBaseUrlDefault;

        #region Usuario
        [HttpGet]
        [Route("usuario/{codigo}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BuscarPorCodigoAsync(int codigo)
        {
            try
            {

                var usuario = await usuarioService.BuscarPorCodigo(codigo);
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
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
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

                var usuarios = await usuarioService.BuscarPorFiltroAsync(filtro);
                if (usuarios == null)
                    return NotFound();
                return Ok(new ApiResponse<dynamic>(new
                {
                    usuarios
                }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
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
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var temUsuario = await usuarioService.BuscarPorEmailAsync(request.Email);
                if (temUsuario is not null) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Email já cadastrado!" }));

                var usuarioCriado = await usuarioService.InserirAsync(request);
                return Ok(new ApiResponse<dynamic>(new
                {
                    usuario = usuarioCriado
                }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPut]
        [Route("usuario/bloquear-desbloquear/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> BloquearDesbloquearAsync(int id, [FromBody] UsuarioBloqueadoRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var model = await usuarioService.BuscarPorCodigo(id);
                if (model == null) return NotFound(new ApiResponse<dynamic>("Usuário não encontrado"));
                model.Bloqueado = request.Bloqueado;
                model.MotivoBloqueio = request.MotivoBloqueio;
                var resultado = await usuarioService.EditarAsync(model);
                return Ok(new ApiResponse<dynamic>(new { resultado }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
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
                if (!ModelState.IsValid) return BadRequest();

                var igreja = await igrejaService.InserirAsync(request);
                igreja.Ativo = true;

                if (!string.IsNullOrEmpty(request.Imagem))
                {
                    igreja.ImagemUrl = $"{igreja.Id}{ImageHelper.BuscarExtensao(request.Imagem)}";
                    var urlTemp2 = imagemService.UploadAzure(request.Imagem, "igreja", igreja.ImagemUrl);
                }

                igreja = await igrejaService.EditarAsync(igreja);

                await divulgacaoService.EnviarEmailAsync(igreja, true);

                var response = (IgrejaResponse)igreja;
                response.EmailCriacaoEnviado = await emailEventoIgrejaService.EmailCriacaoJaEnviadoAsync(igreja.Id);

                return Ok(new ApiResponse<dynamic>(new { response }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
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
                var igreja = await igrejaService.BuscarPorIdAsync(request.Id);
                if (igreja is null) return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Igreja não encontrada!" }));

                if (!string.IsNullOrEmpty(request.Imagem))
                {
                    igreja.ImagemUrl = $"{igreja.Id}{ImageHelper.BuscarExtensao(request.Imagem)}";
                    imagemService.UploadAzure(request.Imagem, "igreja", igreja.ImagemUrl);
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
                            await redeSociaisService.InserirAsync(rede);
                        }
                    }
                }


                if (request.Contato is not null)
                {
                    //var contato = await _contatoService.ObterIgrejaIdAsync(igreja.Id);
                    if (igreja.Contato is not null)
                    {
                        igreja.Contato.DDD = request.Contato.DDD;
                        igreja.Contato.Telefone = request.Contato.Telefone;
                        igreja.Contato.DDDWhatsApp = request.Contato.DDDWhatsApp;
                        igreja.Contato.TelefoneWhatsApp = request.Contato.TelefoneWhatsApp;
                        igreja.Contato.EmailContato = request.Contato.EmailContato;
                        igreja.Contato.Website = request.Contato.Website;
                    }
                    else
                    {
                        var contato = (Contato)request.Contato;
                        contato.IgrejaId = igreja.Id;
                        await contatoService.InserirAsync(contato);
                    }
                }

                await igrejaService.EditarAsync(igreja, request);
                
                if (!string.IsNullOrWhiteSpace(request.TipoEmailContato) &&
                    !string.IsNullOrWhiteSpace(request.Contato?.EmailContato))
                {
                    var tipo = request.TipoEmailContato.Contains("criacao") ? true : false;
                    await divulgacaoService.EnviarEmailAsync(igreja, tipo);
                }

                var response = (IgrejaResponse)igreja;
                response.EmailCriacaoEnviado = await emailEventoIgrejaService.EmailCriacaoJaEnviadoAsync(igreja.Id);

                return Ok(new ApiResponse<dynamic>(new { response }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        
        // ─── Rota: POST /admin/igreja/endereco/reverso ────────────────────
        [HttpPost("igreja/endereco/reverso")]
        [ProducesResponseType(typeof(IEnumerable<EnderecoViaCepResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuscarCepPorEndereco(
            [FromBody] EnderecoReversoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Uf)         ||
                string.IsNullOrWhiteSpace(request.Cidade)     ||
                string.IsNullOrWhiteSpace(request.Logradouro))
            {
                return BadRequest(new { mensagem = "UF, Cidade e Logradouro são obrigatórios." });
            }

            logger.LogInformation(
                "Requisição de CEP reverso — UF: {Uf}, Cidade: {Cidade}, Logradouro: {Logradouro}",
                request.Uf, request.Cidade, request.Logradouro);

            var resultados = await viaCepService.ConsultarCepPorEnderecoAsync(
                request.Uf,
                request.Cidade,
                request.Logradouro,
                request.Bairro);

            if (!resultados.Any())
                return NotFound(new { mensagem = "Nenhum CEP encontrado para o endereço informado." });

            return Ok(new ApiResponse<dynamic>(new
            {
                candidatos = resultados
            }));
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route("igreja/ativar/{igrejaId}/usuario/{usuarioId}")]
        public async Task<IActionResult> AtivarIgreja(int igrejaId, int usuarioId)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var usuario = await usuarioService.BuscarPorCodigo(usuarioId);
                if (usuario is null) return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Usuário não encontrado!" }));

                await igrejaService.AtivarAsync(igrejaId, usuario.Id);
                return Ok(new ApiResponse<dynamic>(new { mensagemAplicacao = "Igreja ativada com sucesso!" }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("igreja/deletar/{id}")]
        public async Task<IActionResult> DeletarIgreja(int id)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();

                var resultado = await igrejaService.DeletarAsync(id);

                if (!resultado)
                    return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Igreja não encontrada!" }));

                return Ok(new ApiResponse<dynamic>(new { mensagemAplicacao = "Igreja deletada com sucesso!" }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("igreja/deletar/redesocial/{igrejaId}/{tipoRedeSocial}")]
        public async Task<IActionResult> DeletarRedeSocial(TipoRedeSocialEnum tipoRedeSocial, int igrejaId)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                await redeSociaisService.DeletarAsync(igrejaId, tipoRedeSocial);
                return Ok(new ApiResponse<dynamic>(new { mensagemAplicacao = "Rede social deletada com sucesso!" }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
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

        [HttpGet]
        [Route("igreja/{id}/metricas")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ObterMetricasIgreja(int id)
        {
            try
            {
                var metricas = await servicoConsultaMetricas.ObterMetricasUltimos30DiasAsync(TipoEntidadeMetricaEnum.Igreja, id);
                return Ok(new ApiResponse<dynamic>(metricas));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpGet]
        [Route("metricas/dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ObterDashboardMetricas()
        {
            try
            {
                var dashboard = await servicoConsultaMetricas.ObterDashboardAsync();
                return Ok(new ApiResponse<dynamic>(dashboard));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPut]
        [Route("igreja/reportar-problema/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ResolverProblemaReportado(int id, [FromBody] ReportarProblemaAdminRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var model = await igrejaReportarProblemaService.BuscarPorIdAsync(id);
                if (model == null) return NotFound(new ApiResponse<dynamic>("Problema reportado não encontrado"));
                model.AcaoRealizada = request.Solucao;
                var response = await igrejaReportarProblemaService.SolucaoAsync(model);
                if (request.EnviarEmail)
                {
                    var responseEmail = await emailService.EnviarEmail(
                        [model.Email],
                        $"Resposta ao problema reportado - {model.Igreja.Nome}",
                        Contant.EmailReportarProblema
                        .Replace("{nome}", model.Nome)
                        .Replace("{descricao}", model.Descricao)
                        .Replace("{solução}", request.Solucao)
                        .Replace("{ano}", DataHoraHelper.Ano())
                        );
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }


        [HttpGet]
        [Route("igreja/buscar-por-filtro")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> BuscarPorFiltro([FromQuery] FiltroIgrejaAdminRequest filtro)
        {
            try
            {
                var resultado = await igrejaService.BuscarPorFiltrosAsync(filtro);
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
                var model = await solicitacaoService.BuscarPorId(id);
                if (model == null) return NotFound(new ApiResponse<dynamic>("Solicitação não encontrada"));
                model.Resposta = request.Resposta;
                model.Solucao = request.Solucao;
                model.Resolvido = request.Resolvido;
                model.EnviarResposta = request.EnviarResposta;
                await solicitacaoService.EditarAsync(model);
                if (request.EnviarResposta)
                {
                    var responseEmail = await emailService.EnviarEmail(
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
                }
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        
        [HttpGet]
        [Route("solicitacao")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BuscarSolicitacao([FromQuery]bool? resolvida)
        {
            try
            {
                var resultado = await solicitacaoService.BuscarTodosAsync(resolvida);
                if (resultado.Count() == 0) return NotFound();
                return Ok(new ApiResponse<dynamic>(resultado));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }
        
        #endregion
        
        #region Email Eventos Igreja

        [HttpGet]
        [Route("email-evento/igreja/{igrejaId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BuscarEmailEventosPorIgrejaId(int igrejaId)
        {
            try
            {
                var resultado = await emailEventoIgrejaService.BuscarPorIgrejaIdAsync(igrejaId);

                if (resultado.Count == 0)
                    return NotFound(new ApiResponse<dynamic>(new { mensagemAplicacao = "Nenhum evento de e-mail encontrado para esta igreja." }));

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
        [Route("email-evento/buscar-por-filtro")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BuscarEmailEventosPorFiltro([FromQuery] FiltroEmailEventoIgrejaRequest filtro)
        {
            try
            {
                var resultado = await emailEventoIgrejaService.BuscarPorFiltroAsync(filtro);

                if (resultado.TotalItems == 0)
                    return NotFound(new ApiResponse<dynamic>(new { mensagemAplicacao = "Nenhum evento de e-mail encontrado." }));

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
        [Route("email-evento/criar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CriarEmailEvento([FromBody] CriarEmailEventoIgrejaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var igreja = await igrejaService.BuscarPorIdAsync(request.IgrejaId);
                if (igreja is null)
                    return NotFound(new ApiResponse<dynamic>(new { mensagemAplicacao = "Igreja não encontrada." }));

                var model = await emailEventoIgrejaService.InserirAsync(request);
                var response = (EmailEventoIgrejaResponse)model;

                return Ok(new ApiResponse<dynamic>(new { response }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost]
        [Route("email-evento/registrar-contato")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegistrarContato([FromBody] RegistrarContatoRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var igreja = await igrejaService.BuscarPorIdAsync(request.IgrejaId);
                if (igreja is null)
                    return NotFound(new ApiResponse<dynamic>(new { mensagemAplicacao = "Igreja não encontrada." }));

                var model = await emailEventoIgrejaService.RegistrarContatoAsync(request);
                var response = (EmailEventoIgrejaResponse)model;

                return Ok(new ApiResponse<dynamic>(new { response }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPut]
        [Route("email-evento/atualizar/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtualizarEmailEvento(int id, [FromBody] AtualizarEmailEventoIgrejaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (id != request.Id)
                    return BadRequest(new ApiResponse<dynamic>(new { mensagemAplicacao = "Id da rota diferente do Id do corpo da requisição." }));

                var model = await emailEventoIgrejaService.BuscarPorIdAsync(id);
                if (model is null)
                    return NotFound(new ApiResponse<dynamic>(new { mensagemAplicacao = "Evento de e-mail não encontrado." }));

                var igreja = await igrejaService.BuscarPorIdAsync(request.IgrejaId);
                if (igreja is null)
                    return NotFound(new ApiResponse<dynamic>(new { mensagemAplicacao = "Igreja não encontrada." }));

                model = await emailEventoIgrejaService.EditarAsync(model, request);
                var response = (EmailEventoIgrejaResponse)model;

                return Ok(new ApiResponse<dynamic>(new { response }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(BuscaMissa.Constants.Constants.MensagemErroInterno);
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        #endregion

        #region Importação em lote

        [HttpPost("igrejas/lote")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportarLote([FromBody] ImportacaoIgrejaLoteRequest request)
        {
            try
            {
                if (request.Igrejas == null || request.Igrejas.Count == 0)
                    return BadRequest("Nenhuma igreja informada.");

                var resultado = await igrejaService.ImportarLoteAsync(request);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        #region Divulgacao

        [HttpGet]
        [Route("divulgacao/dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DivulgacaoDashboard()
        {
            try
            {
                var response = await divulgacaoService.ObterDashboardAsync();
                return Ok(new ApiResponse<dynamic>(response));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route("divulgacao/igrejas")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DivulgacaoIgrejas([FromQuery] FiltroIgrejaDivulgacaoRequest filtro)
        {
            try
            {
                var response = await divulgacaoService.BuscarIgrejasAsync(filtro);
                return Ok(new ApiResponse<dynamic>(response));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("divulgacao/enviar-email-lote")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DivulgacaoEnviarEmailLote([FromBody] EnviarEmailLoteRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var response = await divulgacaoService.EnviarEmailLoteAsync(request);
                return Ok(new ApiResponse<dynamic>(response));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion
    }
}