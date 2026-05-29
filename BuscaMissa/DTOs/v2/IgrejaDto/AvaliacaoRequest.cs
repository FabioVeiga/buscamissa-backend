using System.Text.Json.Serialization;

namespace BuscaMissa.DTOs.v2.IgrejaDto;

public class AvaliacaoRequest
{
    [JsonIgnore]
    public int IgrejaId { get; set; }
    public int Nota { get; set; }
    public string Fingerprint { get; set; }
}