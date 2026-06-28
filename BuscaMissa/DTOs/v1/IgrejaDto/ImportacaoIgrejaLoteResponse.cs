using BuscaMissa.Models;
using System.Text.Json.Serialization;

namespace BuscaMissa.DTOs.v1.IgrejaDto;

public class ImportacaoIgrejaLoteResponse
{
    public int Inseridas { get; set; }
    public int Puladas { get; set; }
    public IList<ImportacaoErroItem> Erros { get; set; } = [];

    [JsonIgnore]
    public List<Igreja> IgrejasInseridas { get; set; } = [];
}

public class ImportacaoErroItem
{
    public int Linha { get; set; }
    public string Nome { get; set; } = default!;
    public string Motivo { get; set; } = default!;
}
