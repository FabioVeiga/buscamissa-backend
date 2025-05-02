using Microsoft.AspNetCore.Mvc;
#if DEBUG
namespace BuscaMissa.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestesController(IConfiguration configuration) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration;

        [HttpGet("obter-secret")]
        public IActionResult Test([FromQuery] string nomeSecret, [FromHeader] string senha)
        {
            if (string.IsNullOrEmpty(nomeSecret))
                return BadRequest("Nome do segredo não pode ser nulo ou vazio.");
            if (!Autenticar(senha))
                return Unauthorized("Senha informada não é válida.");
            var secret = _configuration[nomeSecret];
            return Ok(secret);
        }


        private bool Autenticar(string senhaInformada)
        {
            var senha = _configuration["SenhaFixaParaTeste"];
            if (senhaInformada == senha)
                return true;
            return false;
        }

    }
}
#endif