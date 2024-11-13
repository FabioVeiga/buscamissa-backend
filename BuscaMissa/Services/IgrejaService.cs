using BuscaMissa.Context;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.DTOs.MissaDto;
using BuscaMissa.DTOs.PaginacaoDto;
using BuscaMissa.DTOs.UsuarioDto;
using BuscaMissa.Enums;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services
{
    public class IgrejaService(ApplicationDbContext context, ILogger<IgrejaService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<IgrejaService> _logger = logger;
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
                    .FirstOrDefaultAsync(x => x.Endereco.Cep == CepHelper.FormatarCep(cep));
                if (model == null)
                {
                    return null;
                }
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

                if(!string.IsNullOrEmpty(filtro.Horario))
                    query = query.Where(x => x.Missas.Any(y => y.Horario == filtro.HorarioMissa));
                

                var aux = query.Select(x => new IgrejaResponse(){
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
    }
}