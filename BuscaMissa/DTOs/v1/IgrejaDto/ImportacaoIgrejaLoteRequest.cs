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
    public string? Telefone { get; set; }
    public string? WhatsApp { get; set; }
    public string? Site { get; set; }

    // Endereço completo — quando preenchido, ignora ViaCEP
    public string? Logradouro { get; set; }
    public string? Bairro { get; set; }
    public string? Localidade { get; set; }
    public string? Uf { get; set; }
    public string? Estado { get; set; }
    public string? Regiao { get; set; }

    // URL pública da foto da paróquia — o backend baixa e sobe para o blob
    public string? ImagemUrl { get; set; }

    // Status ativo — default true; permite importar pontos incompletos como inativos
    public bool? Ativo { get; set; }

    // Coordenadas (geocodificadas) — localizam o ponto mesmo sem CEP
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class ImportacaoMissaRequest
{
    // Aceita: "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo"
    public string DiaSemana { get; set; } = default!;

    // Formato: "HH:mm"
    public string Horario { get; set; } = default!;

    // Observação por missa (ex: "1ª sexta do mês", "Missa pela saúde")
    public string? Observacao { get; set; }
}
