using BuscaMissa.DTOs;
using BuscaMissa.DTOs.IgrejaDto;
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
        ILogger<AdminController> logger,
        UsuarioService usuarioService,
        IgrejaService igrejaService,
        ImagemService imagemService,
        ViaCepService viaCepService,
        ContatoService contatoService
        ) : ControllerBase
    {
        private readonly ILogger<AdminController> _logger = logger;
        private readonly UsuarioService _usuarioService = usuarioService;
        private readonly IgrejaService _igrejaService = igrejaService;
        private readonly ImagemService _imagemService = imagemService;
        private readonly ViaCepService _viaCepService = viaCepService;
        private readonly ContatoService _contatoService = contatoService;


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
                if(temUsuario is not null) return BadRequest(new ApiResponse<dynamic>(new { mensagemTela = "Email já cadastrado!" }));

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

                if(!ModelState.IsValid) return BadRequest();

                var igreja = await _igrejaService.InserirAsync(request);
                igreja.Ativo = true;
                igreja = await _igrejaService.EditarAsync(igreja);
                
                if (!string.IsNullOrEmpty(request.Imagem)){
                    igreja.ImagemUrl= $"{igreja.Id}{ImageHelper.BuscarExtensao(request.Imagem)}";
                    var urlTemp2 = _imagemService.UploadAzure(request.Imagem, "igreja", igreja.ImagemUrl);
                    await _igrejaService.EditarAsync(igreja);
                }
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
        public async Task<IActionResult> Atualizar([FromBody] AtualicaoIgrejaRequest request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                var igreja = await _igrejaService.BuscarPorIdAsync(request.Id);
                if (igreja is null) return NotFound(new ApiResponse<dynamic>(new { messagemAplicacao = "Igreja não encontrada!" }));

                await _igrejaService.EditarAsync(igreja, request);

                if(request.Contato is not null)
                {
                    var contato = (Contato)request.Contato;
                    contato.IgrejaId = igreja.Id;
                    if(igreja.Contato is not null){
                        contato.Id = igreja.Contato!.Id;
                        await _contatoService.EditarAsync(contato); 
                    }
                    else
                        await _contatoService.InserirAsync(contato);
                }

                if (!string.IsNullOrEmpty(request.Imagem)){
                    igreja.ImagemUrl= $"{igreja.Id}{ImageHelper.BuscarExtensao(request.Imagem)}";
                    var urlTemp = _imagemService.UploadAzure(request.Imagem, "igreja", igreja.ImagemUrl);
                    await _igrejaService.EditarAsync(igreja);
                }
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
        
        #endregion
    }
}