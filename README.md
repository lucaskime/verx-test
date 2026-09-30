# Verx

Serviços: **Identity** (autenticação/JWT), **Transaction** (lançamentos) e **BalanceProjection** (saldos).
Infra no Docker: **PostgreSQL 17** e **RabbitMQ 4**.

| Serviço           | URL                    | Banco                   |
|-------------------|------------------------|-------------------------|
| Identity          | http://localhost:5191  | `verx_identity`         |
| Transaction       | http://localhost:5107  | `verx_transaction`      |
| BalanceProjection | http://localhost:5192  | `verx_balanceprojection`|
| RabbitMQ (painel) | http://localhost:15672 | —                       |

## Requisitos

- [Docker](https://docs.docker.com/get-docker/) em execução
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Ferramenta do EF Core: `dotnet tool install --global dotnet-ef`

## Primeira execução

Os passos 1 a 4 são feitos **uma vez** por máquina.

### 1. Criar o `.env` com senhas fortes

O `.env` não é versionado. Crie-o a partir do `.env.example`, gerando uma senha para cada variável.

**macOS / Linux**

```bash
cp .env.example .env
for v in POSTGRES_PASSWORD RABBITMQ_PASSWORD; do
  sed -i.bak "s|^$v=.*|$v=$(openssl rand -hex 24)|" .env
done
rm .env.bak
```

**Windows (PowerShell)**

```powershell
Copy-Item .env.example .env
function New-Secret { $b = New-Object byte[] 24; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); ($b | ForEach-Object { $_.ToString('x2') }) -join '' }
$env = Get-Content .env
foreach ($v in 'POSTGRES_PASSWORD','RABBITMQ_PASSWORD') {
  $env = $env -replace "^$v=.*", "$v=$(New-Secret)"
}
$env | Set-Content .env
```

### 2. Subir o Docker

```bash
docker compose up -d postgres rabbitmq
docker compose ps        # os 2 devem aparecer como "healthy"
```

Na primeira subida o Postgres cria os bancos `verx_identity`, `verx_transaction` e `verx_balanceprojection`
(via `docker/postgres/init-databases.sh`). Esse script só roda com o volume vazio.
Se os bancos não existirem (volume antigo), crie-os:

```bash
docker compose exec postgres psql -U verx -d verx -c "CREATE DATABASE verx_identity"
docker compose exec postgres psql -U verx -d verx -c "CREATE DATABASE verx_transaction"
docker compose exec postgres psql -U verx -d verx -c "CREATE DATABASE verx_balanceprojection"
```

> Usuário diferente de `verx`? Troque pelo valor de `POSTGRES_USER` do `.env`.

### 3. Configurar os user-secrets dos serviços

Connection string do banco e credenciais do RabbitMQ ficam nos *user-secrets* do .NET
(não entram no repositório). Substitua `<...>` pelos valores do seu `.env`.

**macOS / Linux**

```bash
set -a; . ./.env; set +a
I=projects/Identity/Identity.WebApi/Identity.WebApi.csproj
T=projects/Transaction/Transaction.WebApi/Transaction.WebApi.csproj
B=projects/BalanceProjection/BalanceProjection.WebApi/BalanceProjection.WebApi.csproj

dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=verx_identity;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" --project $I
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=verx_transaction;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" --project $T
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=verx_balanceprojection;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" --project $B

for P in $T $B; do
  dotnet user-secrets set "RabbitMQ:User" "$RABBITMQ_USER" --project $P
  dotnet user-secrets set "RabbitMQ:Password" "$RABBITMQ_PASSWORD" --project $P
done
```

**Windows (PowerShell)**

```powershell
Get-Content .env | Where-Object { $_ -match '^\w+=' } | ForEach-Object {
  $k, $v = $_ -split '=', 2; Set-Item "env:$k" $v
}
$I = 'projects\Identity\Identity.WebApi\Identity.WebApi.csproj'
$T = 'projects\Transaction\Transaction.WebApi\Transaction.WebApi.csproj'
$B = 'projects\BalanceProjection\BalanceProjection.WebApi\BalanceProjection.WebApi.csproj'

dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=verx_identity;Username=$env:POSTGRES_USER;Password=$env:POSTGRES_PASSWORD" --project $I
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=verx_transaction;Username=$env:POSTGRES_USER;Password=$env:POSTGRES_PASSWORD" --project $T
dotnet user-secrets set "Database:ConnectionString" "Host=localhost;Port=5432;Database=verx_balanceprojection;Username=$env:POSTGRES_USER;Password=$env:POSTGRES_PASSWORD" --project $B

foreach ($P in $T, $B) {
  dotnet user-secrets set "RabbitMQ:User" $env:RABBITMQ_USER --project $P
  dotnet user-secrets set "RabbitMQ:Password" $env:RABBITMQ_PASSWORD --project $P
}
```

### 4. Aplicar as migrations

As APIs **não** criam as tabelas sozinhas. O Identity usa o schema `identity` no banco `verx_identity`;
em Development, a API cria 50 usuários de teste se a tabela estiver vazia (credenciais em `Identity.WebApi/Data/test-credentials.json`).

**macOS / Linux**

```bash
(cd projects/Identity && dotnet ef database update --project Identity.WebApi --startup-project Identity.WebApi)
(cd projects/Transaction && dotnet ef database update --project Transaction.Infrastructure --startup-project Transaction.WebApi)
(cd projects/BalanceProjection && dotnet ef database update --project BalanceProjection.Infrastructure --startup-project BalanceProjection.WebApi)
```

**Windows (PowerShell)**

```powershell
Push-Location projects\Identity
dotnet ef database update --project Identity.WebApi --startup-project Identity.WebApi
Pop-Location
Push-Location projects\Transaction
dotnet ef database update --project Transaction.Infrastructure --startup-project Transaction.WebApi
Pop-Location
Push-Location projects\BalanceProjection
dotnet ef database update --project BalanceProjection.Infrastructure --startup-project BalanceProjection.WebApi
Pop-Location
```

## Rodar a aplicação

Um comando sobe o Docker (se ainda não estiver no ar) e os 3 serviços, cada um em uma janela do navegador.

| Sistema | Comando |
|---|---|
| macOS | `./run/macos/run.command` (ou duplo clique no Finder) |
| Linux | `./run/macos/run.command` |
| Windows | duplo clique em `run\windows\run.bat`, ou `run\windows\run.bat` no terminal |

`Ctrl+C` encerra os 3 serviços. Logs em `run/logs/`. Mais detalhes em [run/README.md](run/README.md).

### Pelo VS Code

Abra a pasta na raiz, vá em **Run and Debug** e escolha
**Todos (Identity + Transaction + BalanceProjection)**.

## Comandos úteis

```bash
docker compose ps                      # estado da infra
docker compose logs -f rabbitmq        # logs de um serviço
docker compose down                    # para a infra (mantém os dados)
docker compose down -v                 # para e APAGA os dados (bancos e filas)
```

Painel do RabbitMQ: http://localhost:15672 (usuário e senha do `.env`).
As portas do Postgres e RabbitMQ ficam expostas somente em `127.0.0.1`.

## Problemas comuns

- **`password authentication failed`** — o `.env` foi alterado depois que o volume do Postgres foi criado.
  Rode `docker compose down -v` e suba de novo (apaga os dados), ou ajuste o `.env` para a senha antiga.
- **API não sobe reclamando de `Database`/`RabbitMQ`** — faltam os user-secrets (passo 3).
- **`relation ... does not exist`** — faltam as migrations (passo 4).
- **Porta em uso** — outro processo usa 5191, 5107, 5192, 5432, 5672, 6379 ou 15672.
