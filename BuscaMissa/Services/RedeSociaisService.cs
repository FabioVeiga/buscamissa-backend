using BuscaMissa.Context;
using BuscaMissa.Enums;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services
{
    public class RedeSociaisService(ApplicationDbContext context, ILogger<RedeSociaisService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<RedeSociaisService> _logger = logger;

        public async Task InserirAsync(RedeSocial model)
        {
            try
            {
                _context.RedesSociais.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing {Model}", model);
                throw;
            }
        }

        public async Task EditarAsync(RedeSocial model)
        {
            try
            {
                _context.RedesSociais.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing {Model}", model);
                throw;
            }
        }

        public async Task<RedeSocial?> ObterPorTipoEIgrejaIdAsync(TipoRedeSocialEnum tipoRedeSocial, int igrejaId)
        {
            try
            {
                return await _context.RedesSociais
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IgrejaId == igrejaId && x.TipoRedeSocial == tipoRedeSocial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching {Model}", igrejaId);
                throw;
            }
        }

        public async Task<IList<RedeSocial>?> ObterPorIgrejaId(int igrejaId)
        {
            try
            {
                return await _context.RedesSociais
                .AsNoTracking()
                .Where(x => x.IgrejaId == igrejaId)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching {Model}", igrejaId);
                throw;
            }
        }
    }
}