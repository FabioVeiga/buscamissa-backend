using BuscaMissa.Context;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services
{
    public class EnderecoService(ApplicationDbContext context, ILogger<EnderecoService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<EnderecoService> _logger = logger;

        public async Task<Endereco> InserirAsync(EnderecoIgrejaRequest request)
        {
            try
            {
                Endereco model = (Endereco)request;
                _context.Enderecos.Add(model);
                await _context.SaveChangesAsync();
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while insering Endereco {Endereco}", request);
                throw;
            }
        }

        public async Task<EnderecoIgrejaBuscaResponse> BuscarDadosBuscaAsync(EnderecoIgrejaBuscaRequest request)
        {
            try
            {
               var query = await _context.Enderecos
               .Include(e => e.Igreja)
               .AsNoTracking()
               .Where(x => x.Uf.ToUpper() == request.Uf.ToUpper() && x.Igreja.Ativo)
               .ToListAsync();

               var response = new EnderecoIgrejaBuscaResponse();

               if(request.Localidade is not null){
                    response.Localidades = [.. query.Where(x => x.Localidade == request.Localidade).Select(y => y.Localidade).Distinct()];
                    response.Bairros = [.. query.Where(x => x.Localidade == request.Localidade).Select(y => y.Bairro).Distinct()];
                    if(request.Bairro is not null)
                    {
                        response.Bairros = [.. query.Where(x => x.Bairro == request.Bairro).Select(y => y.Bairro).Distinct()];
                    }
               }else{
                response.Localidades = [.. query.Select(y => y.Localidade).Distinct()];
               }

               return response;
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while BuscarDadosBusca Endereco {Endereco}", request);
                throw;
            }
        }
    }
}