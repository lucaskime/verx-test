# Sobe a infra (Docker) e os 3 projetos, cada um em uma janela própria do navegador.
# Ctrl+C encerra os 3 projetos (a infra Docker continua no ar).
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $root

$apps = @(
    @{ Name = 'Identity';          Dir = 'projects\Identity\Identity.WebApi';                   Port = 5191 },
    @{ Name = 'Transaction';       Dir = 'projects\Transaction\Transaction.WebApi';             Port = 5107 },
    @{ Name = 'BalanceProjection'; Dir = 'projects\BalanceProjection\BalanceProjection.WebApi'; Port = 5192 }
)

function Show-Urls {
    Write-Host ''
    Write-Host 'URLs:'
    foreach ($app in $apps) { Write-Host ("  {0,-18} http://localhost:{1}" -f $app.Name, $app.Port) }
    Write-Host ("  {0,-18} http://localhost:15672" -f 'RabbitMQ')
    Write-Host ''
}

function Open-Window($url) {
    if (Get-Command chrome -ErrorAction SilentlyContinue) { Start-Process chrome "--new-window $url" }
    else { Start-Process msedge "--new-window $url" }
}

foreach ($tool in 'docker', 'dotnet') {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { Write-Host "$tool não encontrado."; exit 1 }
}

Show-Urls
New-Item -ItemType Directory -Force -Path 'run\logs' | Out-Null
docker compose up -d postgres rabbitmq
if ($LASTEXITCODE -ne 0) { exit 1 }

$env:ASPNETCORE_ENVIRONMENT = 'Development'
$procs = @()
try {
    foreach ($app in $apps) {
        $procs += Start-Process dotnet -PassThru -NoNewWindow `
            -ArgumentList "run --project `"$($app.Dir)`" --urls http://localhost:$($app.Port)" `
            -RedirectStandardOutput "run\logs\$($app.Name).log" `
            -RedirectStandardError "run\logs\$($app.Name).err.log"
    }

    foreach ($app in $apps) {
        Write-Host -NoNewline "Aguardando $($app.Name) (porta $($app.Port))"
        while ($true) {
            try { Invoke-WebRequest "http://localhost:$($app.Port)/" -UseBasicParsing -TimeoutSec 2 | Out-Null; break }
            catch {
                if ($_.Exception.Response) { break }  # respondeu com erro HTTP: já está no ar
                Write-Host -NoNewline '.'; Start-Sleep -Seconds 1
            }
        }
        Write-Host ' ok'
        Open-Window "http://localhost:$($app.Port)/"
        Start-Sleep -Seconds 1
    }

    Write-Host 'Tudo no ar. Logs em run\logs\. Ctrl+C para encerrar.'
    Show-Urls
    Wait-Process -Id ($procs | ForEach-Object Id)
}
finally {
    Write-Host 'Encerrando...'
    foreach ($p in $procs) { taskkill /PID $p.Id /T /F 2>$null | Out-Null }
}
