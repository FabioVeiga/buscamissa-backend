using BuscaMissa.Context;
using BuscaMissa.Models;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services
{
    public class ContribuidoresService(ApplicationDbContext context, ILogger<ContribuidoresService> logger)
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<ContribuidoresService> _logger = logger;

        public async Task<List<Contribuidor>> ObterContribuidoresDoMesVigenteAsync()
        {
            try
            {
                var mesAtual = DateTime.Now.Month;
                var anoAtual = DateTime.Now.Year;

                return await _context.Contribuidores
                    .AsNoTracking()
                    .Where(c => c.DataContribuicao.Month == mesAtual && c.DataContribuicao.Year == anoAtual)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Contribuidores for the current month");
                throw;
            }
        }

        public async Task InserirAsync(Contribuidor model)
        {
            try
            {
                var mesAtual = DateTime.Now.Month;
                var anoAtual = DateTime.Now.Year;

                if (await _context.Contribuidores.AnyAsync(c => c.Nome == model.Nome && c.DataContribuicao.Month == mesAtual && c.DataContribuicao.Year == anoAtual))
                {
                    _logger.LogInformation("Contribuidor with name '{Nome}' already exists for the current month.", model.Nome);
                    return;
                }

                _context.Contribuidores.Add(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting Contribuidor {Contribuidor}", model);
                throw;
            }
        }

        public async Task InserirPorNomesAsync(string nomes)
        {
            try
            {
                var mesAtual = DateTime.Now.Month;
                var anoAtual = DateTime.Now.Year;
                var nomesArray = nomes.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var contribuidores = new List<Contribuidor>();

                foreach (var nome in nomesArray)
                {
                    var trimmedNome = nome.Trim();
                    if (await _context.Contribuidores.AnyAsync(c => c.Nome == trimmedNome && c.DataContribuicao.Month == mesAtual && c.DataContribuicao.Year == anoAtual))
                    {
                        _logger.LogInformation("Contribuidor with name '{Nome}' already exists for the current month.", trimmedNome);
                        continue;
                    }

                    contribuidores.Add(new Contribuidor { Nome = trimmedNome });
                }

                _context.Contribuidores.AddRange(contribuidores);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting Contribuidores from names: {Nomes}", nomes);
                throw;
            }
        }
    }
}