# Run

Sobe a infra (PostgreSQL, RabbitMQ) e os 3 projetos, cada um em uma janela do navegador.

| Projeto           | URL                    |
|-------------------|------------------------|
| Identity          | http://localhost:5191  |
| Transaction       | http://localhost:5107  |
| BalanceProjection | http://localhost:5192  |
| RabbitMQ (painel) | http://localhost:15672 |

## macOS

```bash
./run/macos/run.command
```

Também funciona com duplo clique no Finder.

## Windows

Duplo clique em `run\windows\run.bat`, ou no terminal:

```bat
run\windows\run.bat
```

## Requisitos

- Docker em execução
- .NET 10 SDK
- Arquivo `.env` na raiz (senhas do Docker) e user-secrets dos projetos configurados

## Encerrar

`Ctrl+C` no terminal encerra os 3 projetos. A infra Docker continua no ar; para parar: `docker compose down`.

Logs em `run/logs/`.
