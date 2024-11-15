using BuscaMissa.Context;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services
{
    public class ControleService(ApplicationDbContext context, ILogger<ControleService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<ControleService> _logger = logger;

        public async Task<Controle> InserirAsync(Controle model)
        {
            try
            {
                _context.Controles.Add(model);
                await _context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while insering COntrile {ccontrole}", model);
                throw;
            }
        }

        public async Task<Controle> EditarAsync(Controle model)
        {
            try
            {
                _context.Controles.Update(model);
                await _context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing Controle {Controle}", model);
                throw;
            }
        }

        public async Task<Controle?> BuscarPorIdAsync(int id)
        {
            try
            {
                return await _context.Controles
                .Include(x => x.Igreja)
                .ThenInclude(x => x.Missas)
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Controle {Controle}", id);
                throw;
            }
        }

        public async Task<Controle?> BuscarPorIgrejaIdAsync(int igrejaId)
        {
            try
            {
                return await _context.Controles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IgrejaId == igrejaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Controle {Controle}", igrejaId);
                throw;
            }
        }
    }
}