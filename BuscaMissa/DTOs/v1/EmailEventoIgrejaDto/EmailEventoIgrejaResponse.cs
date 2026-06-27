using BuscaMissa.Enums;
using BuscaMissa.Models;

namespace BuscaMissa.DTOs.v1.EmailEventoIgrejaDto;

public class EmailEventoIgrejaResponse
{
    public int Id { get; set; }

    public int IgrejaId { get; set; }

    public string? IgrejaNome { get; set; }

    public TipoEmailEventoIgrejaEnum Tipo { get; set; }

    public string TipoDescricao => Tipo.ToString();

    public string Assunto { get; set; } = null!;

    public string EmailDestino { get; set; } = null!;

    public string? NomeDestino { get; set; }

    public string Html { get; set; } = null!;

    public bool Ativo { get; set; }

    public bool Enviado { get; set; }

    public DateTime? DataEnvio { get; set; }

    public DateTime DataCriacao { get; set; }

    public DateTime? DataAlteracao { get; set; }

    public string? Observacao { get; set; }

    public static explicit operator EmailEventoIgrejaResponse(EmailEventoIgreja model)
    {
        return new EmailEventoIgrejaResponse
        {
            Id = model.Id,
            IgrejaId = model.IgrejaId,
            IgrejaNome = model.Igreja?.Nome,
            Tipo = model.Tipo,
            Assunto = model.Assunto,
            EmailDestino = model.EmailDestino,
            NomeDestino = model.NomeDestino,
            Html = model.Html,
            Ativo = model.Ativo,
            Enviado = model.Enviado,
            DataEnvio = model.DataEnvio,
            DataCriacao = model.DataCriacao,
            DataAlteracao = model.DataAlteracao,
            Observacao = model.Observacao
        };
    }
}