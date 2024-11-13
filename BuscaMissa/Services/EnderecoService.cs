using BuscaMissa.Context;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.Models;

namespace BuscaMissa.Services
{
    public class EnderecoService(ApplicationDbContext context, ILogger<EnderecoService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<EnderecoService> _logger = logger;

        public async Task<Endereco> InserirAsync(EnderecoIgrejaRequest request)
        {
            try
            {
                Endereco model = (Endereco)request;
                _context.Enderecos.Add(model);
                await _context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while insering Endereco {Endereco}", request);
                throw;
            }
        }
    }
}