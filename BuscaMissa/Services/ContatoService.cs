using BuscaMissa.Context;
using BuscaMissa.Models;

namespace BuscaMissa.Services
{
    public class ContatoService(ApplicationDbContext context, ILogger<ContatoService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<ContatoService> _logger = logger;

        public async Task InserirAsync(Contato model)
        {
            try
            {
                _context.Contatos.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing CodigoPermissao {CodigoPermissao}", model);
                throw;
            }
        }

        public async Task EditarAsync(Contato model)
        {
            try
            {
                _context.Contatos.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing CodigoPermissao {CodigoPermissao}", model);
                throw;
            }
        }
    }
}