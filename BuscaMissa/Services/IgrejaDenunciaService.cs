using BuscaMissa.Context;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services
{
    public class IgrejaDenunciaService(ApplicationDbContext context, ILogger<IgrejaDenunciaService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<IgrejaDenunciaService> _logger = logger;

        public async Task<IgrejaDenuncia?> BuscarPorIgrejaIdAsync(int igrejaId)
        {
            try
            {
               return await _context.IgrejaDenuncias
               .Include(id => id.Igreja)
               .AsNoTracking()
               .FirstOrDefaultAsync(x => x.IgrejaId == igrejaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Igreja");
                throw;
            }
        }

        public async Task<IgrejaDenuncia?> BuscarPorIdAsync(int id)
        {
            try
            {
               return await _context.IgrejaDenuncias
               .Include(id => id.Igreja)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Igreja");
                throw;
            }
        }

        public async Task<bool> TemDenunciaPorIgrejaIdAsync(int igrejaId)
        {
            try
            {
                return await _context.IgrejaDenuncias
                .AsNoTracking()
                .AnyAsync(x => x.IgrejaId == igrejaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Igreja");
                throw;
            }
        }
        

        public async Task<bool> InserirAsync(DenunciarIgrejaRequest request)
        {
            try{
                var model = (IgrejaDenuncia)request;
                _context.Add(model);
                var resultado = await _context.SaveChangesAsync();
                return resultado > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inserir {request}", request);
                return false;
            }
        }

        public async Task<bool> SolucaoAsync(IgrejaDenuncia igrejaDenuncia)
        {
            try
            {
                _context.Update(igrejaDenuncia);
                var resultado = await _context.SaveChangesAsync();
                return resultado > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao solucionar para a igreja ID: {igrejaId}", igrejaDenuncia.Id);
                return false;
            }
        }
    }
}