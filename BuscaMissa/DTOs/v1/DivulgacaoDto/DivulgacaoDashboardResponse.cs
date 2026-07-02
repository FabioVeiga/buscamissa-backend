namespace BuscaMissa.DTOs.v1.DivulgacaoDto;

public class DivulgacaoDashboardResponse
{
    public int TotalIgrejas { get; set; }

    public int SemContatoEmail { get; set; }

    public int SemEmailAlteracaoPendente { get; set; }

    public int SemContatoFacebook { get; set; }

    public int SemContatoInstagram { get; set; }
}
