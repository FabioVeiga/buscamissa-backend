using BuscaMissa.Context;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services.v1
{
    public class IgrejaReportarProblemaService(ApplicationDbContext context, ILogger<IgrejaReportarProblemaService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<IgrejaReportarProblemaService> _logger = logger;

        public async Task<IgrejaReportarProblema?> BuscarPorIdAsync(int id)
        {
            try
            {
               return await _context.IgrejaReportarProblemas
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

        public async Task<IgrejaReportarProblema?> BuscarPorIgrejaIdAsync(int igrejaId)
        {
            try
            {
               return await _context.IgrejaReportarProblemas
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

        public async Task<IgrejaReportarProblema?> TemProblemaReportadoPorIgrejaIdAsync(int igrejaId)
        {
            try
            {
                return await _context.IgrejaReportarProblemas
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IgrejaId == igrejaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar Igreja");
                throw;
            }
        }
        

        public async Task<Paginacao<ReportarProblemaListItemResponse>> BuscarPorFiltroAsync(FiltroReportarProblemaRequest filtro)
        {
            try
            {
                var query = _context.IgrejaReportarProblemas
                    .Include(x => x.Igreja)
                    .ThenInclude(x => x.Endereco)
                    .AsNoTracking()
                    .AsQueryable();

                query = filtro.Resolvido switch
                {
                    true => query.Where(x => x.AcaoRealizada != null && x.AcaoRealizada != ""),
                    false => query.Where(x => x.AcaoRealizada == null || x.AcaoRealizada == ""),
                    null => query,
                };

                var aux = query
                    .OrderByDescending(x => x.DataCriacao)
                    .Select(x => new ReportarProblemaListItemResponse
                    {
                        Id = x.Id,
                        Descricao = x.Descricao,
                        AcaoRealizada = x.AcaoRealizada,
                        Nome = x.Nome,
                        Email = x.Email,
                        DataCriacao = x.DataCriacao,
                        IgrejaId = x.IgrejaId,
                        NomeIgreja = x.Igreja.Nome,
                        Cidade = x.Igreja.Endereco.Localidade,
                        Uf = x.Igreja.Endereco.Uf,
                    });

                return await aux.PaginacaoAsync(filtro.Paginacao.PageIndex, filtro.Paginacao.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar problemas reportados por filtro");
                throw;
            }
        }

        public async Task<bool> InserirAsync(ReportarProblemaRequest request)
        {
            try{
                var model = (IgrejaReportarProblema)request;
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

        public async Task<bool> AtualizarAsync(IgrejaReportarProblema request)
        {
            try
            {
                _context.Update(request);
                var resultado = await _context.SaveChangesAsync();
                return resultado > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar {request}", request);
                return false;
            }
        }

        public async Task<bool> SolucaoAsync(IgrejaReportarProblema problemaReportado)
        {
            try
            {
                _context.Update(problemaReportado);
                var resultado = await _context.SaveChangesAsync();
                return resultado > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao solucionar para a igreja ID: {igrejaId}", problemaReportado.Id);
                return false;
            }
        }
    }
}