namespace TryingKafka.KafkaService.Models
{
    public enum Status
    {
        OrderSubmitted,
        OrderValidated,
        OrderOutOfStock,
        PaymentProcessed,
        PaymentFailed,
        OrderDispatched
    }
}
