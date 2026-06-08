-- =============================================================
-- SCRIPT: Conversão Latitude/Longitude de string para decimal
-- Executar ANTES de aplicar a migration fase1_slug_confianca
-- Banco: BuscaMissa (DEV ou PROD)
-- =============================================================

-- PASSO 1: Diagnóstico — veja o que existe antes de mexer
-- -------------------------------------------------------
SELECT
    COUNT(*)                                            AS total_enderecos,
    SUM(Latitude IS NOT NULL AND Latitude != '')        AS com_latitude,
    SUM(Longitude IS NOT NULL AND Longitude != '')      AS com_longitude,
    SUM(Latitude IS NOT NULL AND Latitude != ''
        AND Longitude IS NOT NULL AND Longitude != '')  AS com_ambos
FROM Enderecos;

-- PASSO 2: Identifica valores que NÃO são decimais válidos
-- (devem retornar 0 linhas — se retornar alguma, revise antes de continuar)
-- -------------------------------------------------------
SELECT
    e.Id,
    e.Latitude,
    e.Longitude,
    i.Nome AS NomeIgreja
FROM Enderecos e
JOIN Igrejas i ON i.Id = e.IgrejaId
WHERE
    (e.Latitude IS NOT NULL AND e.Latitude != ''
        AND e.Latitude REGEXP '[^0-9.\\-]')
    OR
    (e.Longitude IS NOT NULL AND e.Longitude != ''
        AND e.Longitude REGEXP '[^0-9.\\-]');

-- Se o SELECT acima retornar linhas, anote os IDs e corrija manualmente
-- antes de continuar.

-- PASSO 3: Preview da conversão — confira os valores antes de alterar
-- -------------------------------------------------------
SELECT
    e.Id,
    i.Nome,
    e.Latitude                          AS lat_string,
    CAST(e.Latitude AS DECIMAL(10,7))   AS lat_decimal,
    e.Longitude                         AS lng_string,
    CAST(e.Longitude AS DECIMAL(10,7))  AS lng_decimal
FROM Enderecos e
JOIN Igrejas i ON i.Id = e.IgrejaId
WHERE e.Latitude IS NOT NULL AND e.Latitude != ''
LIMIT 20;

-- PASSO 4: Adiciona colunas temporárias para a conversão segura
-- (sem alterar as colunas originais ainda)
-- -------------------------------------------------------
ALTER TABLE Enderecos
    ADD COLUMN IF NOT EXISTS Latitude_new  DECIMAL(10,7) NULL,
    ADD COLUMN IF NOT EXISTS Longitude_new DECIMAL(10,7) NULL;

-- PASSO 5: Popula as novas colunas a partir das strings
-- -------------------------------------------------------
UPDATE Enderecos
SET
    Latitude_new  = CASE
        WHEN Latitude IS NOT NULL AND Latitude != ''
             AND Latitude NOT REGEXP '[^0-9.\\-]'
        THEN CAST(Latitude AS DECIMAL(10,7))
        ELSE NULL
    END,
    Longitude_new = CASE
        WHEN Longitude IS NOT NULL AND Longitude != ''
             AND Longitude NOT REGEXP '[^0-9.\\-]'
        THEN CAST(Longitude AS DECIMAL(10,7))
        ELSE NULL
    END;

-- PASSO 6: Validação — compara totais antes de remover as originais
-- (os dois SELECTs devem retornar o mesmo número)
-- -------------------------------------------------------
SELECT 'string originals'  AS fonte, SUM(Latitude IS NOT NULL AND Latitude != '')  AS count FROM Enderecos
UNION ALL
SELECT 'decimal converted' AS fonte, SUM(Latitude_new IS NOT NULL)                 AS count FROM Enderecos;

-- PASSO 7: Troca as colunas
-- Execute só depois de validar o PASSO 6
-- -------------------------------------------------------
ALTER TABLE Enderecos
    DROP COLUMN Latitude,
    DROP COLUMN Longitude,
    CHANGE COLUMN Latitude_new  Latitude  DECIMAL(10,7) NULL,
    CHANGE COLUMN Longitude_new Longitude DECIMAL(10,7) NULL;

-- PASSO 8: Verificação final
-- -------------------------------------------------------
SELECT
    COUNT(*)                              AS total,
    SUM(Latitude IS NOT NULL)             AS com_latitude_decimal,
    SUM(Longitude IS NOT NULL)            AS com_longitude_decimal,
    MIN(Latitude)                         AS lat_min,
    MAX(Latitude)                         AS lat_max,
    MIN(Longitude)                        AS lng_min,
    MAX(Longitude)                        AS lng_max
FROM Enderecos;

-- Valores esperados para o Brasil:
--   Latitude:  entre -33.75 e  5.27
--   Longitude: entre -73.99 e -34.79
-- Se aparecer algo fora dessa faixa, o dado original estava errado.

-- =============================================================
-- APÓS EXECUTAR ESSE SCRIPT: aplique a migration
--   dotnet ef database update
-- A migration já não vai mais alterar o tipo das colunas
-- (elas já estarão como DECIMAL), só vai registrar no histórico.
-- =============================================================
