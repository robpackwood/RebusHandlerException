namespace Rebus.Shared;

public record RabbitMqConfig(
    string RabbitConnectionString, string RabbitVirtualHost, string RabbitUserName,
    string RabbitPassword, string? RabbitInputQueueName = null)
{
    public bool IsSendOnlyEndpoint = string.IsNullOrWhiteSpace(RabbitInputQueueName);
}