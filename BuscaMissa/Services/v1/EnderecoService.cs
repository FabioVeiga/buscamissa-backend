using BuscaMissa.Context;
using BuscaMissa.DTOs.EnderecoDto;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BuscaMissa.Services.v1
{
    public class EnderecoService(ApplicationDbContext context, ILogger<EnderecoService> logger, IMemoryCache cache)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<EnderecoService> _logger = logger;
        private readonly IMemoryCache _cache = cache;

        // Dados de endereços mudam pouco (só ao cadastrar/editar igreja); TTL curto
        // evita reagrupar toda a base a cada carregamento da home (3.I).
        private const string CacheKeyOrganizarEnderecos = "enderecos:organizados";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

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

        // ignorarCache: usado pelo Admin ao ajustar dados de igreja e precisar ver o
        // resultado imediatamente, sem esperar o TTL de 10 minutos. O cache é
        // recalculado e reescrito normalmente, só não é lido nessa chamada.
        public async Task<Dictionary<string, Dictionary<string, List<string>>>> OrganizarEnderecosAsync(bool ignorarCache = false)
        {
            try
            {
            if (!ignorarCache &&
                _cache.TryGetValue(CacheKeyOrganizarEnderecos,
                    out Dictionary<string, Dictionary<string, List<string>>>? cached) && cached is not null)
                return cached;

            var enderecos = await _context.Enderecos
                .AsNoTracking()
                .Where(x => x.Igreja.Ativo)
                .ToListAsync();

            var organizado = enderecos
                .GroupBy(e => e.Uf)
                .ToDictionary(
                ufGroup => ufGroup.Key,
                ufGroup => ufGroup
                    .GroupBy(e => e.Localidade)
                    .ToDictionary(
                    localidadeGroup => localidadeGroup.Key,
                    localidadeGroup => localidadeGroup
                        .Select(e => e.Bairro)
                        .Where(bairro => !string.IsNullOrEmpty(bairro))
                        .Distinct()
                        .ToList()
                    )
                );

            _cache.Set(CacheKeyOrganizarEnderecos, organizado, CacheTtl);
            return organizado;
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "An error occurred while organizing addresses.");
            throw;
            }
        }
    }
}