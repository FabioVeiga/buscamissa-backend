-- =============================================================
-- SCRIPT: Correção retroativa de MetricasDiarias.Data (bug de fuso horário)
-- Contexto: ServicoMetricas.IncrementarAsync calculava o "dia de hoje"
-- com DateOnly.FromDateTime(DateTime.UtcNow). Como o Brasil é UTC-3,
-- toda métrica registrada entre 21:00 e 23:59 (horário do Brasil) foi
-- gravada com Data = dia seguinte (pois em UTC já era 00:00-02:59 do
-- dia seguinte). Corrigido em fix/timezone-metricas-brasil (PR #90).
--
-- LIMITAÇÃO IMPORTANTE: MetricasDiarias guarda só a contagem agregada
-- por dia (Quantidade), não o horário de cada incremento individual.
-- Só sabemos o instante do PRIMEIRO incremento do dia (CriadoEm) e do
-- ÚLTIMO (AtualizadoEm). Por isso, só corrigimos automaticamente as
-- linhas em que TODA a atividade do dia (criação e última atualização)
-- ficou dentro da janela suspeita — ou seja, não há risco de mover
-- incrementos que legitimamente pertencem ao dia certo.
-- Linhas com atividade "misturada" (começaram na janela suspeita mas
-- continuaram depois) ficam listadas para revisão manual (Passo 2) e
-- NÃO são alteradas por este script.
--
-- FAÇA BACKUP da tabela MetricasDiarias antes de rodar os passos 3 e 4.
-- Banco: BuscaMissa (DEV ou PROD)
-- =============================================================

-- PASSO 1: Diagnóstico — dimensiona o problema antes de mexer
-- -------------------------------------------------------
SELECT
    COUNT(*) AS total_linhas,
    SUM(HOUR(CriadoEm) IN (0, 1, 2) AND DATE(CriadoEm) = Data) AS linhas_com_criacao_suspeita,
    SUM(
        HOUR(CriadoEm) IN (0, 1, 2) AND DATE(CriadoEm) = Data
        AND HOUR(AtualizadoEm) IN (0, 1, 2) AND DATE(AtualizadoEm) = Data
    ) AS linhas_seguras_para_corrigir,
    SUM(
        HOUR(CriadoEm) IN (0, 1, 2) AND DATE(CriadoEm) = Data
        AND NOT (HOUR(AtualizadoEm) IN (0, 1, 2) AND DATE(AtualizadoEm) = Data)
    ) AS linhas_mistas_revisao_manual
FROM MetricasDiarias;

-- PASSO 2: Linhas "mistas" — atividade começou na janela suspeita mas
-- continuou depois das 03:00 UTC (00:00 Brasil). Não são alteradas
-- automaticamente porque não dá para saber quantos dos incrementos
-- pertencem ao dia certo e quantos ao dia anterior. Revise manualmente
-- se o volume for relevante.
-- -------------------------------------------------------
SELECT
    Id, TipoEntidade, EntidadeId, TipoMetrica, Data, Quantidade, CriadoEm, AtualizadoEm
FROM MetricasDiarias
WHERE HOUR(CriadoEm) IN (0, 1, 2) AND DATE(CriadoEm) = Data
  AND NOT (HOUR(AtualizadoEm) IN (0, 1, 2) AND DATE(AtualizadoEm) = Data)
ORDER BY Data, EntidadeId;

-- PASSO 3: Corrige linhas seguras SEM conflito — quando ainda não existe
-- uma linha para (TipoEntidade, EntidadeId, TipoMetrica, Data - 1), basta
-- mover a própria linha um dia para trás.
-- -------------------------------------------------------
UPDATE MetricasDiarias m
SET Data = DATE_SUB(Data, INTERVAL 1 DAY)
WHERE HOUR(m.CriadoEm) IN (0, 1, 2) AND DATE(m.CriadoEm) = m.Data
  AND HOUR(m.AtualizadoEm) IN (0, 1, 2) AND DATE(m.AtualizadoEm) = m.Data
  AND NOT EXISTS (
      SELECT 1 FROM MetricasDiarias m2
      WHERE m2.TipoEntidade = m.TipoEntidade
        AND m2.EntidadeId   = m.EntidadeId
        AND m2.TipoMetrica  = m.TipoMetrica
        AND m2.Data         = DATE_SUB(m.Data, INTERVAL 1 DAY)
  );

-- PASSO 4: Corrige linhas seguras COM conflito — já existe uma linha
-- legítima para o dia anterior (mesma entidade/tipo de métrica). Soma a
-- quantidade nela, estende CriadoEm/AtualizadoEm e remove a linha errada.
-- -------------------------------------------------------
UPDATE MetricasDiarias destino
JOIN MetricasDiarias origem
  ON  destino.TipoEntidade = origem.TipoEntidade
  AND destino.EntidadeId   = origem.EntidadeId
  AND destino.TipoMetrica  = origem.TipoMetrica
  AND destino.Data         = DATE_SUB(origem.Data, INTERVAL 1 DAY)
SET
  destino.Quantidade   = destino.Quantidade + origem.Quantidade,
  destino.CriadoEm     = LEAST(destino.CriadoEm, origem.CriadoEm),
  destino.AtualizadoEm = GREATEST(destino.AtualizadoEm, origem.AtualizadoEm)
WHERE HOUR(origem.CriadoEm) IN (0, 1, 2) AND DATE(origem.CriadoEm) = origem.Data
  AND HOUR(origem.AtualizadoEm) IN (0, 1, 2) AND DATE(origem.AtualizadoEm) = origem.Data;

DELETE origem FROM MetricasDiarias origem
JOIN MetricasDiarias destino
  ON  destino.TipoEntidade = origem.TipoEntidade
  AND destino.EntidadeId   = origem.EntidadeId
  AND destino.TipoMetrica  = origem.TipoMetrica
  AND destino.Data         = DATE_SUB(origem.Data, INTERVAL 1 DAY)
WHERE HOUR(origem.CriadoEm) IN (0, 1, 2) AND DATE(origem.CriadoEm) = origem.Data
  AND HOUR(origem.AtualizadoEm) IN (0, 1, 2) AND DATE(origem.AtualizadoEm) = origem.Data;

-- PASSO 5: Verificação final — deve restar só as linhas "mistas" do
-- Passo 2 (se houver), já que essas não são tocadas por este script.
-- -------------------------------------------------------
SELECT
    COUNT(*) AS total_linhas,
    SUM(
        HOUR(CriadoEm) IN (0, 1, 2) AND DATE(CriadoEm) = Data
        AND HOUR(AtualizadoEm) IN (0, 1, 2) AND DATE(AtualizadoEm) = Data
    ) AS linhas_seguras_restantes -- deve ser 0 após rodar os passos 3 e 4
FROM MetricasDiarias;
