using BuscaMissa.Context;
using BuscaMissa.DTOs.v2.ConfiabilidadeDto;
using BuscaMissa.Models;
using BuscaMissa.Util;
using Microsoft.EntityFrameworkCore;

namespace BuscaMissa.Services.v2;

public class ConfiabilidadeService(
    ApplicationDbContext context,
    ILogger<ConfiabilidadeService> logger)
{
    /// <summary>
    /// Registra que o usuário confirmou que os horários estão corretos.
    /// Retorna false se o usuário já confirmou anteriormente (dedup por fingerprint + IP).
    /// </summary>
    public async Task<bool> ConfirmarHorariosAsync(
        int igrejaId,
        ConfirmarHorarioRequest request,
        string ip)
    {
        try
        {
            var hash = GeradorHash.Gerar(request.Fingerprint);

            bool jaConfirmou = await context.ConfirmacoesHorario
                .AnyAsync(x => x.IgrejaId == igrejaId &&
                               (x.HashFingerprint == hash || x.EnderecoIp == ip));

            if (jaConfirmou) return false;

            context.ConfirmacoesHorario.Add(new ConfirmacaoHorario
            {
                IgrejaId        = igrejaId,
                HashFingerprint = hash,
                EnderecoIp      = ip
            });

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao confirmar horários da Igreja {IgrejaId}", igrejaId);
            throw;
        }
    }

    /// <summary>
    /// Retorna o total de confirmações de uma igreja nos últimos N dias.
    /// Reservado para futura evolução do score (fator de comunidade).
    /// </summary>
    public async Task<int> ContarConfirmacoesRecentesAsync(int igrejaId, int dias = 90)
    {
        var limite = DateTime.UtcNow.AddDays(-dias);
        return await context.ConfirmacoesHorario
            .CountAsync(x => x.IgrejaId == igrejaId && x.DataCriacao >= limite);
    }
}
