namespace BuscaMissa.DTOs.v2.IgrejaDto;

public class ComentarioRequest
{
    public int IgrejaId { get; set; }

    public string Nome { get; set; }

    public string Comentario { get; set; }

    public string Fingerprint { get; set; }
}