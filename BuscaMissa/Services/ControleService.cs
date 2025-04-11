using BuscaMissa.Context;
using BuscaMissa.Enums;
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

        public async Task EditarStatusAsync(StatusEnum status, int controleId)
        {
            try
            {
                var controle = await _context.Controles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == controleId) 
                    ?? throw new Exception("Controle not found");

                controle.Status = status;

                // Verifica se a entidade já está sendo rastreada e a desanexa
                var trackedEntity = _context.ChangeTracker.Entries<Controle>().FirstOrDefault(e => e.Entity.Id == controleId);
                if (trackedEntity != null)
                {
                    _context.Entry(trackedEntity.Entity).State = EntityState.Detached;
                }

                _context.Controles.Update(controle);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing Status Controle {Controle}", controleId);
                throw;
            }
        }

        public async Task<Controle?> BuscarPorIdAsync(int id)
        {
            try
            {
                var controle = await _context.Controles
                    .Include(x => x.Igreja)
                    .ThenInclude(x => x!.Missas)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (controle != null)
                {
                    _context.Entry(controle).State = EntityState.Detached; // Desanexa a entidade do contexto
                }

                return controle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Controle {Controle}", id);
                throw;
            }
        }

        public async Task<StatusEnum> BuscarPorStatusIdAsync(int id)
        {
            try
            {
                var model = await _context.Controles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
                return model?.Status ?? throw new Exception("Controle not found");
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