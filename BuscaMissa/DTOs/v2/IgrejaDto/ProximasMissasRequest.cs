using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.DTOs.v2.IgrejaDto;

public class ProximasMissasRequest
{
    [Required]
    public decimal Lat { get; set; }

    [Required]
    public decimal Lng { get; set; }

    /// <summary>Raio de busca em km. Padrão: 10. Máximo: 50.</summary>
    [Range(0.1, 50)]
    public decimal RaioKm { get; set; } = 10;

    /// <summary>Janela temporal em horas. Fixado em 2 conforme spec do produto (tela Missa Agora).</summary>
    [Range(1, 2)]
    public int Horas { get; set; } = 2;
}
