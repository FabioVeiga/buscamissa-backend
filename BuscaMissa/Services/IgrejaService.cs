using BuscaMissa.Context;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.IgrejaDto;
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

        public async Task<Igreja?> BuscarPorCepAsync(string cep)
        {
            try
            {
                return await _context.Igrejas
                    .Include(igreja => igreja.Endereco)
                    .Include(x => x.Usuario)
                    .FirstOrDefaultAsync(x => x.Endereco.Cep == cep);
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
         
    }
}