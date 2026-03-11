using System.Text.Json.Serialization;

namespace BuscaMissa.DTOs.v2.IgrejaDto;

public class CurtidaRequest
{
    [JsonIgnore]
    public int IgrejaId { get; set; }
    public string Fingerprint { get; set; }
}