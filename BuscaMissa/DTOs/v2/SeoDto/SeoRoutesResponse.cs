namespace BuscaMissa.DTOs.v2.SeoDto;

public record CidadeSeoDto(string Uf, string CitySlug, DateTime LastModified);
public record ParoquiaSeoDto(string Uf, string CitySlug, string Slug, DateTime LastModified);
public record SeoRoutesResponse(
    IList<CidadeSeoDto> Cities,
    IList<ParoquiaSeoDto> Parishes,
    DateTime GeneratedAt);
