using System.Linq;
using System.Text;
using Asp.Versioning;
using BuscaMissa.Services.v2;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("[controller]")]
public class SitemapController(
    ILogger<SitemapController> logger,
    IgrejaService igrejaService,
    IConfiguration configuration) : ControllerBase
{
    private string FrontendBaseUrl => configuration["FrontendBaseUrl"] ?? BuscaMissa.Constants.Constants.FrontendBaseUrlDefault;

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Sitemap()
    {
        try
        {
            var igrejas = await igrejaService.ObterDadosSitemapAsync();
            var baseUrl = FrontendBaseUrl.TrimEnd('/');

            var sb = new StringBuilder();
            sb.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
            sb.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");

            // Home
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/home</loc>");
            sb.AppendLine("    <changefreq>daily</changefreq>");
            sb.AppendLine("    <priority>1.0</priority>");
            sb.AppendLine("  </url>");

            // Páginas de cidade (distintas por uf + cidadeSlug) — lastmod = alteração mais recente da cidade
            var cidades = igrejas
                .Where(x => !string.IsNullOrEmpty(x.Uf) && !string.IsNullOrEmpty(x.CidadeSlug))
                .GroupBy(x => new { Uf = x.Uf!.ToLower(), CidadeSlug = x.CidadeSlug! })
                .Select(g => new { g.Key.Uf, g.Key.CidadeSlug, UltimaAlteracao = g.Max(i => i.Alteracao) });

            foreach (var cidade in cidades)
            {
                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>{baseUrl}/missas/{cidade.Uf}/{cidade.CidadeSlug}</loc>");
                sb.AppendLine($"    <lastmod>{cidade.UltimaAlteracao:yyyy-MM-dd}</lastmod>");
                sb.AppendLine("    <changefreq>weekly</changefreq>");
                sb.AppendLine("    <priority>0.9</priority>");
                sb.AppendLine("  </url>");
            }

            // Páginas de paróquia — URL canônica nova quando há slug+cidade; senão legado
            foreach (var igreja in igrejas)
            {
                var temUrlNova = !string.IsNullOrEmpty(igreja.Uf)
                              && !string.IsNullOrEmpty(igreja.CidadeSlug)
                              && !string.IsNullOrEmpty(igreja.Slug);
                var loc = temUrlNova
                    ? $"{baseUrl}/paroquia/{igreja.Uf!.ToLower()}/{igreja.CidadeSlug}/{igreja.Slug}"
                    : $"{baseUrl}/igrejas/{igreja.NomeUnico}";

                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>{loc}</loc>");
                sb.AppendLine($"    <lastmod>{igreja.Alteracao:yyyy-MM-dd}</lastmod>");
                sb.AppendLine("    <changefreq>weekly</changefreq>");
                sb.AppendLine("    <priority>0.8</priority>");
                sb.AppendLine("  </url>");
            }

            sb.AppendLine("</urlset>");

            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
