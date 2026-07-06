using System.Globalization;

namespace BuscaMissa.Helpers
{
    public static class DataHoraHelper
    {
        // Fuso horário padrão do Brasil — Windows e Linux têm IDs diferentes.
        // Brasil não observa mais horário de verão (desde 2019), então é fixo em UTC-3.
        public static readonly TimeZoneInfo FusoBrasilia = CarregarFusoBrasilia();

        private static TimeZoneInfo CarregarFusoBrasilia()
        {
            foreach (var id in new[] { "E. South America Standard Time", "America/Sao_Paulo" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch { /* tenta o próximo */ }
            }
            return TimeZoneInfo.Utc; // fallback improvável — UTC-0 melhor do que exceção
        }

        /// <summary>Hora atual no fuso do Brasil, a partir de DateTime.UtcNow.</summary>
        public static DateTime AgoraBrasil() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, FusoBrasilia);

        /// <summary>
        /// Dia de hoje no fuso do Brasil. Usar sempre que "hoje" for atribuído a um
        /// registro (ex: agregação diária de métricas) — DateOnly.FromDateTime(DateTime.UtcNow)
        /// vira o dia ~3h antes da meia-noite real no Brasil (ex: 21h de domingo já é
        /// segunda em UTC).
        /// </summary>
        public static DateOnly HojeBrasil() => DateOnly.FromDateTime(AgoraBrasil());

        public static string Formatar(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm");
        }

        public static string Ano()
        {
            return DateTime.Now.Year.ToString();
        }

        public static bool TryParseHorarioMissa(string? horario, out TimeSpan resultado)
        {
            resultado = default;

            if (string.IsNullOrWhiteSpace(horario))
                return false;

            var valor = horario.Trim();

            if (valor.All(char.IsDigit))
                return TryParseHorarioCompacto(valor, out resultado);

            var formatos = new[]
            {
                @"h\:mm",
                @"hh\:mm",
                @"h\:mm\:ss",
                @"hh\:mm\:ss",
                @"h\:mm\:ss\.FFFFFF",
                @"hh\:mm\:ss\.FFFFFF"
            };

            if (!TimeSpan.TryParseExact(
                    valor,
                    formatos,
                    CultureInfo.InvariantCulture,
                    out var parsed))
            {
                return false;
            }

            if (parsed < TimeSpan.Zero || parsed >= TimeSpan.FromDays(1))
                return false;

            resultado = new TimeSpan(parsed.Hours, parsed.Minutes, 0);
            return true;
        }

        public static TimeSpan ParseHorarioMissaOuFalhar(string? horario)
        {
            if (TryParseHorarioMissa(horario, out var resultado))
                return resultado;

            throw new ArgumentException($"Horário de missa inválido: {horario}");
        }

        public static string? NormalizarHorarioMissa(string? horario)
        {
            return TryParseHorarioMissa(horario, out var resultado)
                ? resultado.ToString(@"hh\:mm")
                : null;
        }

        private static bool TryParseHorarioCompacto(string valor, out TimeSpan resultado)
        {
            resultado = default;

            if (valor.Length is < 3 or > 4)
                return false;

            var horaTexto = valor.Length == 3
                ? valor[..1]
                : valor[..2];

            var minutoTexto = valor[^2..];

            if (!int.TryParse(horaTexto, out var hora) ||
                !int.TryParse(minutoTexto, out var minuto))
            {
                return false;
            }

            if (hora is < 0 or > 23 || minuto is < 0 or > 59)
                return false;

            resultado = new TimeSpan(hora, minuto, 0);
            return true;
        }
    }
}