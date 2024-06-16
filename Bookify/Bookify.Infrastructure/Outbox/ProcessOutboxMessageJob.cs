using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Abstractions.Data;
using Bookify.Domain.Abstractions;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using System.Data;

namespace Bookify.Infrastructure.Outbox;

[DisallowConcurrentExecution]
internal sealed partial class ProcessOutboxMessageJob : IJob
{
    private static readonly JsonSerializerSettings JsonSerializeSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IPublisher _publisher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly OutboxOptions _outboxOptions;
    private readonly ILogger<ProcessOutboxMessageJob> _logger;

    public ProcessOutboxMessageJob(
        ISqlConnectionFactory sqlConnectionFactory, 
        IPublisher publisher, 
        IDateTimeProvider dateTimeProvider, 
        IOptions<OutboxOptions> outboxOptions, 
        ILogger<ProcessOutboxMessageJob> logger)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _publisher = publisher;
        _dateTimeProvider = dateTimeProvider;
        _outboxOptions = outboxOptions.Value;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Processing outbox messages.");

        using var connection = _sqlConnectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        var outboxMessages = await GetOutboxMessageAsync(connection, transaction);

        foreach (var outboxMessage in outboxMessages)
        {
            Exception? exception = null;

            try
            {
                var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                    outboxMessage.Content, 
                    JsonSerializeSettings);

                await _publisher.Publish(domainEvent, context.CancellationToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Error occurred while processing outbox message with id: {Id}.", 
                    outboxMessage.Id);

                exception = ex;
            }

            await UpdateOutboxMessageAsync(connection, transaction, outboxMessage, exception);
        }

        transaction.Commit();

        _logger.LogInformation("Outbox messages processed.");
    }

    private async Task UpdateOutboxMessageAsync(
        IDbConnection connection, 
        IDbTransaction transaction, 
        OutboxMessageResponse outboxMessage, 
        Exception? exception)
    {
        const string sql = @"
            UPDATE outbox_messages
            SET processed_on_utc = @ProcessedOnUtc, 
                error = @Error
            WHERE id = @Id
            ";

        await connection.ExecuteAsync(
            sql, 
            new
            {
                outboxMessage.Id,
                ProcessedOnUtc = _dateTimeProvider.UtcNow,
                Error = exception?.ToString()
            }, 
            transaction: transaction);
    }

    private async Task<IReadOnlyList<OutboxMessageResponse>> GetOutboxMessageAsync(
        IDbConnection connection,
        IDbTransaction transaction)
    {
        var sql = $"""
            SELECT id, content
            from outbox_messages
            WHERE processed_on_utc IS NULL
            ORDER BY occurred_on_utc
            LIMIT {_outboxOptions.BatchSize}
            FOR UPDATE
            """;

        var outboxMessages = await connection.QueryAsync<OutboxMessageResponse>(sql, transaction: transaction);

        return outboxMessages.ToList();
    }
}
