using BuscaMissa.Context;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services
{
    public class IgrejaTemporariaService(ApplicationDbContext context, ILogger<IgrejaTemporariaService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<IgrejaTemporariaService> _logger = logger;

        public async Task<AtualizacaoIgrejaResponse?> BuscarPorIgrejaIdAsync(int igrejaId)
        {
            try
            {
                var model = await _context.IgrejaTemporarias
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IgrejaId == igrejaId);

                if(model is null){
                    return null;
                }

                var missasTemp = await _context.MissasTemporarias
                    .Where(x => x.IgrejaId == igrejaId)
                    .AsNoTracking()
                    .ToListAsync();

                var igreja = await _context.Igrejas
                    .Include(x => x.Endereco)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == model.IgrejaId);

                var response = new AtualizacaoIgrejaResponse
                {
                    Id = igrejaId,
                    Nome = igreja!.Nome,
                    ImagemUrl = model.ImagemUrl,
                    Paroco = model.Paroco,
                    MissasTemporaria = [.. missasTemp.Select(item => (MissaResponse)item)],
                    Endereco = (EnderecoIgrejaResponse)igreja.Endereco
                };

                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Igreja Temporaria");
                throw;
            }
        }

        public async Task<bool> InserirAsync(AtualicaoIgrejaRequest request)
        {
            var igrejaTemporaria = (IgrejaTemporaria)request;

            var deletaIgrejaTemporaria = await DeletaIgrejaAsync(request.Id);
            var deletaMissasTemporaria = await DeletaMissasTemporarias(request.Id);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.IgrejaTemporarias.Add(igrejaTemporaria);
                    if (deletaMissasTemporaria)
                    {
                        var missasTemp = request.Missas.Select(item =>
                        {
                            var aux = (MissaTemporaria)item;
                            aux.IgrejaId = request.Id;
                            return aux;
                        }).ToList();
                        await _context.MissasTemporarias.AddRangeAsync(missasTemp);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return true;
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro ao inserir igreja temporaria: {request}");
                    await transaction.RollbackAsync();
                    return false;
                }
            });
        }

        public async Task<bool> DeletaIgrejaAsync(int igrejaId)
        {
            try
            {
                var model = await _context.IgrejaTemporarias
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IgrejaId == igrejaId);
                if (model != null)
                {
                    _context.IgrejaTemporarias.Remove(model);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao deletar missas temporarias para a igreja ID: {igrejaId}");
                return false;
            }
        }

        public async Task<bool> DeletaMissasTemporarias(int igrejaId)
        {
            try
            {
                var missas = await _context.MissasTemporarias
                .AsNoTracking()
                .Where(m => m.IgrejaId == igrejaId)
                .ToListAsync();

                if (missas.Count != 0)
                {
                    _context.MissasTemporarias.RemoveRange(missas);
                    var result = await _context.SaveChangesAsync();
                    if (result == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao deletar missas temporarias para a igreja ID: {igrejaId}");
                return false; // Indicate failure
            }
        }
    }
}