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

        // [HttpPost]
        // public async Task<IActionResult> InserirIgreja([FromBody] IgrejaRequest request)
        // {
        //     try
        //     {
        //         //cadastrando usuario se nao existe
        //         var usuario = await _usuarioService.BuscarPorEmailAsync(request.EmailUsuario);
        //         usuario ??= await _usuarioService.InserirAsync(request);

        //         //verifica se existe CEP para esta igreja 
        //         var igreja = await _igrejaService.BuscarPorCepAsync(request.Endereco.Cep);
        //         if (igreja is not null)
        //         {
        //             //TODO: verificar na tabela de controle qual status e devolver conforme o status
        //             var controle = await _controleService.BuscarPorIgrejaIdAsync(igreja.Id);
        //             return BadRequest(new ApiResponse<dynamic>(new { mensagem = $"Necessita de aguardar a confirmação do código validador!" }));
        //         }
        //         //cadastrando igreja, endereco, controle, codigo
        //         igreja = await _igrejaService.InserirAsync(request, usuario);
        //         var controleNovo = await _controleService.InserirAsync(igreja);
        //         var codigo = await _codigoValidacaoService.InserirAsync(controleNovo);
        //         var enviarEmail = await EnviarEmailComCodigoToken(codigo, usuario);
        //         var mensagem = "Código de validação gerado";
        //             if(!string.IsNullOrWhiteSpace(enviarEmail))
        //             {
        //                 mensagem += " e enviado no email!";
        //                 controleNovo.Status = Enums.StatusEnum.Aguardando_Confirmacao;
        //                 controleNovo = await _controleService.EditarAsync(controleNovo);
        //             }

        //         //enviando imagem para azure, se existir
        //         if(!string.IsNullOrEmpty(request.Imagem))
        //         {
        //             var url = await _azureBlobStorageService.UploadImagemAsync(request.Imagem, "igreja", igreja.Id.ToString());
        //             igreja.ImagemUrl = url;
        //             igreja = await _igrejaService.EditarAsync(igreja);
        //         }

        //         return Ok(new ApiResponse<dynamic>(new { mensagem }));
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError("{Ex}", ex);
        //         var response = new ApiResponse<dynamic>(ex.Message);
        //         return StatusCode(StatusCodes.Status500InternalServerError, response);
        //     }
        // }
    
        // [HttpPost("validar-codigo")]
        // public async Task<IActionResult> ValidarCriacaoIgreja([FromBody] ValidarCriacaoIgrejaRequest request)
        // {
        //     try
        //     {
        //         var igreja = await _igrejaService.BuscarPorIdAsync(request.IgrejaId);
        //         if (igreja is null)
        //         {
        //             return NotFound(new ApiResponse<dynamic>(new { mensagem = "Igreja não encontrada!" }));
        //         }
        //         //cadastrando usuario se nao existe
        //         var usuario = await _usuarioService.BuscarPorEmailAsync(request.Email);
        //         if(usuario is null)
        //         {
        //             usuario = await _usuarioService.InserirAsync(request);
        //             igreja.UsuarioId = usuario.Id;
        //             igreja.Usuario = usuario;
        //             await _igrejaService.EditarAsync(igreja);
        //             var controleAux = await _controleService.BuscarPorIgrejaIdAsync(request.IgrejaId);
        //             var codigoAux = await _codigoValidacaoService.BuscarPorControleIdAsync(controleAux.Id);
        //             var validadorAux = _codigoValidacaoService.Validar(codigoAux,request,usuario);
        //             if(validadorAux.Contains("Enviado email"))
        //             {
        //                 codigoAux = await _codigoValidacaoService.EditarAsync(codigoAux);
        //             }
        //             await EnviarEmailComCodigoToken(codigoAux, usuario);
        //             return BadRequest(new ApiResponse<dynamic>(new { mensagem = "Enviamos um código para validar!" }));
        //         }

        //         var codigo = await _codigoValidacaoService.BuscarPorCodigoTokenAsync(request.CodigoToken);
        //         if (codigo is null)
        //         {
        //             return NotFound(new ApiResponse<dynamic>(new { mensagem = "Código de validação não encontrado!" }));
        //         };

        //         codigo.Controle = await _controleService.BuscarPorIgrejaIdAsync(request.IgrejaId);
        //         var validar = _codigoValidacaoService.Validar(codigo,request,usuario); 
        //         if(codigo.Controle is not null)
        //         {
        //             if(!string.IsNullOrEmpty(validar))
        //             {
        //                 //se o código estiver expirado
        //                 if(validar.Contains("Enviado email"))
        //                 {
        //                     codigo = await _codigoValidacaoService.EditarAsync(codigo);
        //                     await EnviarEmailComCodigoToken(codigo, usuario);
        //                 }
        //                 return BadRequest(new ApiResponse<dynamic>(new { mensagem = validar }));
        //             }
        //             if(codigo.Controle.Status == Enums.StatusEnum.Aguardando_Confirmacao)
        //             {
        //                 codigo.Controle.Status = Enums.StatusEnum.Ativado;
        //                 await _controleService.EditarAsync(codigo.Controle);
        //                 igreja.Ativo = true;
        //                 igreja.Alteracao = DateTime.Now;
        //                 igreja.UsuarioId = usuario.Id;
        //                 igreja.Usuario = usuario;
        //                 await _igrejaService.EditarAsync(igreja);
        //                 return Ok(new ApiResponse<dynamic>(new { mensagem = "Igreja ativada com sucesso!"}));  
        //             }
        //         }
        //         return BadRequest(new ApiResponse<dynamic>(new { mensagem = "Não atualiações para esta transação!"}));
                
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError("{Ex}", ex);
        //         var response = new ApiResponse<dynamic>(ex.Message);
        //         return StatusCode(StatusCodes.Status500InternalServerError, response);
        //     }
        // }

        private async Task<string?> EnviarEmailComCodigoToken(CodigoPermissao codigo, Usuario usuario)
        {
            var dic = EmailService.DicionarioParaEnvioDoCodigo(usuario.Nome,codigo.CodigoToken, codigo.ValidoAte);
            #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var enviarEmail = await _emailService.EnviarCodigoValidacao([usuario.Email], "BuscaMissa - Código para validar", null, dic);
            #pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return enviarEmail;
        }
    }
}

