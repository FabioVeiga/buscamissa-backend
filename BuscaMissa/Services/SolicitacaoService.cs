using BuscaMissa.Context;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services
{
    public class SolicitacaoService(ApplicationDbContext context, ILogger<SolicitacaoService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<SolicitacaoService> _logger = logger;

        public async Task InserirAsync(Solicitacao model)
        {
            try
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing {Model}", model);
                throw;
            }
        }

        public async Task EditarAsync(Solicitacao model)
        {
            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing {Model}", model);
                throw;
            }
        }

        public async Task<Solicitacao?> BuscarPorId(int id)
        {
            try
            {
                return await _context.Solicitacoes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching {Id}", id);
                throw;
            }
        }
    }
}