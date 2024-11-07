using BuscaMissa.Context;
using BuscaMissa.DTOs;
using BuscaMissa.DTOs.ControleDto;
using BuscaMissa.Helpers;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services
{
    public class CodigoValidacaoService(ApplicationDbContext context, ILogger<CodigoValidacaoService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<CodigoValidacaoService> _logger = logger;

        public async Task<CodigoPermissao> InserirAsync(Controle? controle)
        {
            try
            {
                var model = new CodigoPermissao(){
                    Controle = controle,
                    ControleId = controle?.Id,
                    CodigoToken = await GerarCodigo(),
                    ValidoAte = DataHoraHelper.AdicionarHoraEArredondarParaCima(DateTime.Now, 1)
                };
                _context.CodigoPermissoes.Add(model);
                await _context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while insering Controle {Controle}", controle);
                throw;
            }
        }

        public async Task<CodigoPermissao> EditarAsync(CodigoPermissao model)
        {
            try
            {
                model.CodigoToken = await GerarCodigo();
                model.ValidoAte = DataHoraHelper.AdicionarHoraEArredondarParaCima(DateTime.Now, 1);
                _context.CodigoPermissoes.Update(model);
                await _context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing CodigoPermissao {CodigoPermissao}", model);
                throw;
            }
        }

        public async Task<CodigoPermissao?> BuscarPorCodigoTokenAsync(int codigoToken)
        {
            try
            {
                return await _context.CodigoPermissoes
                .Include(x => x.Controle)
                .FirstOrDefaultAsync(x => x.CodigoToken == codigoToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching CodigoPermissao {CodigoPermissao}", codigoToken);
                throw;
            }
        }

        public async Task<CodigoPermissao?> BuscarPorIgrejaIdAsync(int IgrejaId)
        {
            try
            {
                return await _context.CodigoPermissoes
                .Include(x => x.Controle)
                .FirstOrDefaultAsync(x => x.Controle.IgrejaId == IgrejaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching CodigoPermissao {CodigoPermissao}", IgrejaId);
                throw;
            }
        }

        public async Task<CodigoPermissao?> BuscarPorControleIdAsync(int controleId)
        {
            try
            {
                return await _context.CodigoPermissoes
                .Include(x => x.Controle)
                .FirstOrDefaultAsync(x => x.ControleId == controleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching CodigoPermissao {CodigoPermissao}", controleId);
                throw;
            }
        }
        
        public string Validar(CodigoValidadorRequest request, CodigoPermissao codigo)
        {
            try
            {
                if(codigo.ControleId != request.ControleId)
                    return "Este código de validação é inválido";
                if (codigo.CodigoToken != request.CodigoValidador)
                    return "Código de validação invalido";
                if(codigo.ValidoAte <= DateTime.Now)
                    return "Enviado email com o código para validação porque a data foi expirada";
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while validating the token.");
                throw;
            }
        }

        private async Task<int> GerarCodigo()
        {
            var codigoToken = SenhaHelper.GerarSenhaTemporariaInt();
            var temCodigo = await _context.CodigoPermissoes.FirstOrDefaultAsync(c => c.CodigoToken == codigoToken);
            if (temCodigo != null)
                return await GerarCodigo();
            return codigoToken;
        }
    }
}