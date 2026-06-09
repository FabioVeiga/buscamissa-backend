using BuscaMissa.Context;
using BuscaMissa.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services;

/// <summary>
/// Popula Igreja.Slug e Endereco.CidadeSlug para registros existentes.
/// Idempotente: só atua em registros com Slug/CidadeSlug nulos.
/// Roda uma vez no startup; nas execuções seguintes sai imediatamente.
/// </summary>
public static class SlugBackfillService
{
    public static void Executar(ApplicationDbContext context)
    {
        // Sai cedo se não há nada a preencher (evita custo em todo deploy)
        var temPendentes = context.Igrejas.Any(x => x.Slug == null)
                        || context.Enderecos.Any(x => x.CidadeSlug == null);
        if (!temPendentes) return;

        var igrejas = context.Igrejas
            .Include(x => x.Endereco)
            .ToList();

        // 1) CidadeSlug em todos os endereços sem slug
        foreach (var igreja in igrejas)
        {
            if (igreja.Endereco != null && string.IsNullOrEmpty(igreja.Endereco.CidadeSlug))
                igreja.Endereco.CidadeSlug = IgrejaHelper.CriarCidadeSlug(igreja.Endereco.Localidade);
        }

        // 2) Slug local único dentro de (Uf, CidadeSlug)
        // Agrupa por cidade para garantir unicidade com sufixo
        var grupos = igrejas
            .Where(x => x.Endereco != null)
            .GroupBy(x => new { x.Endereco!.Uf, x.Endereco.CidadeSlug });

        foreach (var grupo in grupos)
        {
            // slugs já existentes na cidade (de registros que já tinham slug)
            var usados = new HashSet<string>(
                grupo.Where(x => !string.IsNullOrEmpty(x.Slug)).Select(x => x.Slug!),
                StringComparer.OrdinalIgnoreCase);

            foreach (var igreja in grupo.Where(x => string.IsNullOrEmpty(x.Slug)).OrderBy(x => x.Id))
            {
                var baseSlug = IgrejaHelper.CriarSlugLocal(igreja.Nome);
                var slug = baseSlug;
                var sufixo = 1;
                while (usados.Contains(slug))
                {
                    sufixo++;
                    slug = IgrejaHelper.CriarNomeUnicoComSufixo(baseSlug, sufixo);
                }
                igreja.Slug = slug;
                usados.Add(slug);
            }
        }

        context.SaveChanges();
    }
}
