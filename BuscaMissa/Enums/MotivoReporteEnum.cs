namespace BuscaMissa.Enums;

[Flags]
public enum MotivoReporteEnum
{
    HorarioIncorreto       = 1,
    MissaNaoOcorreMais     = 2,
    InformacaoDesatualizada = 4,
    OutroMotivo            = 8
}
