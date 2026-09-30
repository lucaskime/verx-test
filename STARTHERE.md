# START HERE 👋

Guia passo a passo para rodar o **Verx** na sua máquina do zero. Não precisa saber nada do projeto antes.
Siga **na ordem**. Se algum passo der erro, vá até a seção [Deu erro?](#deu-erro) no final.

---

## 0. O que é este projeto?

São 3 programas (APIs) que conversam entre si, mais 2 "coisas de apoio" que rodam no Docker:

```
  Você ──► Identity ──► devolve um TOKEN (é o seu "crachá")
  Você ──► Transaction ──► guarda a transação e avisa pelo RabbitMQ
                                    │
                                    ▼
                         BalanceProjection ──► calcula o SALDO de cada conta
  Você ──► BalanceProjection ──► consulta o saldo
```

| Peça | Para que serve | Endereço |
|---|---|---|
| **Identity** | Cria usuário e faz login (devolve o token JWT) | http://localhost:5191 |
| **Transaction** | Registra créditos e débitos | http://localhost:5107 |
| **BalanceProjection** | Calcula e mostra o saldo por conta | http://localhost:5192 |
| **PostgreSQL** (Docker) | Banco de dados (um banco para cada API) | porta 5432 |
| **RabbitMQ** (Docker) | Fila de mensagens entre Transaction e BalanceProjection | http://localhost:15672 |

> **Docker** é um programa que roda o Postgres e o RabbitMQ em "caixinhas" isoladas (containers),
> para você não precisar instalar nada disso no computador.

---

## 1. Instale as ferramentas (uma vez só)

| Ferramenta | Para quê | Como conferir que instalou |
|---|---|---|
| [Docker Desktop](https://docs.docker.com/get-docker/) | Roda Postgres e RabbitMQ | `docker --version` |
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Roda as APIs | `dotnet --version` (deve começar com `10.`) |
| Ferramenta `dotnet-ef` | Cria as tabelas no banco | `dotnet ef --version` |
| [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/) *(opcional)* | Teste de carga | `k6 version` |

Instale a ferramenta do banco com:

```bash
dotnet tool install --global dotnet-ef
```

✅ **Abra o Docker Desktop e espere ele ficar "running"** antes de continuar.

---

## 2. Baixe o projeto e abra o terminal na pasta certa

Todos os comandos deste guia são rodados **na raiz do projeto** (a pasta `Verx`, onde está o arquivo `docker-compose.yml`).

```bash
cd caminho/para/Verx
ls        # você deve ver: docker-compose.yml  projects  run  tests  STARTHERE.md ...
```

---

## 3. Crie o arquivo `.env` (senhas)

O `.env` guarda as senhas do Postgres e do RabbitMQ. Ele **não vai para o Git**, então cada pessoa cria o seu.

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

✅ **Conferir:** abra o `.env`. Nenhuma linha pode ficar com `troque-me`.

> ⚠️ Não mude as senhas do `.env` depois de subir o Docker pela primeira vez. O Postgres grava a senha
> na criação; se mudar depois, vai dar `password authentication failed` (veja [Deu erro?](#deu-erro)).

---

## 4. Suba o Docker (Postgres + RabbitMQ)

```bash
docker compose up -d postgres rabbitmq
docker compose ps
```

✅ **Conferir:** os dois (`verx-postgres` e `verx-rabbitmq`) precisam aparecer como **`healthy`**.
Se aparecer `starting`, espere uns 20 segundos e rode `docker compose ps` de novo.

Na primeira vez, o Postgres cria sozinho os 3 bancos: `verx_identity`, `verx_transaction` e `verx_balanceprojection`.

---

## 5. Configure os *user-secrets* (conexão com o banco e RabbitMQ)

As APIs precisam saber a senha do banco e do RabbitMQ. Em vez de colocar isso no código
(perigoso: iria para o Git), o .NET tem os **user-secrets**, um cofre local na sua máquina.

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

✅ **Conferir** (troque o caminho pelo projeto que quiser ver):

```bash
dotnet user-secrets list --project projects/BalanceProjection/BalanceProjection.WebApi/BalanceProjection.WebApi.csproj
```

Devem aparecer `Database:ConnectionString`, `RabbitMQ:User` e `RabbitMQ:Password`.

---

## 6. Crie as tabelas (migrations)

Os bancos nascem vazios. As **migrations** criam as tabelas. As APIs **não** fazem isso sozinhas,
então rode estes 3 comandos:

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

✅ **Conferir:** cada comando termina com a palavra **`Done.`**

> 🟡 **Vai aparecer um `fail:` no meio do log. Fique tranquilo, é normal.**
> Antes de criar as tabelas, o EF consulta a tabela `__EFMigrationsHistory` (o "caderninho" das migrations já aplicadas).
> Num banco novo ela ainda não existe, então a consulta falha e o EF mostra o erro. Logo depois ele cria essa tabela e
> aplica as migrations (`Applying migration ...`). O que vale é a palavra **`Done.`** no final de cada comando.
> Também é normal ver o aviso `The Entity Framework tools version ... is older than that of the runtime`; ele não atrapalha.
> (Para silenciar: `dotnet tool update --global dotnet-ef`.)

> Os passos 3 a 6 você faz **uma vez só** por máquina. Da próxima vez, pule direto para o passo 7.

---

## 7. Rode as 3 APIs

O jeito mais fácil: um script que sobe tudo de uma vez.

| Sistema | Comando |
|---|---|
| macOS / Linux | `./run/macos/run.command` |
| Windows | `run\windows\run.bat` |

Ele abre uma janela do navegador para cada API. Para **parar tudo**, aperte `Ctrl+C` no terminal.

**Preferiu o VS Code?** Abra a pasta `Verx`, vá em **Run and Debug** (Ctrl+Shift+D) e escolha
**Todos (Identity + Transaction + BalanceProjection)**.

✅ **Conferir:** abra no navegador. Cada endereço mostra a documentação interativa da API (Scalar).
Ou no terminal:

```bash
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5191/health/ready   # 200
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5107/health/ready   # 200
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5192/health/ready   # 200
```

Logs de cada API ficam em `run/logs/`.

---

## 8. Faça o fluxo completo: login → transação → saldo

Agora vamos ver o sistema funcionando de ponta a ponta.

### 8.1 Faça login e pegue o token

Na primeira vez que o Identity sobe (em Development), ele cria **50 usuários de teste**.
As senhas estão em `projects/Identity/Identity.WebApi/Data/test-credentials.json`. Exemplo:

```bash
curl -s -X POST http://localhost:5191/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user001@example.local","password":"TestUser001!"}'
```

A resposta tem um campo `accessToken`. Esse texto enorme é o seu crachá. Guarde ele numa variável:

```bash
TOKEN="cole-seu-token-aqui"
```

### 8.2 Crie uma transação

```bash
curl -s -X POST http://localhost:5107/api/transactions \
  -H "Authorization: Bearer $TOKEN" \
  -H "Idempotency-Key: $(uuidgen)" \
  -H "Content-Type: application/json" \
  -d '{"type":"Credit","accountId":1,"amount":100.50,"createdBy":"eu"}'
```

- `type`: `Credit` (entra dinheiro) ou `Debit` (sai dinheiro).
- `accountId`: o número da conta (um inteiro, de 1 em diante).
- `Idempotency-Key`: um código único por tentativa. Se você repetir **a mesma chave** com o mesmo corpo,
  a transação **não** é duplicada. Por isso usamos `uuidgen`, que gera um código novo a cada vez.

✅ **Conferir:** resposta **201**. Isso significa "anotei a transação". O saldo ainda pode não ter atualizado
(isso acontece um instante depois, pela fila).

### 8.3 Consulte o saldo

Espere 1 ou 2 segundos e rode:

```bash
curl -s http://localhost:5192/api/balances/1 -H "Authorization: Bearer $TOKEN"
```

✅ **Conferir:** o saldo da conta 1 aparece com o valor que você lançou. 🎉

Para listar todas as contas: `GET http://localhost:5192/api/balances`.

> O token expira depois de um tempo. Se der **401**, faça o login de novo (passo 8.1).

---

## 9. (Opcional) Teste de carga com k6

O script [tests/k6/transactions-burst.js](tests/k6/transactions-burst.js) dispara **10 rodadas de 100 transações
simultâneas**, espalhadas nas contas 1 a 10.

```bash
k6 run -e JWT="$TOKEN" tests/k6/transactions-burst.js
```

No final, o k6 mostra um resumo. O que olhar:
- `status_201` deve ser **1000** (todas deram certo).
- `http_req_failed` deve ser **0.00%**.
- `http_req_duration` mostra o tempo das respostas (`p(95)` = 95% das requisições foram mais rápidas que isso).

Dá para mudar o teste sem editar o arquivo:

```bash
k6 run -e JWT="$TOKEN" -e VUS=200 -e ROUNDS=5 -e ROUND_GAP_SECONDS=10 tests/k6/transactions-burst.js
```

| Variável | O que muda | Padrão |
|---|---|---|
| `VUS` | Requisições simultâneas por rodada | 100 |
| `ROUNDS` | Quantidade de rodadas | 10 |
| `ROUND_GAP_SECONDS` | Segundos entre o início de cada rodada | 5 |
| `ACCOUNT_MIN` / `ACCOUNT_MAX` | Faixa de contas sorteadas | 1 / 10 |

Enquanto o teste roda, abra o painel do RabbitMQ (http://localhost:15672, usuário e senha do `.env`) e
veja a fila `balance-projection` enchendo e esvaziando.

---

## 10. Dia a dia

| Quero… | Comando |
|---|---|
| Subir tudo (depois da primeira vez) | `./run/macos/run.command` (ou `run\windows\run.bat`) |
| Parar as APIs | `Ctrl+C` no terminal do script |
| Parar o Docker **mantendo** os dados | `docker compose down` |
| Parar o Docker e **apagar tudo** (bancos e filas) | `docker compose down -v` |
| Ver se o Docker está de pé | `docker compose ps` |
| Ver logs do RabbitMQ | `docker compose logs -f rabbitmq` |

> ⚠️ Depois de `docker compose down -v` os bancos voltam vazios. Refaça os passos **4 e 6**
> (subir o Docker e rodar as migrations). Os user-secrets do passo 5 continuam valendo, desde que o `.env` não mude.

---

## Deu erro?

| Erro / sintoma | O que significa | O que fazer |
|---|---|---|
| `Cannot connect to the Docker daemon` | O Docker Desktop está fechado | Abra o Docker Desktop e espere iniciar |
| `password authentication failed` | O `.env` mudou depois que o Postgres foi criado | `docker compose down -v`, suba de novo e refaça os passos 4 a 6 (apaga os dados) |
| A API não sobe e reclama de `Database` ou `RabbitMQ` | Faltam os user-secrets | Refaça o passo 5 |
| `relation "..." does not exist` | Faltam as tabelas | Refaça o passo 6 |
| `database "verx_..." does not exist` | O volume do Postgres é antigo e não criou os bancos | Crie na mão: `docker compose exec postgres psql -U verx -d verx -c "CREATE DATABASE verx_identity"` (troque o nome e faça para os 3) |
| `address already in use` / porta em uso | Outro programa usa a porta (5191, 5107, 5192, 5432, 5672 ou 15672) | Feche o outro programa ou a instância antiga das APIs |
| `dotnet ef` não encontrado | Falta a ferramenta | `dotnet tool install --global dotnet-ef` |
| **401 Unauthorized** ao chamar uma API | Token ausente, errado ou expirado | Faça o login de novo (passo 8.1) |
| **400 / 422** ao criar transação | Corpo inválido, ou o mesmo `Idempotency-Key` com dados diferentes | Confira o JSON; use uma chave nova a cada tentativa |
| Saldo não atualiza | A fila ainda está processando, ou o BalanceProjection está parado | Espere uns segundos e veja `run/logs/BalanceProjection.log` e a fila no RabbitMQ |

Ainda travou? Copie a mensagem de erro **inteira** e o número do passo em que parou, e peça ajuda ao time.

---

## Onde ler mais

- [README.md](README.md): visão geral e referência rápida
- [run/README.md](run/README.md): detalhes do script que sobe tudo
- [projects/BalanceProjection/README.md](projects/BalanceProjection/README.md): como o saldo é calculado (inbox, outbox, ACK)
