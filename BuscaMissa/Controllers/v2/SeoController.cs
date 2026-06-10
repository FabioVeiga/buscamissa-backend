using BuscaMissa.DTOs.v2.SeoDto;
using BuscaMissa.Services.v2;
using Microsoft.AspNetCore.Mvc;

namespace BuscaMissa.Controllers.v2;

[ApiController]
public class SeoController(
    ILogger<SeoController> logger,
    IgrejaService igrejaService) : ControllerBase
{
    [HttpGet("/v2/seo/routes")]
    public async Task<IActionResult> GetRoutes()
    {
        try
        {
            var igrejas = await igrejaService.ObterDadosSitemapAsync();

            var parishes = igrejas
                .Where(x => x.Slug != null && x.CidadeSlug != null && x.Uf != null)
                .Select(x => new ParoquiaSeoDto(
                    x.Uf!.ToLower(),
                    x.CidadeSlug!,
                    x.Slug!,
                    x.Alteracao))
                .ToList();

            var cities = igrejas
                .Where(x => x.CidadeSlug != null && x.Uf != null)
                .GroupBy(x => (Uf: x.Uf!.ToLower(), CidadeSlug: x.CidadeSlug!))
                .Select(g => new CidadeSeoDto(
                    g.Key.Uf,
                    g.Key.CidadeSlug,
                    g.Max(x => x.Alteracao)))
                .ToList();

            return Ok(new SeoRoutesResponse(cities, parishes, DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            logger.LogError("{Ex}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
