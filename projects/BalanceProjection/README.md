# Balance Projection

Read-side service for balance reporting. It consumes `BalanceTransactionCreatedEvent` from the
`balance-transactions` topic, maintains a consolidated row per account and publishes the business
ACK `BalanceTransactionProcessedEvent` to `balance-transaction-processed`.

The solution follows the same Clean Architecture layering as the Transaction service:

| Project | Responsibility |
|---|---|
| `BalanceProjection.Domain` | Entities and ports (repository interfaces). No framework dependencies. |
| `BalanceProjection.Application` | Reports use cases: `Result`/`Error` pattern, request validators, handlers. |
| `BalanceProjection.Infrastructure` | EF Core (PostgreSQL), the DbContext, repository and the two RabbitMQ hosted services. |
| `BalanceProjection.IoC` | Single composition root (`AddBalanceProjection()`): options, DbContext, RabbitMQ connection, health checks. |
| `BalanceProjection.WebApi` | ASP.NET Core host: controllers and the `ApiResponse<T>` envelope. Never references Infrastructure directly. |

## Storage

The service owns the `balance_projection` SQL Server schema:

- `AccountBalances`: consolidated report with current balance, credit/debit totals and count.
- `ProcessedTransactions`: durable inbox used to make at-least-once delivery idempotent.
- `BusinessAcks`: transactional outbox for ACK events sent back to Transaction.
- `__EFMigrationsHistory`: migration history isolated from the Transaction service.

Projection update, inbox insert and ACK outbox insert execute in one database transaction. The
incoming RabbitMQ message is acked only after the commit. `BusinessAckPublisher` relays the
ACK with at-least-once delivery; Transaction is responsible for consuming it and updating its own
`ProcessedAt`. No Transaction table is read or written by this service. Events may arrive out of
order; balance calculation is commutative and `LastTransactionAt` uses the greatest timestamp.

## Configuration

| Section | Consumed by | Keys |
|---|---|---|
| `Database` | IoC (`PersistenceRegistration`) | `ConnectionString`, `MaxRetryCount`, `MaxRetryDelay` |
| `RabbitMQ` | Infrastructure (`RabbitMqConnectionProvider`) | `Host`, `Port`, `User`, `Password`, `VirtualHost` |
| `BalanceEventConsumer` | Infrastructure (`BalanceEventProcessor`) | `MaxConcurrentCalls` |
| `BusinessAck` | Infrastructure (`BusinessAckPublisher`) | `PollingInterval`, `BatchSize` |

All required keys are validated on startup (`ValidateDataAnnotations` + `ValidateOnStart`).

## Health checks

`GET /health`, `GET /health/ready` and `GET /health/live` follow the same contract as Transaction:
`database` opens a direct PostgreSQL connection (`SELECT 1`), `eventbus` checks the RabbitMQ connection. Both are 5-second-timeout checks tagged `ready`; `live` never touches a
dependency.

## Run

Exchanges and queues (`balance-projection`, dead-letter queues) are declared automatically on the first
connection. Configure secrets through environment variables (`Database__ConnectionString`,
`RabbitMQ__User`, `RabbitMQ__Password`) and apply migrations:

```bash
dotnet ef database update \
  --project BalanceProjection.Infrastructure \
  --startup-project BalanceProjection.WebApi

dotnet run --project BalanceProjection.WebApi
```

Reports are available at `GET /api/balances/{accountId}` and `GET /api/balances`, wrapped in the
same `{ success, data, errors, traceId }` envelope used by Transaction.
