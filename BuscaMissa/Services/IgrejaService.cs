using BuscaMissa.Context;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services
{
    public class IgrejaService(ApplicationDbContext context, ILogger<IgrejaService> logger, IgrejaTemporariaService igrejaTemporariaService)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<IgrejaService> _logger = logger;
        private readonly IgrejaTemporariaService _igrejaTemporariaService = igrejaTemporariaService;

        public async Task<Igreja?> BuscarPorIdAsync(int id)
        {
            try
            {
                return await _context.Igrejas
                    .Include(igreja => igreja.Endereco)
                    .FirstOrDefaultAsync(igreja => igreja.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Igreja with ID {Id}", id);
                throw;
            }
        }

        public async Task<IgrejaResponse?> BuscarPorCepAsync(string cep)
        {
            try
            {
                var model = await _context.Igrejas
                    .Include(igreja => igreja.Endereco)
                    .Include(x => x.Usuario)
                    .Include(x => x.Missas)
                    .FirstOrDefaultAsync(x => x.Endereco.Cep == CepHelper.FormatarCep(cep));

                if (model == null) return null;
                return (IgrejaResponse)model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Igreja with CEP {Cep}", cep);
                throw;
            }
        }

        public async Task<Igreja> InserirAsync(CriacaoIgrejaRequest request)
        {
            try
            {
                Igreja model = (Igreja)request;
                _context.Igrejas.Add(model);
                await _context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while insering Igreja {IgrejaRequest}", request);
                throw;
            }
        }

        public async Task<IgrejaResponse> AtivarAsync(Controle model, Usuario usuario)
        {
            try
            {
                model.Igreja.Ativo = true;
                model.Igreja.Alteracao = DateTime.Now;
                model.Igreja.Usuario = usuario;
                model.Igreja.UsuarioId = usuario.Id;
                model.Status = Enums.StatusEnum.Finalizado;
                _context.Controles.Update(model);
                await _context.SaveChangesAsync();
                return (IgrejaResponse)model.Igreja;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while activate Igreja {Igreja}", model);
                throw;
            }
        }

        public async Task<Paginacao<IgrejaResponse>> BuscarPorFiltros(FiltroIgrejaRequest filtro)
        {
            try
            {
                var query = _context.Igrejas
                .Include(x => x.Endereco)
                .Include(x => x.Usuario)
                .Where(x =>
                    x.Endereco.Uf == filtro.Uf.ToUpper()
                    && x.Ativo == filtro.Ativo)
                .AsQueryable();

                if (!string.IsNullOrEmpty(filtro.Localidade))
                    query = query.Where(x => x.Endereco.Localidade == filtro.Localidade);

                if (!string.IsNullOrEmpty(filtro.Nome))
                    query = query.Where(x => x.Nome == filtro.Nome.ToUpper());

                if (filtro.DiaDaSemana is not null)
                    query = query.Where(x => x.Missas.Any(y => y.DiaSemana == filtro.DiaDaSemana));

                if (!string.IsNullOrEmpty(filtro.Horario))
                    query = query.Where(x => x.Missas.Any(y => y.Horario == filtro.HorarioMissa));


                var aux = query.Select(x => new IgrejaResponse()
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    Endereco = (EnderecoIgrejaResponse)x.Endereco,
                    Usuario = (UsuarioDtoResponse)x.Usuario,
                    Alteracao = x.Alteracao,
                    Ativo = x.Ativo,
                    Criacao = x.Criacao,
                    ImagemUrl = x.ImagemUrl,
                    Paroco = x.Paroco,
                    Missas = x.Missas.Select(m => (MissaResponse)m).ToList()
                });

                var resultado = await aux.PaginacaoAsync(filtro.Paginacao.PageIndex, filtro.Paginacao.PageSize);
                return resultado;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Igreja> EditarAsync(Igreja model)
        {
            try
            {
                _context.Igrejas.Update(model);
                await _context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing Igreja {Igreja}", model);
                throw;
            }
        }

        public async Task<bool> EditarPorTemporariaAsync(Igreja igreja, AtualizacaoIgrejaResponse atualizacao)
        {
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Missas.RemoveRange(igreja.Missas);
                _context.Missas.AddRange(atualizacao.MissasTemporaria.Select(x => new Missa(){
                    DiaSemana = x.DiaSemana,
                    Horario = TimeSpan.Parse(x.Horario),
                    IgrejaId = igreja.Id,
                    Observacao = x.Observacao
                }));
                igreja.Paroco = atualizacao.Paroco;
                igreja.ImagemUrl = atualizacao.ImagemUrl;
                igreja.Alteracao = DateTime.Now;
                await _igrejaTemporariaService.DeletaIgrejaAsync(igreja.Id);
                await _igrejaTemporariaService.DeletaMissasTemporarias(igreja.Id);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing Igreja {Igreja}", igreja);
                await transaction.RollbackAsync();
                return false;
            }

        }
    
        public InformacoesGeraisResponse InformacoesGeraisResponse()
        {
            try
            {
                return new InformacoesGeraisResponse{
                    QuantidadesIgrejas = _context.Igrejas.Count(x => x.Ativo),
                    QuantidadeMissas = _context.Missas.Count()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting InformacoesGeraisResponse");
                throw;
            }
        }
    
    }
}