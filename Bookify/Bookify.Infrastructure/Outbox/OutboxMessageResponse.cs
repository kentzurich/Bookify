namespace Bookify.Infrastructure.Outbox;

internal sealed partial class ProcessOutboxMessageJob
{
    internal sealed record OutboxMessageResponse(Guid Id, string Content);
}
