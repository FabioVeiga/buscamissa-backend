namespace BuscaMissa.DTOs.v1.DivulgacaoDto;

public class EnviarEmailLoteResponse
{
    public int TotalSolicitado { get; set; }

    public int TotalEnviado { get; set; }

    public List<EnvioLoteFalhaResponse> Falhas { get; set; } = [];
}

public class EnvioLoteFalhaResponse
{
    public int IgrejaId { get; set; }

    public string? Nome { get; set; }

    public string Motivo { get; set; } = null!;
}
