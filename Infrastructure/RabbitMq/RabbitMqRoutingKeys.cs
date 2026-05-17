using Infrastructure.Enums;

namespace Infrastructure.RabbitMq.Messages;

public static class RabbitMqRoutingKeys
{
    public static readonly string TaskRawCreated =
        EventType.IncomingTaskReceived.ToString();

    public static readonly string TaskEnrichedCreated =
        EventType.IncomingTaskProceeded.ToString();

    public const string TaskEnrichmentQueue = "task.enrichment.queue";
}