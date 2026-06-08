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
    private string FrontendBaseUrl => configuration["FrontendBaseUrl"] ?? "https://buscamissa.com.br";

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Sitemap()
    {
        try
        {
            var igrejas = await igrejaService.ObterDadosSitemapAsync();

            var sb = new StringBuilder();
            sb.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
            sb.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");

            var baseUrl = FrontendBaseUrl.TrimEnd('/');

            foreach (var igreja in igrejas)
            {
                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>{baseUrl}/igrejas/{igreja.NomeUnico}</loc>");
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
