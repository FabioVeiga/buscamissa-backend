using BuscaMissa.Models;
using BuscaMissa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContribuidorController(ContribuidoresService service) : ControllerBase
    {
        private readonly ContribuidoresService _service = service;

        [Authorize(Roles = "Admin")]
        [HttpPost("inserir-por-nomes")]
        public async Task<IActionResult> InserirPorNomes([FromBody] string nomes)
        {
            try
            {
                await _service.InserirPorNomesAsync(nomes);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "App,Admin")]
        [HttpGet("do-mes-vigente")]
        public async Task<IActionResult> ObterContribuidoresDoMesVigente()
        {
            try
            {
                var contribuidores = await _service.ObterContribuidoresDoMesVigenteAsync();
                return Ok(contribuidores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}