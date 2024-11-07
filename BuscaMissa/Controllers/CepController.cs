using BuscaMissa.Context;
using BuscaMissa.DTOs;
using BuscaMissa.Helpers;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CepController : ControllerBase
    {
        private readonly ILogger<CepController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ViaCepService _viaCepService;

        public CepController(ViaCepService viaCepService,
        ApplicationDbContext context,
        ILogger<CepController> logger)
        {
            _context = context;
            _logger = logger;
            _viaCepService = viaCepService;
        }

        [HttpGet("{cep}")]
        public async Task<IActionResult> GetEndereco(string cep)
        {
            try
            {
                var cepFormatado = CepHelper.FormatarCep(cep);
                var temIgrejaComEsteCep = await _context.Enderecos.AnyAsync(x => x.Cep == cepFormatado);
                if (temIgrejaComEsteCep)
                {
                    var igreja = await _context.Enderecos.Where(x => x.Cep == cepFormatado).FirstOrDefaultAsync();
                    return Ok("Sucesso ao buscar o endereço"); //montar URL do GET
                }
                var endereco = await _viaCepService.ConsultarCepAsync(cepFormatado.ToString());

                if (endereco == null)
                {
                    return NotFound(new { Message = "CEP não encontrado" });
                }

                return Ok(new ApiResponse<dynamic>(
                        endereco
                    ));
            }
            catch (Exception ex)
            {
                _logger.LogError("{Ex}",ex);
                var response = new ApiResponse<dynamic>(
                    ex.Message
                    );
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            
        }
    }
}