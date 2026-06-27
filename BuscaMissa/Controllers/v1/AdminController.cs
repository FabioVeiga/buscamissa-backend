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


namespace BuscaMissa.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AdminController(
        ILogger<AdminController> logger, UsuarioService usuarioService, IgrejaService igrejaService,
        ImagemService imagemService, RedeSociaisService redeSociaisService, ContatoService contatoService,
        IgrejaDenunciaService igrejaDenunciaService, EmailService emailService, SolicitacaoService solicitacaoService,
        ControleService controleService,
        ViaCepService viaCepService,
        IConfiguration configuration
        ) : ControllerBase
    {
        private readonly ControleService _controleService = controleService;
        private string FrontendBaseUrl => configuration["FrontendBaseUrl"] ?? "https://buscamissa.com.br";

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
                var response = new ApiResponse<dynamic>(ex.Message);
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
                if (!ModelState.IsValid) return BadRequest();

                var igreja = await igrejaService.InserirAsync(request);
                igreja.Ativo = true;

                if (!string.IsNullOrEmpty(request.Imagem))
                {
                    igreja.ImagemUrl = $"{igreja.Id}{ImageHelper.BuscarExtensao(request.Imagem)}";
                    var urlTemp2 = imagemService.UploadAzure(request.Imagem, "igreja", igreja.ImagemUrl);
                }

                igreja = await igrejaService.EditarAsync(igreja);

                await EnviarEmailAsync(igreja);

                var response = (IgrejaResponse)igreja;
                return Ok(new ApiResponse<dynamic>(new { response }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
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
                    await  EnviarEmailAsync(igreja, tipo);
                }

                var response = (IgrejaResponse)igreja;
                return Ok(new ApiResponse<dynamic>(new { response }));
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
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
                var response = new ApiResponse<dynamic>(ex.Message);
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
                var response = new ApiResponse<dynamic>(ex.Message);
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

        [HttpPut]
        [Route("igreja/denunciar/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DenunciarIgreja(int id, [FromBody] DenunciarIgrejaAdminRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var model = await igrejaDenunciaService.BuscarPorIdAsync(id);
                if (model == null) return NotFound(new ApiResponse<dynamic>("Denuncia não encontrada"));
                model.AcaoRealizada = request.Solucao;
                var response = await igrejaDenunciaService.SolucaoAsync(model);
                if (request.EnviarEmailDenunciador)
                {
                    var responseEmail = await emailService.EnviarEmail(
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
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
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
                    Console.WriteLine(@"Email enviado: {responseEmail}" ?? "Email não enviado!");
                }
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError("{Ex}", ex);
                var response = new ApiResponse<dynamic>(ex.Message);
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
                var response = new ApiResponse<dynamic>(ex.Message);
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
        
        private async Task EnviarEmailAsync(Igreja igreja, bool criacao = true)
        {
            try
            {
                var emailContato = igreja.Contato?.EmailContato;
                var html = string.Empty;
                if (string.IsNullOrWhiteSpace(emailContato))
                    return;

                var url = string.Concat(
                    FrontendBaseUrl,
                    "/paroquia/",
                    igreja.Endereco.Uf.ToLower(),
                    "/",
                    igreja.Endereco.CidadeSlug,
                    "/",
                    igreja.Slug
                );

                if (criacao)
                {
                    html = EmailHtmlGenerator.GerarHtmlEmailCriacao(
                        igreja.Nome,
                        igreja.Endereco.Logradouro,
                        igreja.Endereco.Numero,
                        igreja.Endereco.Bairro,
                        igreja.Endereco.Localidade,
                        igreja.Endereco.Estado,
                        igreja.Paroco,
                        url
                    );
                }
                else
                {
                    html = EmailHtmlGenerator.GerarHtmlEmailAlteracao(
                        igreja.Nome,
                        igreja.Endereco.Logradouro,
                        igreja.Endereco.Numero,
                        igreja.Endereco.Bairro,
                        igreja.Endereco.Localidade,
                        igreja.Endereco.Estado,
                        igreja.Paroco,
                        url
                    );
                }

                var responseEmail = await emailService.EnviarEmail(
                    [emailContato],
                    $"Sua Igreja {igreja.Nome} foi cadastrada no Busca Missa!",
                    html
                );

                if (string.IsNullOrWhiteSpace(responseEmail))
                {
                    logger.LogWarning(
                        "Igreja criada com sucesso, mas o e-mail não foi enviado. IgrejaId: {IgrejaId}, Email: {Email}",
                        igreja.Id,
                        emailContato
                    );
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Igreja criada com sucesso, mas ocorreu erro ao enviar e-mail. IgrejaId: {IgrejaId}",
                    igreja.Id
                );
            }
        }

    }
}