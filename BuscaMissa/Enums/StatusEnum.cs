namespace BuscaMissa.Enums
{
    public enum StatusEnum
    {
        Finalizado = 1,
        // Admin recusou a criação/alteração pendente (fila de Aprovações) — distinto de Finalizado.
        Rejeitado = 2,
        Igreja_Criacao = 100,
        Igreja_Criacao_Aguardando_Codigo_Validador = 101,
        Igreja_Atualizacao_Temporaria_Inserido = 200,
        Igreja_Atualizacao_Aguardando_Codigo_Validador = 201,
    }
}