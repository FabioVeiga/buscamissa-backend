using BuscaMissa.Context;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Contato?> ObterIgrejaIdAsync(int igrejaId)
        {
            try
            {
                return await _context.Contatos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IgrejaId == igrejaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching {Model}", igrejaId);
                throw;
            }
        }
    }
}