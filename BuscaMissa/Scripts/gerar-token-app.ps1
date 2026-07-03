<#
.SYNOPSIS
  Gera o token JWT da role "App" usado pelo frontend (environment.ts).

.DESCRIPTION
  Gera um JWT HS256 assinado com o SecretApp (Key Vault), incluindo os claims
  iss/aud exigidos pela validação Jwt:ValidarIssuerAudience (auditoria item 1.7).
  O secret é pedido de forma interativa (não fica no histórico do terminal).

.EXAMPLE
  powershell -File .\Scripts\gerar-token-app.ps1
  powershell -File .\Scripts\gerar-token-app.ps1 -Email app@buscamissa.com.br -AnosValidade 5
#>
param(
  [string]$Email = "droidbinho@gmail.com",
  [string]$Issuer = "BuscaMissa",
  [string]$Audience = "BuscaMissaApi",
  [int]$AnosValidade = 10
)

$secure = Read-Host -Prompt "Cole o SecretApp (entrada oculta)" -AsSecureString
$bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
$secret = [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)
[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)

if ([string]::IsNullOrWhiteSpace($secret)) {
  Write-Error "SecretApp vazio."
  exit 1
}

function ConvertTo-Base64Url([byte[]]$bytes) {
  [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+','-').Replace('/','_')
}

$agora = [DateTimeOffset]::UtcNow
$exp = $agora.AddYears($AnosValidade)

# Claims iguais aos gerados por UsuarioService.GerarTokenAsync para perfil App,
# usando os nomes curtos de claim (email/role) que o JwtSecurityTokenHandler emite.
$header = @{ alg = "HS256"; typ = "JWT" } | ConvertTo-Json -Compress
$payload = [ordered]@{
  email = $Email
  role  = "App"
  nbf   = $agora.ToUnixTimeSeconds()
  exp   = $exp.ToUnixTimeSeconds()
  iat   = $agora.ToUnixTimeSeconds()
  iss   = $Issuer
  aud   = $Audience
} | ConvertTo-Json -Compress

$headerB64 = ConvertTo-Base64Url ([Text.Encoding]::UTF8.GetBytes($header))
$payloadB64 = ConvertTo-Base64Url ([Text.Encoding]::UTF8.GetBytes($payload))
$toSign = "$headerB64.$payloadB64"

$hmac = New-Object System.Security.Cryptography.HMACSHA256
# Encoding.ASCII para casar com Program.cs: Encoding.ASCII.GetBytes(secret)
$hmac.Key = [Text.Encoding]::ASCII.GetBytes($secret)
$sigB64 = ConvertTo-Base64Url ($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($toSign)))
$hmac.Dispose()

$jwt = "$toSign.$sigB64"

Write-Host ""
Write-Host "Token App gerado (valido ate $($exp.ToString('yyyy-MM-dd'))):" -ForegroundColor Green
Write-Host ""
Write-Output $jwt
Write-Host ""
Write-Host "Proximos passos:" -ForegroundColor Yellow
Write-Host " 1. Substituir o token em src/environments/environment*.ts do frontend"
Write-Host " 2. Apos deploy do backend com iss/aud, setar Jwt:ValidarIssuerAudience=true"
