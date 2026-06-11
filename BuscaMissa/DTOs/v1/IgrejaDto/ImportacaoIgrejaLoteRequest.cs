using BuscaMissa.Enums;

namespace BuscaMissa.DTOs.v1.IgrejaDto;

public class ImportacaoIgrejaLoteRequest
{
    public IList<ImportacaoIgrejaItemRequest> Igrejas { get; set; } = [];
}

public class ImportacaoIgrejaItemRequest
{
    public string Nome { get; set; } = default!;
    public string? Paroco { get; set; }
    public string Cep { get; set; } = default!;
    public int Numero { get; set; }
    public IList<ImportacaoMissaRequest> Missas { get; set; } = [];

    // Contato — todos opcionais
    public string? Email { get; set; }

    // Telefone e WhatsApp em formato livre, ex: "(19) 3234-8269" ou "19 99235-4070"
    public string? Telefone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Site { get; set; }
}

public class ImportacaoMissaRequest
{
    // Aceita: "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo"
    public string DiaSemana { get; set; } = default!;

    // Formato: "HH:mm"
    public string Horario { get; set; } = default!;
}
