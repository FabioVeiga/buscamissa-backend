using BuscaMissa.DTOs.IgrejaDto;
using BuscaMissa.Models;

namespace BuscaMissa.Helpers;

public static class IgrejaHelper
{
    public static string CriarNomeUnico(CriacaoIgrejaRequest request)
    {
        if (request.RedeSociais != null && request.RedeSociais.Any())
        {
            return request.RedeSociais.FirstOrDefault()!.NomeDoPerfil.ToLower();
        }
        return $"{request.Endereco.Uf}_{request.Nome.Replace(" ", "").ToLower()}";
    }
    
    public static string CriarNomeUnico(Igreja model)
    {
        return $"{model.Id}_{model.Nome.Replace(" ", "").ToLower()}";
    }
}